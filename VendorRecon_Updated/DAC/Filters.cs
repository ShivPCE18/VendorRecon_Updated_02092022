using PX.Data;
using PX.Objects.AP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VendorRecon_Updated.DAC;

namespace VendorRecon_code.DAC
{
    [PXHidden]
    public class Filters:IBqlTable
    {

        #region MyCustomField
        [PXInt()]
        [PXUIField(DisplayName = "Vendor")]
        [PXSelector(typeof(Search<VendorR.bAccountID>), typeof(VendorR.acctCD), typeof(VendorR.acctName), SubstituteKey = typeof(VendorR.acctCD), DescriptionField = typeof(VendorR.acctName), IsDirty = false)]
        public virtual int? VendorID { get; set; }
        public abstract class vendorID : PX.Data.BQL.BqlInt.Field<vendorID> { }

        #endregion


        public class invoice : PX.Data.BQL.BqlString.Constant<invoice>{
            public invoice() : base(ReconciliationConstants.INV) { }
        }
        
        public class open : PX.Data.BQL.BqlString.Constant<open>
        {
            public open() : base(ReconciliationConstants.Open) { }
        }


        public class completed : PX.Data.BQL.BqlString.Constant<completed>
        {
            public completed() : base(ReconciliationConstants.Completed) { }
        }


        public class onHold : PX.Data.BQL.BqlString.Constant<onHold>
        {
            public onHold() : base(ReconciliationConstants.OnHold) { }
        }
    }
}
