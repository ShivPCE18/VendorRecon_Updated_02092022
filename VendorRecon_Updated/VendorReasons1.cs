using System;
using PX.Data;
using PX.Data.BQL.Fluent;
using PX.Objects.AP;
using VendorRecon0301202211;

namespace VendorRecon
{
    [Serializable]
    [PXCacheName("VendorReasons")]
    public class VendorReasons : IBqlTable
    {
        #region RecordID
        [PXDBIdentity(IsKey =true)]
        public virtual int? RecordID { get; set; }
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }

        #endregion
        #region ExtRefNbr
        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Ext. Ref. Nbr.")]
        public virtual string ExtRefNbr { get; set; }
        public abstract class extRefNbr : PX.Data.BQL.BqlString.Field<extRefNbr> { }
        #endregion

        #region UsrPaymentValue
        private decimal? _pmtAmt = 0;
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Payment Value", Enabled = false)]
        public virtual decimal? PaymentValue { get { return _pmtAmt; } set { _pmtAmt = value; } }
        public abstract class paymentValue : PX.Data.BQL.BqlDecimal.Field<paymentValue> { }
        #endregion

        #region Selected
        [PXDBBool()]
        [PXUIField(DisplayName = "")]
        [PXDefault(false)]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region VendorID
        [PXDBInt(IsKey =true)]
      [PXDefault(typeof( Current<VendorRecon.vendorID>))]
        public virtual int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        #endregion

        #region LineNbr
        [PXDBInt()]        
        [PXUIField(DisplayName = "Line Nbr", Visible = false)]
        //[PXDBDefault(typeof(VendorRecon.lineNbr))]
        [PXLineNbr(typeof(VendorRecon.lineNbr))]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : IBqlField { }
        #endregion

        #region ReconItemType
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Reconciling Item Type")]
        [PXSelector(typeof(VendorReasonType.reconReason), SubstituteKey = typeof(VendorReasonType.reconReason), DescriptionField = typeof(VendorReasonType.reconReason))]
        public virtual string ReconItemType { get; set; }
        public abstract class reconItemType : PX.Data.BQL.BqlString.Field<reconItemType> { }
        #endregion

        #region ReasonDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Reason Date")]
        public virtual DateTime? ReasonDate { get; set; }
        public abstract class reasonDate : PX.Data.BQL.BqlDateTime.Field<reasonDate> { }
        #endregion

        #region Reference
        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Reference")]
        public virtual string Reference { get; set; }
        public abstract class reference : PX.Data.BQL.BqlString.Field<reference> { }
        #endregion

        #region Comment
        [PXDBString(50, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Comment")]
        public virtual string Comment { get; set; }
        public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
        #endregion

        #region finPeriod
        [PXDBString(50, IsUnicode = true,IsKey =true)]
        [PXParent(typeof(Select<VendorRecon,
        Where<VendorRecon.periodID, Equal<Current<finPeriod>>,
        And<VendorRecon.vendorID, Equal<Current<vendorID>>>>>))]
        [PXDefault(typeof(Current<VendorRecon.periodID>))]
        public virtual string FinPeriod { get; set; }
        public abstract class finPeriod : PX.Data.BQL.BqlString.Field<finPeriod> { }
        #endregion

        #region Credit
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Statement Balance")]
        public virtual Decimal? Credit { get; set; }
        public abstract class credit : PX.Data.BQL.BqlDecimal.Field<credit> { }
        #endregion

    }
}