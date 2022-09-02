using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using PX.Common;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using PX.Objects.CR;
using VendorRecon_code.DAC;
using VendorRecon_Updated.DAC;
using VendorRecon0301202211;

namespace VendorRecon
{
    public class VendorReconEntry : PXGraph<VendorReconEntry, VendorRecon>
    {
        //public PXCopyPasteAction<VendorRecon> PXCopyPasteAction;
        public PXSave<VendorRecon> XSave;
        //public PXFirst<VendorRecon> PXFirst;
        //public PXPrevious<VendorRecon> XPrevious;
        //public PXNext<VendorRecon> XNext;
        //public PXLast<VendorRecon> XLast;

        #region Views
        public SelectFrom<VendorRecon>.View MasterView;

        public PXSelect<VendorReasons,
      Where2<Where<VendorReasons.vendorID, Equal<Current<VendorRecon.vendorID>>>,
        And<Where<VendorReasons.finPeriod, Equal<Current<VendorRecon.periodID>>>>>> DetailsView;

        public PXSelect<VendorRecon> NewRevisionPanel;
        public PXSelect<APManualSelection, Where<APManualSelection.vendorID, Equal<Current<VendorRecon.vendorID>>, And<APManualSelection.finPeriod, Equal<Current<VendorRecon.periodID>>>>> ManualSelection;

        //Reconciled invoice 
        public PXSelect<APReconciledInvoice, Where<APReconciledInvoice.vendorID, Equal<Current<VendorRecon.vendorID>>, And<APReconciledInvoice.finPeriod, Equal<Current<VendorRecon.periodID>>>>> ReconciledInvoices;

        public PXSelectJoinOrderBy<APRegister, InnerJoin<VendorR,
            On<VendorR.bAccountID, Equal<APRegister.vendorID>, And<APRegister.docType, Equal<Filters.invoice>>>>, OrderBy<Desc<APRegister.vendorID>>> BalanceSummary;

        //public PXSelect<APInvoice, Where2<Where<APInvoice.vendorID, Equal<Current<VendorRecon.vendorID>>, And<APInvoice.status, Equal<BillsFilters.open>>>,
        //    And<Where<Where2<Where<APInvoice.finPeriodID, LessEqual<Current<VendorRecon.periodID>>, And<APInvoice.finPeriodID, GreaterEqual<Current<VendorRecon.Fyear>>>>,
        //        And<Where<APInvoice.docType, Equal<BillsFilters.invoice>>>>>>>> Bills;


        //public PXSelect<APInvoice,Where<APInvoice.vendorID,Equal<Current<VendorRecon.vendorID>>,And<APInvoice.status,Equal<BillsFilters.open>>>> Bills;
        public PXSelect<APInvoice, Where<APInvoice.status, Equal<BillsFilters.open>>> Bills;
        //public PXSelect<APRegister, Where<APRegister.vendorID, Equal<Current<VendorRecon.vendorID>>, And<APRegister.status, Equal<BillsFilters.open>>>> Register;

        //public PXSelect<APInvoice, Where<APInvoice.vendorID, Equal<VendorRecon.vendorID.FromCurrent>>> Bills;
        #endregion

        #region member variables

        public static decimal reconItemsValue;
        public static decimal reconCreditValue;
        public static decimal reconDebitValue;
        public static List<VendorReasons> manualSelectedReasons = new List<VendorReasons>();
        #endregion


        public static void RemoveChangeID(PXGraph graph, PXAction action, String name)
        {
            PXButtonState astate = action.GetState(null) as PXButtonState;
            if (astate != null)
            {
                PXButtonState bstate = graph.Actions[name].GetState(null) as PXButtonState;
                if (bstate != null && bstate.Menus != null)
                {
                    ButtonMenu[] array = bstate.Menus.Where(b => b.Command != astate.Name).ToArray();
                    graph.Actions[name].SetMenu(array);
                }
            }
        }


        public VendorReconEntry()
        {
            // this.action.AddMenuAction(Pay);
            // RemoveChangeID(this, Save, "Action");
            action.AddMenuAction(Pay);
            reconDebitValue = 0;
            reconCreditValue = 0;

            if (MasterView.Current != null)
            {

            }
        }

        public PXAction<VendorRecon> action;
        [PXUIField(DisplayName = "Actions", MapEnableRights = PXCacheRights.Update, MapViewRights = PXCacheRights.Update)]
        [PXProcessButton(SpecialType = PXSpecialButtonType.ActionsFolder)]
        protected IEnumerable Action(PXAdapter adapter)
        {
            return adapter.Get();
        }

        public virtual void Initialize()
        {
            
        }

        public PXAction<VendorRecon> Pay;
        [PXUIField(DisplayName = "Pay", MapViewRights = PXCacheRights.Select, MapEnableRights = PXCacheRights.Select)]
        [PXProcessButton()]
        public virtual IEnumerable pay(PXAdapter adapter)
        {
            List<VendorRecon> vendorsRecons = new List<VendorRecon>();
            VendorRecon list = MasterView.Current;

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
                                            PXProcessing.SetError<VendorRecon>(0, "This Row Has Pending Print Status.");
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
                        PXUIFieldAttribute.SetWarning<VendorRecon.totPymtAmt>(paymentEntry.Document.Cache, null, $"Zero amount can not be processed.");

                    }

                    if (paymentEntry.Document.Current.UnappliedBal != 0)
                    {
                        PXUIFieldAttribute.SetWarning<VendorRecon.totPymtAmt>(paymentEntry.Document.Cache, null, $"Document is out of balance");
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
                PXUIFieldAttribute.SetWarning<VendorRecon.paymentStatus>(MasterView.Cache, MasterView.Current, list.PaymentStatus.Trim() == "H" ? "Payment status is OnHold." : "Payment status is closed.");

            }
            return adapter.Get();
        }


        #region Match

        /// <summary>
        /// It'll call matchprocess method
        /// </summary>
        public PXAction<VendorRecon> Match;
        [PXUIField(DisplayName = "Match", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXButton()]
        public virtual IEnumerable match(PXAdapter adapter)
        {
            foreach (PXResult<APReconciledInvoice> b in ReconciledInvoices.Select())
            {
                APReconciledInvoice bill = (APReconciledInvoice)b;

                foreach (VendorReasons reason in this.DetailsView.Cache.Cached)
                {
                    if (bill.DocDate == reason.ReasonDate && bill.DocBal == reason.Credit && bill.VendorRef.Trim() == reason.Reference.Trim() && bill.Selected == false && bill.Selected != null && reason.Selected == false)
                    {
                        autoMatched = true;

                        bill.Selected = true;
                        reason.Selected = true;

                        bill.PaymentValue = bill.DocBal;

                        ReconciledInvoices.Cache.Update(bill);
                        DetailsView.Cache.Update(reason);

                        MasterView.Cache.Update(MasterView.Current);
                        this.MasterView.Cache.IsDirty = true;
                    }
                }
            }
            autoMatched = false;
            return adapter.Get();
        }

        #endregion

        #region Create invoice

        /// <summary>
        /// Generating payment and check record then process 
        /// </summary>
        public PXAction<VendorRecon> CreateInvoice;
        [PXUIField(DisplayName = "Create Invoice", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable createInvoice(PXAdapter adapter)
        {
            APInvoiceEntry entry = PXGraph.CreateInstance<APInvoiceEntry>();
            entry.Document.Current = entry.Document.Insert(new APInvoice
            {
                VendorID = MasterView.Current.VendorID
            });

            if (entry != null)
                throw new PXRedirectRequiredException(entry, "Document") { Mode = PXBaseRedirectException.WindowMode.NewWindow };
            else
                return adapter.Get();
        }
        #endregion



        #region Ready for payment 

        /// <summary>
        /// Generating payment and check record then process 
        /// </summary>
        public PXAction<VendorRecon> ReadyForPayment;
        [PXUIField(DisplayName = "Process Payment", MapViewRights = PXCacheRights.Update, MapEnableRights = PXCacheRights.Update)]
        [PXProcessButton()]
        public virtual IEnumerable readyForPayment(PXAdapter adapter)
        {
            List<VendorRecon> vendorsRecons = new List<VendorRecon>();

            bool isMassProcess = adapter.MassProcess;

            if (this.Bills.Select().Count == 0)
            {
                return adapter.Get();
            }

            if (isMassProcess)
                foreach (VendorRecon reason in adapter.Get<VendorRecon>().ToList())
                {
                    if (reason.Selected == true)
                        vendorsRecons.Add(reason);
                }
            else
                vendorsRecons.Add(MasterView.Current);

            PXLongOperation.StartOperation(this, delegate ()
            {
                ProcessPayment(vendorsRecons, isMassProcess);
            });

            return adapter.Get();
        }
        #endregion


        #region Pay

        public static void PayReconciliations(VendorRecon list)
        {
            List<string> generatedCheck = new List<string>();
            var pendingPrint = false;


            var vendorReconEntry = PXGraph.CreateInstance<VendorReconEntry>();
            //----------foreach END------------//
            pendingPrint = false;
            APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();

            //clear graph 
            paymentEntry.Clear();

            if (list.PaymentStatus.Trim() == "H")
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
                                            PXProcessing.SetError<VendorRecon>(0, "This Row Has Pending Print Status.");
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
                        PXUIFieldAttribute.SetWarning<VendorRecon.totPymtAmt>(paymentEntry.Document.Cache, null, $"Zero amount can not be processed.");

                    }

                    if (paymentEntry.Document.Current.UnappliedBal != 0)
                    {
                        PXUIFieldAttribute.SetWarning<VendorRecon.totPymtAmt>(paymentEntry.Document.Cache, null, $"Document is out of balance");
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

        }
        #endregion

        /// <summary>
        /// Payment with mass process
        /// </summary>
        /// <param name="list">Vendor Reconciliations</param>
        /// <param name="isMassProcess">true if mass processed from processing form</param>
        public static void ProcessPayment(List<VendorRecon> list, bool isMassProcess = false)
        {
            List<string> generatedCheck = new List<string>();
            var pendingPrint = false;
            var count = list.Count;

            var vendorReconEntry = PXGraph.CreateInstance<VendorReconEntry>();
            //----------foreach END------------//
            foreach (VendorRecon master in list)
            {
                pendingPrint = false;
                APPaymentEntry paymentEntry = PXGraph.CreateInstance<APPaymentEntry>();
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
                                                PXProcessing.SetError<VendorRecon>(list.IndexOf(master), "This Row Has Pending Print Status.");
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
                            PXUIFieldAttribute.SetWarning<VendorRecon.totPymtAmt>(paymentEntry.Document.Cache, null, $"Zero amount can not be processed.");

                        }

                        if (paymentEntry.Document.Current.UnappliedBal != 0)
                        {
                            PXUIFieldAttribute.SetWarning<VendorRecon.totPymtAmt>(paymentEntry.Document.Cache, null, $"Document is out of balance");
                        }

                        paymentEntry.Actions.PressSave();

                        master.PaymentStatus = ReconciliationConstants.Completed;

                        vendorReconEntry.MasterView.Update(master);

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
                }
                APPrintChecks aPPrintChecks = PXGraph.CreateInstance<APPrintChecks>();


                VendorLocationMaint vendorLocation = PXGraph.CreateInstance<VendorLocationMaint>();
                vendorLocation.Location.Current = PXSelect<Location, Where<Location.bAccountID, Equal<Required<Location.bAccountID>>>>.Select(vendorLocation, list.FirstOrDefault().VendorID, "MAIN");


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

                if (count == 0)
                    throw new PXRedirectRequiredException(aPPrintChecks, true, "Payment Processing");
            }

        }

        #region UnMatchAll
        /// <summary>
        /// It'll call UnmatchProcess method to unmatch all
        /// </summary>
        public PXAction<VendorRecon> UnMatchAll;

        [PXUIField(DisplayName = "Unmatch All", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXButton()]
        public virtual IEnumerable unMatchAll(PXAdapter adapter)
        {
            UnmatchProcess();

            return adapter.Get();
        }
        #endregion


        /// <summary>
        /// Put On hold and enable cache update
        /// </summary>
        public PXAction<VendorRecon> PutOnHold;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Put On Hold")]
        protected virtual IEnumerable putOnHold(PXAdapter adapter)
        {

            if (MasterView.Current != null)
            {
                var row = MasterView.Current;

                PXCache cache = MasterView.Cache;
                row.PaymentStatus = ReconciliationConstants.OnHold;
                //MasterView.Cache.SetValue<VendorRecon.paymentStatus>(MasterView.Current, ReconciliationConstants.OnHold);
                PXUIFieldAttribute.SetEnabled<VendorRecon.vendorID>(cache, row, row.PaymentStatus.Trim() == "H");
                PXUIFieldAttribute.SetEnabled<VendorRecon.periodID>(cache, row, row.PaymentStatus.Trim() == "H");
                PXUIFieldAttribute.SetEnabled<VendorRecon.comment>(cache, row, row.PaymentStatus.Trim() == "H");
                PXUIFieldAttribute.SetEnabled<VendorRecon.statementBalance>(cache, row, row.PaymentStatus.Trim() == "H");


                DetailsView.Cache.AllowInsert = DetailsView.Cache.AllowDelete = DetailsView.Cache.AllowUpdate = row.PaymentStatus.Trim() == "H";
                ReconciledInvoices.Cache.AllowInsert = ReconciledInvoices.Cache.AllowDelete = ReconciledInvoices.Cache.AllowUpdate = row.PaymentStatus.Trim() == "H";

                MasterView.Update(MasterView.Current);
                this.Save.Press();
            }
            return adapter.Get();
        }


        /// <summary>
        /// Release from hold and disable cache update
        /// </summary>
        public PXAction<VendorRecon> ReleaseFromHold;
        [PXButton(CommitChanges = true)]
        [PXUIField(DisplayName = "Release From Hold")]
        protected virtual IEnumerable releaseFromHold(PXAdapter adapter)
        {

            if (MasterView.Current != null)
            {
                var row = MasterView.Current;

                PXCache cache = MasterView.Cache;
                row.PaymentStatus = ReconciliationConstants.Open;
                //MasterView.Cache.SetValue<VendorRecon.paymentStatus>(MasterView.Current, ReconciliationConstants.OnHold);
                PXUIFieldAttribute.SetEnabled<VendorRecon.vendorID>(cache, row, row.PaymentStatus.Trim() == ReconciliationConstants.Open);
                PXUIFieldAttribute.SetEnabled<VendorRecon.periodID>(cache, row, row.PaymentStatus.Trim() == ReconciliationConstants.Open);
                PXUIFieldAttribute.SetEnabled<VendorRecon.comment>(cache, row, row.PaymentStatus.Trim() == ReconciliationConstants.Open);
                PXUIFieldAttribute.SetEnabled<VendorRecon.statementBalance>(cache, row, row.PaymentStatus.Trim() == ReconciliationConstants.Open);


                DetailsView.Cache.AllowInsert = DetailsView.Cache.AllowDelete = DetailsView.Cache.AllowUpdate = row.PaymentStatus.Trim() == ReconciliationConstants.Open;
                ReconciledInvoices.Cache.AllowInsert = ReconciledInvoices.Cache.AllowDelete = ReconciledInvoices.Cache.AllowUpdate = row.PaymentStatus.Trim() == ReconciliationConstants.Open;

                MasterView.Update(MasterView.Current);
                this.Save.Press();
            }
            return adapter.Get();
        }


        /// <summary>
        /// Unmatch all transactions
        /// </summary>
        private void UnmatchProcess()
        {
            //        var x = PXSelect<APReconciledInvoice, Where<APInvoice.vendorID, Equal<Current<VendorRecon.vendorID>>,
            //And<APInvoice.finPeriodID, Equal<Current<VendorRecon.periodID>>>>>.Select(this);

            oldDirt = MasterView.Cache.IsDirty;
            foreach (PXResult<APReconciledInvoice> b in ReconciledInvoices.Select())
            //foreach (APInvoice bill in Bills.Select(MasterView.Current.VendorID, finPeriodID))
            {
                APReconciledInvoice bill = (APReconciledInvoice)b;

                if (bill.Selected == true)
                {
                    autoMatched = true;
                    bill.Selected = false;
                    bill.PaymentValue = 0;
                    ReconciledInvoices.Cache.Update(bill);
                    autoMatched = false;
                }
            }
            foreach (VendorReasons reason in this.DetailsView.Select())
            {
                if (reason.Selected == true)
                {
                    autoMatched = true;
                    reason.Selected = false;
                    reason.PaymentValue = reason.Credit;
                    DetailsView.Cache.Update(reason);

                    autoMatched = false;
                }
            }

        }




        public PXAction<VendorRecon> UploadFileBatch;
        private static bool autoMatched;
        private static bool reasonMatched;
        private static bool invoiceMatched;
        private decimal? totalMSR;
        private decimal? valuePercentage = 100;

        public bool oldDirt { get; private set; }
        public static bool manualMatch { get; set; }


        #region Upload File
        /// <summary>
        /// Upload excel file trasaction to details view
        /// </summary>
        /// <param name="adapter"></param>
        /// <returns></returns>
        [PXUIField(DisplayName = "Upload File", MapEnableRights = PXCacheRights.Select, MapViewRights = PXCacheRights.Select, Visible = true)]
        [PXButton(CommitChanges = true)]
        public virtual IEnumerable uploadFileBatch(PXAdapter adapter)
        {
            VendorReconEntry graph = new VendorReconEntry();

            //this.Persist();
            //Ask user to upload file, continue if it is OK
            if (this.NewRevisionPanel.AskExt() == WebDialogResult.OK)
            {
                //Retreaving file from session by key
                PX.SM.FileInfo info = PXContext.SessionTyped<PXSessionStatePXData>().FileInfo["ImportStatementProtoFile"] as PX.SM.FileInfo;
                //Binary data will be inside FileInfo
                Byte[] bytes = info.BinData;

                using (PX.Data.XLSXReader reader = new XLSXReader(bytes))
                {
                    //Initialising Reader
                    reader.Reset();
                    //Creating a dictionary to find column index by name
                    Dictionary<String, Int32> indexes = reader.IndexKeyPairs.ToDictionary(p => p.Value, p => p.Key);
                    //Skipping first row with collumns names.
                    //reader.MoveNext();

                    while (reader.MoveNext())
                    {
                        if (!string.IsNullOrEmpty(reader.GetValue(indexes["Reconciling Item Type"])) && !string.IsNullOrEmpty(reader.GetValue(indexes["Tran. Date"])))
                        {
                            //VendorReasons row = graph.DetailsView.Insert();
                            var dateStr = reader.GetValue(indexes["Tran. Date"]);
                            VendorReasons row = new VendorReasons
                            {
                                //ReasonDate = DateTime.FromOADate(Convert.ToDouble(reader.GetValue(indexes["Tran. Date"]))),
                                ReasonDate = Convert.ToDateTime(reader.GetValue(indexes["Tran. Date"])),
                                //ReasonDate= DateTime.ParseExact(dateStr.ToString(), "dd/MM/yyyy", CultureInfo.InvariantCulture),
                                ExtRefNbr = reader.GetValue(indexes["Ext. Ref. Nbr."]),
                                Credit = Convert.ToDecimal(reader.GetValue(indexes["Receipt"])),
                                PaymentValue = Convert.ToDecimal(reader.GetValue(indexes["Receipt"])),
                                // Debit = Convert.ToDecimal(reader.GetValue(indexes["Disbursement"])),
                                ReconItemType = reader.GetValue(indexes["Reconciling Item Type"]),
                                Comment = reader.GetValue(indexes["Comment"]),
                                Reference = reader.GetValue(indexes["Ref. Nbr."]),
                                VendorID = MasterView.Current.VendorID,
                                FinPeriod = MasterView.Current.PeriodID,
                                Selected = false,
                            };

                            var data = this.DetailsView.Insert(row);
                            //DetailsView.Cache.IsDirty = true;
                            //this.DetailsView.Cache.Persist(PXDBOperation.Insert);
                        }
                    }

                }

                //Removing file from session to save memory
                PXContext.Session.Remove("ImportStatementProtoFile");
            }
            return adapter.Get();
        }
        #endregion



        /// <summary>
        /// It's calculating main graph values
        /// </summary>
        protected virtual void masterView()
        {
            decimal? totalBal = 0;
            decimal? adjdtotalBal = 0;

            VendorRecon filter = MasterView.Current;
            if (filter != null)
            {
                decimal credit = 0;
                filter.ReconItems = 0; //reset total to zero

                foreach (VendorReasons r in DetailsView.Select())
                {
                    if (r.Selected == true && r.PaymentValue != null)
                    {
                        totalBal += (decimal)r.PaymentValue;
                    }
                    adjdtotalBal += (decimal)r.Credit;
                }

                foreach (VendorReasons row in DetailsView.Select())
                {
                    if (row.Selected != null)
                        if (row.Selected == true)
                        {
                            credit += (decimal)row.Credit;
                        }
                }

                filter.AdjdStmtAmt = adjdtotalBal;
                //filter.TotPymtAmt = totalBal;

                filter.ReconItems += credit;
                filter.Variance = filter.StatementBalance - filter.ReconItems;

                MasterView.Cache.SetValue<VendorRecon.totPymtAmt>(MasterView.Current, totalBal);
                //PXCache mCache = this.Caches[typeof(VendorRecon)];
                //mCache.Update(filter);
                //MasterView.Update(filter);
            }
        }



        //PaymentValue
        protected void APReconciledInvoice_PaymentValue_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            decimal? totAmt = 0;

            var row = (APReconciledInvoice)e.Row;
            if (row != null)
            {
                if (row.PaymentValue > row.DocBal)
                    PXUIFieldAttribute.SetWarning<APReconciledInvoice.paymentValue>(cache, row, string.Format("value can not be greatet than document balance {0}.", row.DocBal));

                //if (MasterView.Current != null && MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.OnHold && DetailsView.Cache.IsDirty == false)
                //    foreach (PXResult<VendorReasons> item in DetailsView.Select())
                //    {
                //        VendorReasons detail = (VendorReasons)item;


                //        //if (detail.Reference != row.InvoiceNbr && detail.Credit == row.CuryOrigDocAmt)
                //        if (detail.Credit == row.DocBal && detail.Selected == true && row.Selected == true)
                //        {
                //            if (row.PaymentValue > row.DocBal)
                //                PXUIFieldAttribute.SetWarning<APReconciledInvoice.paymentValue>(cache, row, string.Format("value can not be greatet than document balance {0}.", row.DocBal));
                //            else
                //            {
                //                DetailsView.Cache.SetValueExt<VendorReasons.paymentValue>(detail, row.PaymentValue);
                //                DetailsView.Update(detail);
                //            }
                //            totAmt += row.DocBal;
                //        }
                //    }

                //MasterView.Current.TotPymtAmt = totAmt;
            }
        }


        protected void APReconciledInvoice_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (APReconciledInvoice)e.Row;

            if (row != null)
            {
                PXUIFieldAttribute.SetEnabled<APReconciledInvoice.refNbr>(cache, row, false);
                PXUIFieldAttribute.SetEnabled<APReconciledInvoice.docType>(cache, row, false);
                PXUIFieldAttribute.SetEnabled<APReconciledInvoice.docBal>(cache, row, false);
                PXUIFieldAttribute.SetEnabled<APReconciledInvoice.vendorRef>(cache, row, false);
                PXUIFieldAttribute.SetEnabled<APReconciledInvoice.status>(cache, row, false);
                PXUIFieldAttribute.SetEnabled<APReconciledInvoice.docDate>(cache, row, false);
                PXUIFieldAttribute.SetEnabled<APReconciledInvoice.refNbr>(cache, row, false);
            }
        }


        protected void VendorRecon_RowSelecting(PXCache cache, PXRowSelectingEventArgs e)
        {
            var row = (VendorRecon)e.Row;

            if (row != null)
            {
                row.fyear = row.PeriodID.Substring(0, 4) + "01";
            }
        }

        protected void VendorRecon_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            try
            {
                decimal? totalBal = 0;
                var row = (VendorRecon)e.Row;
                if (row != null && !string.IsNullOrEmpty(row.PeriodID))
                {
                    if (row.PaymentStatus.Trim() == ReconciliationConstants.OnHold)
                        row.fyear = row.PeriodID.Substring(0, 4) + "01";

                    ReadyForPayment.SetEnabled(MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.Open);
                    PutOnHold.SetVisible(MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.Open);
                    ReleaseFromHold.SetVisible(MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.OnHold);
                    UploadFileBatch.SetVisible(MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.OnHold);
                    Match.SetVisible(MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.OnHold);
                    UnMatchAll.SetVisible(MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.OnHold);

                    //PXUIFieldAttribute.SetEnabled<VendorRecon.vendorID>(cache, row, row.PaymentStatus.Trim() == ReconciliationConstants.Open || row.PaymentStatus.Trim() == ReconciliationConstants.OnHold);
                    //PXUIFieldAttribute.SetEnabled<VendorRecon.periodID>(cache, row, row.PaymentStatus.Trim() == ReconciliationConstants.Open || row.PaymentStatus.Trim() == ReconciliationConstants.OnHold);
                    PXUIFieldAttribute.SetEnabled<VendorRecon.comment>(cache, row, row.PaymentStatus.Trim() == ReconciliationConstants.OnHold);
                    PXUIFieldAttribute.SetEnabled<VendorRecon.statementBalance>(cache, row, row.PaymentStatus.Trim() == ReconciliationConstants.OnHold);

                    DetailsView.Cache.AllowInsert = DetailsView.Cache.AllowDelete = DetailsView.Cache.AllowUpdate = MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.OnHold;
                    ReconciledInvoices.Cache.AllowDelete = ReconciledInvoices.Cache.AllowUpdate = MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.OnHold;
                    ReconciledInvoices.Cache.AllowInsert = false;

                    foreach (APReconciledInvoice r in ReconciledInvoices.Select())
                    {
                        if (r.Selected == true && r.PaymentValue != null)
                        {
                            totalBal += (decimal)r.PaymentValue;
                        }
                    }
                    row.TotPymtAmt = totalBal;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            cache.IsDirty = false;
        }

        protected void VendorRecon_VendorID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (VendorRecon)e.Row;

            if (row != null && row.VendorID != null)
            {
                VendorR vendor = PXSelect<VendorR, Where<VendorR.bAccountID, Equal<Required<VendorRecon.vendorID>>>>.Select(this, row.VendorID);

                if (vendor != null)
                {
                    row.VendorName = vendor.AcctName;
                    row.Currency = vendor.CuryID;
                }
            }
        }

        protected virtual void VendorRecon_StatementBalance_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {

            var row = (VendorRecon)e.Row;
            if (row != null)
            {
                var statementBalance = Convert.ToDecimal(row.VendorBalance) - Convert.ToDecimal(row.StatementBalance);
                row.Difference = statementBalance;
            }
        }

        protected virtual void VendorReasons_RowSelected(PXCache cache, PXRowSelectedEventArgs e)
        {
            var row = (VendorReasons)e.Row;

            if (row != null)
            {
                if (MasterView.Current != null && MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.OnHold)
                {
                    APReconciledInvoice item = ReconciledInvoices.Search<APReconciledInvoice.docBal>(row.Credit);

                    APManualSelection refNbr = ManualSelection.Search<APManualSelection.reasonRef>(row.Credit);

                    if (item != null)
                    {
                        if (row.Selected == true && row.Credit == item.DocBal && item.Selected == true)
                        {
                            row.PaymentValue = item.PaymentValue;
                        }
                    }
                    else if (row.Selected == true && refNbr != null)
                    {
                        APReconciledInvoice aPReconciled = ReconciledInvoices.Search<APReconciledInvoice.refNbr>(refNbr.InvRef);

                        if (aPReconciled != null && aPReconciled.PaymentValue != 0)
                        {
                            row.PaymentValue = ((row.Credit / 100) * (aPReconciled.PaymentValue / aPReconciled.DocBal) * 100);
                        }
                        else
                            row.PaymentValue = row.Credit;

                    }
                    else
                    {
                        row.PaymentValue = row.Credit;
                    }

                    masterView();
                }
            }
        }

        protected virtual void VendorReasons_Credit_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (VendorReasons)e.Row;
            if (row != null)
            {
                if (row.Credit != null)
                    row.PaymentValue = row.Credit;
            }
        }


        #region Working code
        //protected virtual void VendorReasons_Selected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        //{
        //    var row = (VendorReasons)e.Row;
        //    bool valueSet = false;
        //    if (row != null && !autoMatched && !invoiceMatched)
        //    {
        //        if (ReconciledInvoices.Select().Count > 0)
        //        {
        //            APReconciledInvoice item = ReconciledInvoices.Search<APReconciledInvoice.docBal>(row.Credit);

        //            if (item != null)
        //                if (row.Credit == item.DocBal)
        //                {
        //                    reasonMatched = true;
        //                    ReconciledInvoices.Cache.SetValueExt<APReconciledInvoice.selected>(item, row.Selected);

        //                    reasonMatched = false;
        //                    valueSet = true;
        //                }
        //        }

        //        if (!valueSet)
        //            row.Selected = false;
        //    }
        //}
        #endregion

        #region multiple select code
        protected virtual void VendorReasons_Selected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (VendorReasons)e.Row;
            decimal? totalMSR = 0;

            bool valueSet = false;
            if (row != null && !autoMatched && !invoiceMatched)
            {
                APReconciledInvoice item = ReconciledInvoices.Search<APReconciledInvoice.docBal>(row.Credit);

                if (item != null && row.Credit == item.DocBal)
                {
                    reasonMatched = true;
                    item.Selected = row.Selected;

                    if (row.Selected == false)
                    {
                        //Updating right grid payment value
                        row.PaymentValue = row.Credit;
                        item.PaymentValue = 0;
                    }
                    else
                        //Updating left grid payment value
                        item.PaymentValue = item.DocBal;

                    ReconciledInvoices.Update(item);
                    reasonMatched = false;
                }
                else
                {
                    //Multiselection being done here
                    if (row.Selected == true)
                    {
                        manualSelectedReasons.Add(row);
                        foreach (VendorReasons MSR in manualSelectedReasons)
                        {
                            totalMSR += MSR.Credit;
                        }

                        APReconciledInvoice manualSelectInv = ReconciledInvoices.Search<APReconciledInvoice.docBal>(totalMSR);

                        if (manualSelectInv != null)
                        {
                            foreach (VendorReasons MSR in manualSelectedReasons)
                            {
                                APManualSelection aPManualSelection = PXSelect<APManualSelection, Where2<Where<APManualSelection.finPeriod, Equal<Required<APManualSelection.finPeriod>>,
                        And<APManualSelection.vendorID, Equal<Required<APManualSelection.vendorID>>>>, And<Where<APManualSelection.reasonRef, Equal<Required<APManualSelection.reasonRef>>>>>>.Select(this, row.FinPeriod, row.VendorID, row.Credit);
                                if (aPManualSelection == null)
                                    ManualSelection.Update(ManualSelection.Insert(new APManualSelection { InvRef = manualSelectInv.RefNbr, ReasonRef = MSR.Credit, FinPeriod = row.FinPeriod, VendorID = row.VendorID }));

                            }

                            reasonMatched = true;
                            manualSelectInv.Selected = true;
                            manualSelectInv.PaymentValue = manualSelectInv.DocBal;
                            ReconciledInvoices.Update(manualSelectInv);
                            reasonMatched = false;

                            manualSelectedReasons.Clear();
                        }
                    }
                    if (row.Selected == false)
                    {
                        if (manualSelectedReasons.Count > 0 && manualSelectedReasons.Contains(row))
                            manualSelectedReasons.Remove(row);

                        APManualSelection aPManualSelection = ManualSelection.Search<APManualSelection.vendorID, APManualSelection.finPeriod, APManualSelection.reasonRef>(row.VendorID, row.FinPeriod, row.Credit);

                        if (aPManualSelection != null)
                        {

                            //deselect vendor transactions
                            foreach (APManualSelection MS in ManualSelection.Select())
                            {

                                if (aPManualSelection.InvRef.Trim() == MS.InvRef.Trim())
                                {
                                    VendorReasons reason = DetailsView.Search<VendorReasons.credit>(MS.ReasonRef);

                                    if (reason != null)
                                    {
                                        reason.Selected = false;
                                        reason.PaymentValue = row.Credit;
                                        invoiceMatched = true;
                                        DetailsView.Update(reason);
                                        invoiceMatched = false;
                                    }
                                }
                            }

                            APReconciledInvoice manualSelectInv = ReconciledInvoices.Search<APReconciledInvoice.refNbr>(aPManualSelection.InvRef);
                            //deselect invoice 
                            if (manualSelectInv != null)
                            {
                                reasonMatched = true;
                                manualSelectInv.PaymentValue = 0;
                                manualSelectInv.Selected = false;
                                manualSelectedReasons.Clear();
                                ReconciledInvoices.Update(manualSelectInv);
                                reasonMatched = false;
                            }
                        }
                    }
                }

                //if (!valueSet)
                //{
                //    row.Selected = false;
                //}
            }
        }
        #endregion

        #region Event Handlers

        protected void APReconciledInvoice_Selected_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {

            var row = (APReconciledInvoice)e.Row;
            var valueSet = false;

            if (row != null && !autoMatched && !reasonMatched && MasterView.Current != null && MasterView.Current.PaymentStatus.Trim() == ReconciliationConstants.OnHold)
            {

                if (row.Status.Trim() != "N")
                    return;


                //foreach (PXResult<VendorReasons> item in DetailsView.Select())
                //{
                VendorReasons detail = DetailsView.Search<VendorReasons.credit>(row.DocBal);

                //if (detail.Reference != row.InvoiceNbr && detail.Credit == row.CuryOrigDocAmt)

                if (detail != null)
                {
                    invoiceMatched = true;
                    DetailsView.Cache.SetValueExt<VendorReasons.selected>(detail, row.Selected);
                    DetailsView.Cache.SetValueExt<VendorReasons.paymentValue>(detail, row.PaymentValue);

                    valueSet = true;
                    DetailsView.Update(detail);

                    invoiceMatched = false;
                }
                else
                {
                    if (row.Selected == false)
                    {
                        foreach (APManualSelection item1 in ManualSelection.Select())
                        {
                            if (row.RefNbr.Trim() == item1.InvRef)
                            {
                                VendorReasons item2 = DetailsView.Search<VendorReasons.credit>(item1.ReasonRef);
                                if (item2 != null)
                                {
                                    invoiceMatched = true;
                                    item2.Selected = row.Selected;

                                    DetailsView.Update(item2);
                                    invoiceMatched = false;
                                    valueSet = true;
                                }
                            }
                        }
                    }
                }

                if (!valueSet)
                    row.Selected = false;
            }



        }
        #endregion


        protected virtual void VendorRecon_PeriodID_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        {
            var row = (VendorRecon)e.Row;
            decimal? vendorBalance = 0;
            decimal? dueBalance = 0;
            bool partialPaid = false;
            decimal? paidBalance = 0;


            if (cache.GetStatus(row) != PXEntryStatus.Updated && row.PaymentStatus != null)
                //Fetching it's balance amount from vendor
                if (row != null && row.VendorID != null && row.PeriodID != null && row.PaymentStatus.Trim() == ReconciliationConstants.OnHold)
                {
                    row.VendorBalance = 0;

                    var g = PXSelect<APRegister, Where<APRegister.vendorID, Equal<Required<VendorRecon.vendorID>>, And<APRegister.docType.IsEqual<Filters.invoice>>>>.Select(this, row.VendorID);

                    if (cache.GetStatus(row) != PXEntryStatus.Updated)
                    {
                        foreach (APInvoice item in Bills.Select())
                        {

                            int fYear = int.Parse(item.FinPeriodID.Substring(0, 4));
                            int fMonth = int.Parse(item.FinPeriodID.Substring(4, 2));
                            int reconYear = int.Parse(row.PeriodID.Substring(0, 4));
                            int reconMonth = int.Parse(row.PeriodID.Substring(4, 2));


                            if (row.VendorID == item.VendorID && item.Status.Trim() == "N" && fYear == reconYear && item.Status == "N" && fMonth <= reconMonth && item.DocType == "INV" && row.VendorID == item.VendorID)
                            {

                                APReconciledInvoice exists = PXSelect<APReconciledInvoice, Where2<Where<APReconciledInvoice.refNbr, Equal<Required<APReconciledInvoice.refNbr>>,
                                           And<APReconciledInvoice.vendorID, Equal<Required<APReconciledInvoice.vendorID>>>>, And<Where<APReconciledInvoice.finPeriod, Equal<Required<APReconciledInvoice.finPeriod>>>>>>.Select(this, item.RefNbr, item.VendorID, row.PeriodID);

                                APAdjust aPAdjust = PXSelect<APAdjust, Where<APAdjust.adjdRefNbr, Equal<Required<APAdjust.adjdRefNbr>>>>.Select(this, item.RefNbr);

                                if (aPAdjust != null)
                                {
                                    APInvoiceEntry invoice = PXGraph.CreateInstance<APInvoiceEntry>();
                                    invoice.Document.Current = invoice.Document.Search<APInvoice.refNbr>(item.RefNbr);

                                    if (invoice.Document.Current != null)
                                    {
                                        foreach (APAdjust adj in invoice.Adjustments.Select())
                                        {
                                            paidBalance += adj.CuryAdjdAmt;
                                        }
                                    }
                                    partialPaid = true;
                                }

                                if (partialPaid)
                                {
                                    dueBalance = item.CuryOrigDocAmt - paidBalance;
                                    paidBalance = 0;
                                }
                                else
                                {
                                    dueBalance = item.CuryOrigDocAmt;
                                    paidBalance = 0;
                                }

                                if (exists == null)
                                {
                                    APInvoice aPInvoice = PXSelect<APInvoice, Where<APInvoice.refNbr, Equal<Required<APInvoice.refNbr>>>>.Select(this, item.RefNbr);
                                    APReconciledInvoice invoice = new APReconciledInvoice()
                                    {
                                        VendorID = item.VendorID,
                                        DocBal = dueBalance,
                                        DocDate = item.DocDate,
                                        VendorRef = aPInvoice.InvoiceNbr,
                                        DocType = item.DocType,
                                        PaymentValue = 0,
                                        TermsID = aPInvoice.TermsID,
                                        Selected = false,
                                        FinPeriod = row.PeriodID,
                                        Status = item.Status,
                                        RefNbr = item.RefNbr
                                    };

                                    ReconciledInvoices.Cache.Insert(invoice);
                                }
                                //update if exists
                                else if (exists != null && exists.RefNbr.Trim() == item.RefNbr.Trim())
                                {
                                    exists.Status = item.Status;
                                    exists.DocBal = partialPaid == true ? item.CuryOrigDocAmt - paidBalance : item.CuryOrigDocAmt;
                                    exists.PaymentValue = partialPaid == true ? item.CuryOrigDocAmt - paidBalance : item.CuryOrigDocAmt;

                                    ReconciledInvoices.Update(exists);
                                    PXTrace.WriteInformation($"reconciled invoice deleted");
                                }

                                vendorBalance += dueBalance;
                                partialPaid = false;
                            }


                        }
                        row.PaymentStatus = "H";
                        row.VendorBalance = vendorBalance;
                    }

                    cache.IsDirty = false;
                }
        }
    }

    public class BillsFilters : IBqlTable
    {
        public class open : Constant<string>
        {
            public open() : base("N") { }
        }
        public class fiyear : Constant<string>
        {
            public fiyear() : base(DateTime.Now.ToString("yyyy") + "01") { }
        }
        public class invoice : Constant<string>
        {
            public invoice() : base("INV") { }
        }
    }


    public class MatchedInvoice : IBqlTable
    {

        #region RefNbr
        [PXString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Ref Nbr")]
        public virtual string RefNbr { get; set; }
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        #endregion


        #region DocBal
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Document Balance")]
        public virtual Decimal? DocBal { get; set; }
        public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
        #endregion
    }
    public class MatchedReasons : IBqlTable
    {

        #region RefNbr
        [PXString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Ref Nbr")]
        public virtual string RefNbr { get; set; }
        public abstract class refNbr : PX.Data.BQL.BqlString.Field<refNbr> { }
        #endregion

        #region ResNbr
        [PXString(15, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "ReS Nbr")]
        public virtual string ResNbr { get; set; }
        public abstract class resNbr : PX.Data.BQL.BqlString.Field<resNbr> { }
        #endregion

        #region DocBal
        [PXDecimal()]
        [PXUIField(DisplayName = "Statement Balance")]
        public virtual Decimal? DocBal { get; set; }
        public abstract class docBal : PX.Data.BQL.BqlDecimal.Field<docBal> { }
        #endregion
    }

}