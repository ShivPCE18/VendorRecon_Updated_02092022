using System;
using PX.Data;
using VendorRecon_Updated.DAC;

namespace VendorRecon
{
    [Serializable]
    [PXCacheName("APReconciledInvoice")]
    public class APReconciledInvoice : IBqlTable
    {
        #region Apid
        [PXDBIdentity(IsKey =true)]
        public virtual int? Apid { get; set; }
        public abstract class apid : PX.Data.BQL.BqlInt.Field<apid> { }
        #endregion

        #region DocType
        [PXDBString(3, IsFixed = true, InputMask = "")]
        [PXStringList(
           new string[]
           {
            ReconciliationConstants.INV,
           },
           new string[]
           {
            Messages.Bill,
           }
           )]
        [PXUIField(DisplayName = "Type")]
        public virtual string DocType { get; set; }
        public abstract class docType : PX.Data.BQL.BqlString.Field<docType> { }
        #endregion

        #region Status
        [PXDBString(3, IsFixed = true)]
        [PXStringList(
           new string[]
           {
            ReconciliationConstants.InvOpen,
            ReconciliationConstants.InvClosed,
           },
           new string[]
           {
            Messages.InvOpen,
            Messages.InvClosed,
           }
           )]
        [PXUIField(DisplayName = "Status")]
        public virtual string Status { get; set; }
        public abstract class status : PX.Data.BQL.BqlString.Field<status> { }
        #endregion

        #region VendorRef
        [PXDBString(100, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Vendor Ref")]
        public virtual string VendorRef { get; set; }
        public abstract class vendorRef : PX.Data.BQL.BqlString.Field<vendorRef> { }
        #endregion

        #region FinPeriod
        [PXDBString(15,IsKey =true, IsUnicode = true, InputMask = "")]
        [PXParent(typeof(Select<VendorRecon,
        Where<VendorRecon.periodID, Equal<Current<finPeriod>>,
        And<VendorRecon.vendorID, Equal<Current<vendorID>>>>>))]
        [PXUIField(DisplayName = "Fin Period")]
        public virtual string FinPeriod { get; set; }
        public abstract class finPeriod : PX.Data.BQL.BqlString.Field<finPeriod> { }
        #endregion

        #region RefNbr
        [PXDBString(15, IsUnicode = true, InputMask = "")]
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

        #region DocDate
        [PXDBDate()]
        [PXUIField(DisplayName = "Date")]
        public virtual DateTime? DocDate { get; set; }
        public abstract class docDate : PX.Data.BQL.BqlDateTime.Field<docDate> { }
        #endregion

        #region TermsID
        [PXDBString(10, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Terms ID")]
        public virtual string TermsID { get; set; }
        public abstract class termsID : PX.Data.BQL.BqlString.Field<termsID> { }
        #endregion

        #region LineNbr
        [PXDBInt()]        
        [PXLineNbr(typeof(VendorRecon.lineNbr))]
        [PXUIField(DisplayName = "Line Nbr", Visible = false)]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        #region VendorID
        [PXDBInt(IsKey =true)]
        [PXUIField(DisplayName = "Vendor ID")]
        public virtual int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        #endregion

        #region Selected
        [PXDBBool()]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        #endregion

        #region PaymentValue
        private decimal? _usrPmtAmt = 0;
        [PXDBDecimal()]
        [PXUIField(DisplayName = "Payment Value")]
        public virtual Decimal? PaymentValue { get { return _usrPmtAmt; } set { _usrPmtAmt = value; } }
        public abstract class paymentValue : PX.Data.BQL.BqlDecimal.Field<paymentValue> { }
        #endregion
    }
}