using System;
using PX.Data;

namespace VendorRecon0301202211
{
    [Serializable]
    [PXCacheName("Vendor Reason")]
    public class VendorReasonType : IBqlTable
    {
        #region VendorReasonID
        [PXDBIdentity(IsKey =true)]
        public virtual int? VendorReasonID { get; set; }
        public abstract class vendorReasonID : PX.Data.BQL.BqlInt.Field<vendorReasonID> { }
        #endregion


        //#region VendorReasonCD
        //[PXDBString(30, IsUnicode = true, InputMask = "",IsKey =true)]
        //[PXUIField(DisplayName = "Vendor Reason CD")]
        //public virtual string VendorReasonCD { get; set; }
        //public abstract class vendorReasonCD : PX.Data.BQL.BqlString.Field<vendorReasonCD> { }
        //#endregion

     

        #region ReconReason
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Name")]
        [PXDefault]
        public virtual string ReconReason { get; set; }
        public abstract class reconReason : PX.Data.BQL.BqlString.Field<reconReason> { }
        #endregion

        #region IsActive
        [PXDBBool()]
        [PXUIField(DisplayName = "Inactive")]
        public virtual bool? IsActive { get; set; }
        public abstract class isActive : PX.Data.BQL.BqlBool.Field<isActive> { }
        #endregion


       

    }



}