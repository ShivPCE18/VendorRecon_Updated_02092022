using System;
using System.Collections.Generic;
using PX.Data;
using PX.Data.ReferentialIntegrity.Attributes;
using PX.Objects.AP;
using PX.Objects.CM;
using PX.Objects.CS;
using PX.Objects.GL.FinPeriods;
using VendorRecon_Updated.DAC;

namespace VendorRecon
{
    [Serializable]
    [PXCacheName("Vendor Reconciliation")]
    public class VendorRecon : IBqlTable
    {
        [PXDBString(2, IsFixed = true)]
        [PXStringList(
           new string[]
           {
            ReconciliationConstants.Open,
            ReconciliationConstants.OnHold,
            ReconciliationConstants.Completed,
           },
           new string[]
           {
            VendorRecon_Updated.DAC.Messages.Open,
            VendorRecon_Updated.DAC.Messages.OnHold,
            VendorRecon_Updated.DAC.Messages.Completed,
           }
           )]
        [PXDefault("H")]
        [PXUIField(DisplayName = "Status", Enabled = false)]
        public virtual string PaymentStatus { get; set; }
        public abstract class paymentStatus :
            PX.Data.BQL.BqlString.Field<paymentStatus>
        { }


        #region Adjusted Statement Balance
        private decimal? _adjdAmt = 0;
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Adjusted Statement Balance", Enabled = false)]
        public virtual Decimal? AdjdStmtAmt { get { return _adjdAmt; } set { _adjdAmt = value; } }
        public abstract class adjdStmtAmt : PX.Data.BQL.BqlDecimal.Field<adjdStmtAmt> { }
        #endregion

        #region Total Payment Balance
        private decimal? _totPmtAmt = 0;
        [PXDBDecimal(2)]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Total Payment Balance", Enabled = false)]
        public virtual Decimal? TotPymtAmt { get { return _totPmtAmt; } set { _totPmtAmt = value; } }
        public abstract class totPymtAmt : PX.Data.BQL.BqlDecimal.Field<totPymtAmt> { }
        #endregion


        #region RecordID
        [PXDBIdentity(IsKey = true)]
        public virtual int? RecordID { get; set; }
        public abstract class recordID : PX.Data.BQL.BqlInt.Field<recordID> { }
        #endregion

        #region LineNbr
        [PXDBInt()]
        [PXLineNbr(typeof(VendorReasons.lineNbr))]
        [PXUIField(DisplayName = "Line Nbr.", Visible = false)]
        public virtual int? LineNbr { get; set; }
        public abstract class lineNbr : PX.Data.BQL.BqlInt.Field<lineNbr> { }
        #endregion

        //[PXDBBool()]
        //[PXUIField(DisplayName = "", Enabled = false)]
        //public virtual bool? PaymentStatus { get; set; }
        //public abstract class paymentStatus : PX.Data.BQL.BqlBool.Field<paymentStatus> { }

        #region VendorID
        [PXDBInt(IsKey = true)]
        [PXUIField(DisplayName = "Vendor")]
        [PXSelector(typeof(Search<VendorR.bAccountID>), typeof(VendorR.acctCD), typeof(VendorR.acctName), SubstituteKey = typeof(VendorR.acctCD), DescriptionField = typeof(VendorR.acctName), IsDirty = false)]
        public virtual int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }
        #endregion

        #region unbound Fyear
        public abstract class Fyear : PX.Data.BQL.BqlString.Field<Fyear> { }
        [PXString(255)]
        public virtual string fyear
        {
            get;
            set;
        }
        #endregion

        #region VendorName
        [PXDBString(256, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Vendor Name", Enabled = false)]
        public virtual string VendorName { get; set; }
        public abstract class vendorName : PX.Data.BQL.BqlString.Field<vendorName> { }
        #endregion


        #region Comment
        [PXDBString(30, IsUnicode = true, InputMask = "")]
        [PXUIField(DisplayName = "Comment", Required = true, ErrorHandling = PXErrorHandling.WhenVisible)]
        //[PXDefault(PersistingCheck =PXPersistingCheck.NullOrBlank)]
        public virtual string Comment { get; set; }
        public abstract class comment : PX.Data.BQL.BqlString.Field<comment> { }
        #endregion


        #region PeriodID
        [PXDBString(10, IsKey = true, IsUnicode = true, InputMask = "####-##")]
        [PXUIField(DisplayName = "Financial Period", Required = true, ErrorHandling = PXErrorHandling.WhenVisible, IsDirty = false)]
        public virtual string PeriodID { get; set; }
        public abstract class periodID : PX.Data.BQL.BqlString.Field<periodID> { }
        #endregion


        #region VendorBalance
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Vendor Balance", ErrorHandling = PXErrorHandling.WhenVisible)]
        public virtual Decimal? VendorBalance { get; set; }
        public abstract class vendorBalance : PX.Data.BQL.BqlDecimal.Field<vendorBalance> { }
        #endregion

        #region IsReconciled
        [PXBool()]
        [PXDBCalced(typeof(
Switch<Case<Where<paymentStatus, Equal<Filter.completed>>,
True>, False>), typeof(Boolean))]
        [PXUIField(DisplayName = "Is Reconciled")]
        public virtual bool? IsReconciled { get; set; }
        public abstract class isReconciled : PX.Data.BQL.BqlBool.Field<isReconciled>
        {

        }
        #endregion

        #region Selected
        public abstract class selected : PX.Data.BQL.BqlBool.Field<selected> { }
        [PXBool]
        [PXUIField(DisplayName = "Selected")]
        public virtual bool? Selected { get; set; }
        #endregion

        #region StatementBalance
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Statement Balance")]
        public virtual Decimal? StatementBalance { get; set; }
        public abstract class statementBalance : PX.Data.BQL.BqlDecimal.Field<statementBalance> { }
        #endregion

        #region Difference
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Difference")]
        public virtual Decimal? Difference { get; set; }
        public abstract class difference : PX.Data.BQL.BqlDecimal.Field<difference> { }
        #endregion

        #region ReconItems
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Recon Items")]
        public virtual Decimal? ReconItems { get; set; }
        public abstract class reconItems : PX.Data.BQL.BqlDecimal.Field<reconItems> { }
        #endregion
        #region Currency
        [PXDBString()]
        [PXUIField(DisplayName = "Currency", Enabled = false)]
        public virtual string Currency { get; set; }
        public abstract class currency : PX.Data.BQL.BqlDecimal.Field<currency> { }
        #endregion

        #region Variance
        [PXDBDecimal()]
        [PXDefault(TypeCode.Decimal, "0.00")]
        [PXUIField(DisplayName = "Variance", Enabled = false)]
        public virtual Decimal? Variance { get; set; }
        public abstract class variance : PX.Data.BQL.BqlDecimal.Field<variance> { }
        #endregion

        #region CreatedDateTime
        [PXDBCreatedDateTime()]
        public virtual DateTime? CreatedDateTime { get; set; }
        public abstract class createdDateTime :
        PX.Data.BQL.BqlDateTime.Field<createdDateTime>
        { }
        #endregion
        #region CreatedByID
        [PXDBCreatedByID()]
        public virtual Guid? CreatedByID { get; set; }
        public abstract class createdByID :
        PX.Data.BQL.BqlGuid.Field<createdByID>
        { }
        #endregion
        #region CreatedByScreenID
        [PXDBCreatedByScreenID()]
        public virtual string CreatedByScreenID { get; set; }
        public abstract class createdByScreenID :
        PX.Data.BQL.BqlString.Field<createdByScreenID>
        { }
        #endregion
        #region LastModifiedDateTime
        [PXDBLastModifiedDateTime()]
        public virtual DateTime? LastModifiedDateTime { get; set; }
        public abstract class lastModifiedDateTime :
        PX.Data.BQL.BqlDateTime.Field<lastModifiedDateTime>
        { }
        #endregion
        #region LastModifiedByID
        [PXDBLastModifiedByID()]
        public virtual Guid? LastModifiedByID { get; set; }
        public abstract class lastModifiedByID :
        PX.Data.BQL.BqlGuid.Field<lastModifiedByID>
        { }
        #endregion
        #region LastModifiedByScreenID
        [PXDBLastModifiedByScreenID()]
        public virtual string LastModifiedByScreenID { get; set; }
        public abstract class lastModifiedByScreenID :
        PX.Data.BQL.BqlString.Field<lastModifiedByScreenID>
        { }
        #endregion
        #region Tstamp
        [PXDBTimestamp()]
        public virtual byte[] Tstamp { get; set; }
        public abstract class tstamp : PX.Data.BQL.BqlByteArray.Field<tstamp>
        { }
        #endregion
        #region NoteID
        [PXNote()]
        public virtual Guid? NoteID { get; set; }
        public abstract class noteID : PX.Data.BQL.BqlGuid.Field<noteID> { }
        #endregion
    }

    public class Filter : IBqlTable
    {
        public class completed : Constant<string>
        {
            public completed() : base("C") { }
        }
    }

}