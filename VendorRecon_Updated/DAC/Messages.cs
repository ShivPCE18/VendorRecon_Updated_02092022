using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendorRecon_Updated.DAC
{
    public static class Messages
    {
        //Reconciliation Payment status item types
        public const string Open = "Open";
        public const string Completed = "Completed";
        public const string OnHold = "On Hold";

        public const string Bill = "Bill";

        // Bill message
        public const string InvOpen = "Open";
        public const string InvClosed = "Closed";
        
    }
}
