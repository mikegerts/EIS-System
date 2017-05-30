
namespace EIS.Inventory.Shared.ViewModels
{
    public class ItemChargeDto
    {
        public string OrderItemCode { get; set; }
        public string Type { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
    }
}
