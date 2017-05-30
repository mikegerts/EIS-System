
namespace EIS.Inventory.Core.ViewModels
{
    public class UserPreferenceFeed
    {
        public string DefaultPayPalEmaillAddress { get; set; }
        public bool PayPalAlwaysOn {get;set;}
        public bool PayPalPreferred {get;set;}
        public bool OutOfStockControlPreference { get; set; }

        // helper properties
        public bool isPayPalAlwaysOn { get; set; }
        public bool isPayPalPreferred { get; set; }
        public bool isOutOfStockControlPreference { get; set; }
    }
}
