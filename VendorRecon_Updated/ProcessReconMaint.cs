using System;
using System.Collections;
using PX.Data;
using VendorRecon;

namespace VendorRecon0301202211
{
  public class ProcessReconMaint : PXGraph<ProcessReconMaint>
  {

    public PXCancel<VendorRecon.VendorRecon> Cancel;
    public PXProcessing<VendorRecon.VendorRecon,Where<VendorRecon.VendorRecon.paymentStatus,Equal<False>>> MasterView;

        #region Process Receipts List

        //method is static, list of records from pxprocessing select

        private static void ProcessReceiptList(IList records)

        {

            //here set variable for global error message.

            var globalError = false;

            //here I have a variable for the po receipt nbr to tie to the lines

            int? vendorID = 0;

            //here I create the graph that will do the processing, can be custom graph also

            var graph = CreateInstance<VendorRecon.VendorReconEntry>();

            //here I get a handle to graph extension to be able to use the added methods.

            //now cycle through the list of records and process each.

            foreach (VendorRecon.VendorRecon record in records)

            {

                //here I have a local variable for each line

                var lineError = false;



                //it is also possible to add transaction support here if only needed for the line item

                try

                {

                    //clear the receipt graph of the last record

                    graph.Clear();

                    //assign the new receipt nbr for messages

                    vendorID = record.VendorID;

                    //set the next record to be processed to the current of the graph

                    graph.MasterView.Current = record;

                    //call the process method that I added to the po receipt entry graph

                    graph.ReadyForPayment.PressButton();

                    //then save the results, including the processed flag or condition

                    graph.Save.Press();

                }

      //catch any errors that happen in the receipt graph and format the message here, you can also write code to a log or record extension to fix the error

       catch (Exception e)

                {

                    //set line error to true so will skip the process correct below

                    lineError = true;

                    //set globaError flag to true to get the global message

                    globalError = true;

                    //create a custom error message to post on the grid

                    var message = "Error Processing PO Receipt Transaction: " +vendorID +  ": " +e.Message;

                    //add the custom error message to the grid line PXProcessing.SetError(records.IndexOf(record), message);

                }

                //create a process complete message and assign to the grid line

                var messageTwo = "PO Receipt Transaction: " +vendorID+ " Was Processed.";

                if (!lineError) PXProcessing.SetInfo(records.IndexOf(record), messageTwo);

            }

            //add last create the global error message that displays at the top of the screen

            if (globalError) throw new PXException("At Least One PO Receipt Transaction Was Not Processed.");

        }

        #endregion

        #region Constructor

        public ProcessReconMaint()

        {

            MasterView.SetProcessDelegate(ProcessReceiptList);

            //Processing.SetSelected();

        }

        #endregion

        #region Overridden Properties

        public override bool IsDirty => false;

        #endregion


    }
}