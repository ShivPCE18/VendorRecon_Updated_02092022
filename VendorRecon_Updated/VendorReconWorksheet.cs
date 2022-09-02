using System;
using System.Collections.Generic;
using System.Linq;
using PX.Data;
using PX.Objects.AP;
using PX.Objects.IN;
using VendorRecon;
using VendorRecon_Updated.DAC;
using PX.Objects.CR;
using System.Collections;
using VendorRecon_code.DAC;

namespace VendorRecon0301202211
{
    public class VendorReconWorksheet : PXGraph<VendorReconWorksheet>
    {

        public PXProcessingJoin<VendorRecon.VendorRecon, InnerJoin<APInvoice, On<VendorRecon.VendorRecon.vendorID, Equal<APInvoice.vendorID>>>> MasterView;
        [PXFilterable]
        public PXFilteredProcessing<VendorRecon.VendorRecon, Filters, Where2<
                Where<Filters.vendorID.FromCurrent, IsNull>,
                 Or<Where<VendorRecon.VendorRecon.vendorID, Equal<Filters.vendorID.FromCurrent>>>>, OrderBy<Asc<VendorRecon.VendorRecon.periodID>>> DetailsView;

        private static string billNbr;
        private static string displayRefNbr;

        public VendorReconWorksheet()
        {
            MasterView.SetSelected<VendorRecon.VendorRecon.selected>();
        }


        public PXAction<VendorRecon.VendorRecon> Pay;
        [PXUIField(DisplayName = "Pay", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public virtual IEnumerable pay(PXAdapter adapter)
        {
            List<VendorRecon.VendorRecon> vendorsRecons = new List<VendorRecon.VendorRecon>();
            VendorRecon.VendorRecon list = MasterView.Current;

            bool isMassProcess = adapter.MassProcess;

            List<string> generatedCheck = new List<string>();
            var pendingPrint = false;


            var vendorReconEntry = PXGraph.CreateInstance<VendorReconEntry>();
            //----------foreach END------------//
            pendingPrint = false;
            APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();

            //clear graph 
            paymentEntry.Clear();

            if (list.PaymentStatus.Trim() == "O")
            {

                vendorReconEntry.MasterView.Current = list;
                APInvoiceEntry invoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();

                paymentEntry.Document.Current = paymentEntry.Document.Insert(new APPayment
                {
                    VendorID = list.VendorID
                });

                paymentEntry.Document.Current.PrintCheck = true;

                paymentEntry.Document.Update(paymentEntry.Document.Current);

                // Vendor Invoices 
                foreach (PXResult<APReconciledInvoice> b in vendorReconEntry.ReconciledInvoices.Select())
                {

                    //Vendor uploaded transactions
                    foreach (PXResult<VendorReasons> item in vendorReconEntry.DetailsView.Select())
                    {

                        VendorReasons reasons = (VendorReasons)item;
                        APReconciledInvoice bill = (APReconciledInvoice)b;

                        APAdjust aPAdjust = PXSelect<APAdjust, Where<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>>>.Select(vendorReconEntry, bill.RefNbr);

                        ///Checking & adding selected bills 
                        if (bill.Status.Trim() == "N" && bill.Selected == true && reasons.Selected == true && bill.DocBal == reasons.Credit)
                        {
                            invoiceEntry.Clear();
                            invoiceEntry.Document.Current = invoiceEntry.Document.Search<APInvoice.refNbr>(bill.RefNbr);

                            if (invoiceEntry.Document.Current != null)
                            {
                                foreach (PXResult<APInvoiceEntry.APAdjust, APPayment> adjustment in invoiceEntry.Adjustments.Select())
                                {
                                    APInvoiceEntry.APAdjust aP1 = (APInvoiceEntry.APAdjust)adjustment;
                                    if (aP1 != null)
                                    {
                                        if (aP1.DisplayStatus.Trim() == "G")
                                        {
                                            PXProcessing.SetError<VendorRecon.VendorRecon>(0, "This Row Has Pending Print Status.");
                                            pendingPrint = true;

                                        }
                                    }
                                }

                            }

                            if (pendingPrint)
                            {
                                invoiceEntry.Clear();
                                continue;
                            }

                            // inserting record to payment entry 
                            APAdjust aP = paymentEntry.Adjustments.Insert(new APAdjust
                            {
                                AdjdRefNbr = bill.RefNbr,
                            });

                            if (aP != null)
                            {
                                aP.CuryAdjgAmt = bill.PaymentValue;
                                paymentEntry.Adjustments.Update(aP);
                            }
                        }
                    }
                }

                if (paymentEntry.Document.Current != null)
                {
                    paymentEntry.Document.Cache.SetValueExt<APPayment.curyOrigDocAmt>(paymentEntry.Document.Current, paymentEntry.Document.Current.CuryApplAmt);

                    paymentEntry.Document.Update(paymentEntry.Document.Current);

                    if (Convert.ToInt32(paymentEntry.Document.Current.ApplAmt) == 0)
                    {
                        PXUIFieldAttribute.SetWarning<VendorRecon.VendorRecon.totPymtAmt>(paymentEntry.Document.Cache, null, $"Zero amount can not be processed.");

                    }

                    if (paymentEntry.Document.Current.UnappliedBal != 0)
                    {
                        PXUIFieldAttribute.SetWarning<VendorRecon.VendorRecon.totPymtAmt>(paymentEntry.Document.Cache, null, $"Document is out of balance");
                    }

                    paymentEntry.Actions.PressSave();

                    list.PaymentStatus = ReconciliationConstants.Completed;

                    vendorReconEntry.MasterView.Update(list);

                    vendorReconEntry.Save.Press();

                    if (paymentEntry.Adjustments.Select().Count != 0)
                    {
                        generatedCheck.Add(paymentEntry.Document.Current.RefNbr);
                    }
                }


                //----------foreach END------------//

                if (paymentEntry.Document.Current != null)
                {
                    if (paymentEntry.Document.Current.Status.Trim() == "H")
                    {
                        paymentEntry.releaseFromHold.Press();
                        // Saving payment graph
                        paymentEntry.Actions.PressSave();
                    }
                }


                APPrintChecks aPPrintChecks = PXGraph.CreateInstance<APPrintChecks>();


                VendorLocationMaint vendorLocation = PXGraph.CreateInstance<VendorLocationMaint>();
                vendorLocation.Location.Current = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>>>.Select(vendorLocation, list.VendorID, "MAIN");


                aPPrintChecks.Filter.Current = new PrintChecksFilter { PayTypeID = "CHECK", PayAccountID = vendorLocation.Location.Current.VCashAccountID };

                foreach (string checkNbr in generatedCheck)
                {
                    foreach (PXResult<APPayment, Vendor> item in aPPrintChecks.APPaymentList.Select())
                    {
                        APPayment paymentRow = (APPayment)item;
                        if (paymentRow != null && paymentRow.RefNbr.Trim() == checkNbr.Trim())
                        {
                            paymentRow.Selected = true;
                            aPPrintChecks.APPaymentList.Update(paymentRow);
                        }
                    }
                }
            }
            else
            {
                PXUIFieldAttribute.SetWarning<VendorRecon.VendorRecon.paymentStatus>(MasterView.Cache, MasterView.Current, list.PaymentStatus.Trim() == "H" ? "Payment status is OnHold." : "Payment status is closed.");

            }
            return adapter.Get();
        }

        public static void ProcessPayment(List<VendorRecon.VendorRecon> list, VendorReconEntry vendorReconEntry, bool isMassProcess = false)
        {
            List<string> generatedCheck = new List<string>();
            var pendingPrint = false;
            var count = list.Count;

            if (isMassProcess)
            {
                //var vendorReconEntry = PXGraph.CreateInstance<VendorReconEntry>();

                APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
                //----------foreach END------------//
                foreach (VendorRecon.VendorRecon master in list)
                {

                    // failed bill number
                    billNbr = "";

                    // make true when one of bill has pending check to print 
                    pendingPrint = false;

                    // When bill is has open status
                    if (master.PaymentStatus.Trim() == "O")
                    {
                        //clear graph 
                        paymentEntry.Clear();

                        vendorReconEntry.MasterView.Current = master;

                        //PXResult<VendorReasons> _reasons= PXSelect<VendorReasons, Where<VendorReasons.vendorID, Equal<Required<VendorReasons.vendorID>>,
                        //     And<VendorReasons.finPeriod,Equal<Required<VendorReasons.finPeriod>>>>>.Select(vendorReconEntry, master.VendorID, master.PeriodID);

                        APInvoiceEntry invoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();

                        paymentEntry.Document.Current = paymentEntry.Document.Insert(new APPayment
                        {
                            VendorID = master.VendorID
                        });

                        paymentEntry.Document.Current.PrintCheck = true;

                        paymentEntry.Document.Update(paymentEntry.Document.Current);

                        foreach (PXResult<APReconciledInvoice> b in vendorReconEntry.ReconciledInvoices.Select())
                        {
                            //VendorReasons reasons = (VendorReasons)item;
                            APReconciledInvoice bill = (APReconciledInvoice)b;

                            ///Checking & adding selected bills 
                            //if (bill.Status.Trim() == "N" && bill.Selected == true && reasons.Selected == true && bill.DocBal == reasons.Credit)
                            if (bill.Status.Trim() == "N" && bill.Selected == true)
                            {

                                VendorReasons reasons = PXSelect<VendorReasons, Where2<Where<VendorReasons.vendorID, Equal<Required<VendorReasons.vendorID>>, And<VendorReasons.finPeriod, Equal<Required<VendorReasons.finPeriod>>>>, And<Where<VendorReasons.credit, Equal<Required<VendorReasons.credit>>, And<VendorReasons.selected, Equal<True>>>>>>.Select(vendorReconEntry, bill.VendorID, bill.FinPeriod, bill.DocBal);

                                invoiceEntry.Clear();
                                invoiceEntry.Document.Current = invoiceEntry.Document.Search<APInvoice.refNbr>(bill.RefNbr);


                                //checking if bill had pending print payment
                                if (reasons != null)
                                    if (invoiceEntry.Document.Current != null)
                                    {
                                        foreach (PXResult<APInvoiceEntry.APAdjust, APPayment> adjustment in invoiceEntry.Adjustments.Select())
                                        {
                                            APInvoiceEntry.APAdjust aP1 = (APInvoiceEntry.APAdjust)adjustment;
                                            if (aP1 != null)
                                            {
                                                if (aP1.DisplayStatus.Trim() == "G")
                                                {
                                                    displayRefNbr = aP1.DisplayRefNbr;
                                                    pendingPrint = true;
                                                    billNbr = bill.RefNbr;
                                                    break;
                                                }
                                            }
                                        }

                                    }

                                // When pending for print already generated check 
                                if (pendingPrint)
                                {
                                    paymentEntry.Clear();
                                    invoiceEntry.Clear();
                                }

                                // inserting record to payment entry 
                                if (!pendingPrint)
                                {
                                    APAdjust aP = paymentEntry.Adjustments.Insert(new APAdjust
                                    {
                                        AdjdRefNbr = bill.RefNbr,
                                    });

                                    if (aP != null)
                                    {
                                        aP.CuryAdjgAmt = bill.PaymentValue;
                                        paymentEntry.Adjustments.Update(aP);
                                    }

                                    billNbr = bill.RefNbr;
                                }
                            }
                        }

                        //Filling applied balance
                        if (paymentEntry.Document.Current != null && !pendingPrint && Convert.ToInt32(paymentEntry.Document.Current.CuryApplAmt) != 0)
                        {
                            paymentEntry.Document.Cache.SetValueExt<APPayment.curyOrigDocAmt>(paymentEntry.Document.Current, paymentEntry.Document.Current.CuryApplAmt);

                            paymentEntry.Document.Update(paymentEntry.Document.Current);


                            if (Convert.ToInt32(paymentEntry.Document.Current.UnappliedBal) != 0)
                            {
                                PXProcessing.SetWarning(list.IndexOf(master), $"The Document is out of balance");
                            }

                            // Saving payment graph
                            paymentEntry.Actions.PressSave();

                            master.PaymentStatus = ReconciliationConstants.Completed;

                            vendorReconEntry.MasterView.Update(master);
                            vendorReconEntry.Save.Press();


                            if (paymentEntry.Document.Current.Status.Trim() == ReconciliationConstants.OnHold)
                            {
                                paymentEntry.releaseFromHold.Press();
                                // Saving payment graph
                                paymentEntry.Actions.PressSave();
                            }

                            generatedCheck.Add(paymentEntry.Document.Current.RefNbr);
                            PXProcessing.SetInfo(list.IndexOf(master), string.Format("The payment application {0} generated.", paymentEntry.Document.Current.RefNbr));
                            count--;
                        }
                        else
                        {
                            //generatedCheck.Add(paymentEntry.Document.Current.RefNbr);

                            if (pendingPrint)
                                PXProcessing.SetWarning(list.IndexOf(master), string.Format("The bill number {0} has pending print status with reference number {1} .", billNbr, displayRefNbr));
                            else if (!string.IsNullOrEmpty(billNbr))
                                PXProcessing.SetWarning(list.IndexOf(master), string.Format("The payment processing failed for bill number {0}, vendor : {1} .", billNbr, master.VendorName));
                            else if (Convert.ToInt32(paymentEntry.Document.Current.CuryApplAmt) == 0)
                                PXProcessing.SetError(list.IndexOf(master), string.Format("Zero amount can not be processed."));
                        }
                    }
                }
                //----------foreach END------------//

                APPrintChecks aPPrintChecks = PXGraph.CreateInstance<APPrintChecks>();
                VendorLocationMaint vendorLocation = PXGraph.CreateInstance<VendorLocationMaint>();
                vendorLocation.Location.Current = PXSelect<PX.Objects.CR.Location, Where<PX.Objects.CR.Location.bAccountID, Equal<Required<PX.Objects.CR.Location.bAccountID>>>>.Select(vendorLocation, list.FirstOrDefault().VendorID, "MAIN");


                aPPrintChecks.Filter.Current = new PrintChecksFilter { PayTypeID = "CHECK", PayAccountID = vendorLocation.Location.Current.VCashAccountID };

                foreach (string checkNbr in generatedCheck)
                {
                    foreach (PXResult<APPayment, Vendor> item in aPPrintChecks.APPaymentList.Select())
                    {
                        APPayment paymentRow = (APPayment)item;
                        if (paymentRow != null && paymentRow.RefNbr.Trim() == checkNbr.Trim())
                        {
                            paymentRow.Selected = true;
                            aPPrintChecks.APPaymentList.Update(paymentRow);
                        }
                    }
                    //aPPrintChecks.APPaymentList.Insert(new APPayment { RefNbr = checkNbr, Selected = true });
                }

                if (count == 0)
                {
                    PXProcessing.SetProcessed<VendorRecon.VendorRecon>();
                    throw new PXRedirectRequiredException(aPPrintChecks, true, "Processing");
                }
            }
        }

        public static void Process(List<VendorRecon.VendorRecon> list)
        {
            var pendingPrint = false;

            foreach (VendorRecon.VendorRecon master in list)
            {
                if (master.Selected == true)
                {

                    var vendorReconEntry = PXGraph.CreateInstance<VendorReconEntry>();

                    //----------foreach END------------//
                    APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
                    pendingPrint = false;
                    // When open 
                    if (master.PaymentStatus.Trim() == "O")
                    {
                        //clear graph 
                        paymentEntry.Clear();

                        vendorReconEntry.MasterView.Current = master;
                        APInvoiceEntry invoiceEntry = PXGraph.CreateInstance<APInvoiceEntry>();

                        paymentEntry.Document.Current = paymentEntry.Document.Insert(new APPayment
                        {
                            VendorID = master.VendorID
                        });

                        paymentEntry.Document.Current.PrintCheck = true;

                        paymentEntry.Document.Update(paymentEntry.Document.Current);


                        foreach (PXResult<APReconciledInvoice> b in vendorReconEntry.ReconciledInvoices.Select())
                        {
                            foreach (PXResult<VendorReasons> item in vendorReconEntry.DetailsView.Select())
                            {

                                VendorReasons reasons = (VendorReasons)item;
                                APReconciledInvoice bill = (APReconciledInvoice)b;

                                ///Checking & adding selected bills 
                                if (bill.Status.Trim() == "N" && bill.Selected == true && reasons.Selected == true && bill.DocBal == reasons.Credit)
                                {
                                    invoiceEntry.Clear();
                                    invoiceEntry.Document.Current = invoiceEntry.Document.Search<APInvoice.refNbr>(bill.RefNbr);

                                    if (invoiceEntry.Document.Current != null)
                                    {
                                        foreach (PXResult<APInvoiceEntry.APAdjust, APPayment> adjustment in invoiceEntry.Adjustments.Select())
                                        {
                                            APInvoiceEntry.APAdjust aP1 = (APInvoiceEntry.APAdjust)adjustment;
                                            if (aP1 != null)
                                            {
                                                if (aP1.DisplayStatus.Trim() == "G")
                                                {
                                                    PXProcessing.SetError<VendorRecon.VendorRecon>(list.IndexOf(master), "This Row Has Pending Print Status.");
                                                    pendingPrint = true;

                                                }
                                            }
                                        }

                                    }

                                    if (pendingPrint)
                                    {
                                        invoiceEntry.Clear();
                                        continue;
                                    }
                                    // inserting record to payment entry 
                                    APAdjust aP = paymentEntry.Adjustments.Insert(new APAdjust
                                    {
                                        AdjdRefNbr = bill.RefNbr,
                                    });

                                    if (aP != null)
                                    {
                                        aP.CuryAdjgAmt = bill.PaymentValue;
                                        paymentEntry.Adjustments.Update(aP);
                                    }
                                }
                            }
                        }

                        //Filling applied balance
                        if (paymentEntry.Document.Current != null)
                        {
                            paymentEntry.Document.Cache.SetValueExt<APPayment.curyOrigDocAmt>(paymentEntry.Document.Current, paymentEntry.Document.Current.CuryApplAmt);

                            paymentEntry.Document.Update(paymentEntry.Document.Current);


                            if (paymentEntry.Document.Current.UnappliedBal != 0)
                            {
                                PXTrace.WriteInformation($"Document is out of balance");
                                PXProcessing.SetError(list.IndexOf(master), $"Document is out of balance");
                            }

                            // Saving payment graph
                            paymentEntry.Actions.PressSave();

                            master.PaymentStatus = ReconciliationConstants.Completed;

                            vendorReconEntry.MasterView.Update(master);

                            vendorReconEntry.Save.Press();

                            PXProcessing.SetProcessed<VendorRecon.VendorRecon>();
                        }
                    }

                    if (paymentEntry.Document.Current != null)
                    {
                        if (paymentEntry.Document.Current.Status.Trim() == "H")
                        {
                            paymentEntry.releaseFromHold.Press();
                            // Saving payment graph
                            paymentEntry.Actions.PressSave();
                        }

                    }
                    //----------foreach END------------//


                }
            }
        }
        protected virtual void _(Events.RowSelected<VendorRecon.VendorRecon> e)
        {
            //DetailsView.SetProcessWorkflowAction<VendorReconEntry>(
            //g => g.ReadyForPayment);

            DetailsView.SetProcessDelegate(
                delegate (List<VendorRecon.VendorRecon> list)
                {
                    VendorReconEntry graph = CreateInstance<VendorReconEntry>();
                    ProcessPayment(list, graph, true);
                }
            );
        }

        [Serializable]
        public class MasterTable : IBqlTable
        {

        }

        [Serializable]
        public class DetailsTable : IBqlTable
        {

        }


    }
}