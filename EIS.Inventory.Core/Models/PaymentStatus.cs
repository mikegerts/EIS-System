using System.ComponentModel;

namespace EIS.Inventory.Core.Models
{
    public enum PaymentStatus
    {
        All = -1,

        [Description("Not Paid")]
        NotPaid,

        Paid,

        [Description("Partially Paid")]
        PartiallyPaid,
    }
}
