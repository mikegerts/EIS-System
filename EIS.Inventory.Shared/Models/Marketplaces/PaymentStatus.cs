using System.ComponentModel;

namespace EIS.Inventory.Shared.Models
{
    public enum PaymentStatus
    {
        All = -1,

        [Description("Not Paid")]
        NotPaid = 0,

        Paid = 1,

        [Description("Partially Paid")]
        PartiallyPaid = 2,
    }
}
