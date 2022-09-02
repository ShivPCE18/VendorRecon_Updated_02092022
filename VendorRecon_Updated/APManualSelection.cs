using System;
using PX.Data;

namespace VendorRecon0301202211
{
  [Serializable]
  [PXCacheName("APManualSelection")]
  public class APManualSelection : IBqlTable
  {
    #region Rowid
    [PXDBIdentity(IsKey = true)]
    public virtual int? Rowid { get; set; }
    public abstract class rowid : PX.Data.BQL.BqlInt.Field<rowid> { }
    #endregion

    #region ReasonRef
    [PXDBDecimal()]
    [PXUIField(DisplayName = "Reason Ref")]
    public virtual decimal? ReasonRef { get; set; }
    public abstract class reasonRef : PX.Data.BQL.BqlDecimal.Field<reasonRef> { }
    #endregion

    #region InvRef
    [PXDBString(50, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Inv Ref")]
    public virtual string InvRef { get; set; }
    public abstract class invRef : PX.Data.BQL.BqlString.Field<invRef> { }
    #endregion

    #region FinPeriod
    [PXDBString(50, IsUnicode = true, InputMask = "")]
    [PXUIField(DisplayName = "Fin Period")]
    public virtual string FinPeriod { get; set; }
    public abstract class finPeriod : PX.Data.BQL.BqlString.Field<finPeriod> { }
    #endregion

    #region Linenbr
    [PXDBInt()]
    [PXUIField(DisplayName = "Linenbr")]
    public virtual int? Linenbr { get; set; }
    public abstract class linenbr : PX.Data.BQL.BqlInt.Field<linenbr> { }
    #endregion

    #region VendorID
    [PXDBInt()]
    [PXUIField(DisplayName = "Vendor ID")]
    public virtual int? VendorID { get; set; }
    public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
    #endregion
  }
}