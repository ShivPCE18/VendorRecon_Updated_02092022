using System;
using PX.Data;
using PX.Data.BQL.Fluent;
namespace VendorRecon0301202211
{
    public class ReasonMaintEntry : PXGraph<ReasonMaintEntry>
    {
        public PXSave<VendorReasonType> save;
        public PXCancel<VendorReasonType> cancel;
        public PXSelect<VendorReasonType> DetailsView;

        #region Event Handlers
        //protected void VendorRecon_Status_FieldDefaulting(PXCache cache, PXFieldDefaultingEventArgs e)
        //{

        //    var row = (FilterTab)e.Row;
        //    if (row != null) {
        //        e.NewValue = "A";
        //    }

        //}

        //protected void VendorRecon_Name_FieldUpdated(PXCache cache, PXFieldUpdatedEventArgs e)
        //{

        //    var row = (FilterTab)e.Row;
        //    if(row!=null)
        //    if (row.Name != null&& row.Name != "") {
        //            row.VendorReasonCD = row.Name;
        //    }

        //}


        //protected void VendorRecon_RowPersisting(PXCache cache, PXRowPersistingEventArgs e)
        //{

        //    var row = (FilterTab)e.Row;
        //    if (row != null&&row.Status!=null)
        //    {
        //        if (row.Status.Trim() == "A")
        //            row.IsActive = true;
        //        else
        //            row.IsActive = false;
        //    }
            
        //}
        #endregion


    }
}