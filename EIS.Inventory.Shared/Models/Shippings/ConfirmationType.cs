using System.ComponentModel;

namespace EIS.Inventory.Shared.Models
{
    public enum ConfirmationType
    {
        [Description("No Confirmation")]
        NoConfirmation = 0,
        
        Delivery = 1,
        
        Signature = 2,

        [Description("Adult Signature")]
        AdultSignature = 3
    }
}
