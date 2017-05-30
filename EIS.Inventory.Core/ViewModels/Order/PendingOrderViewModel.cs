namespace EIS.Inventory.Core.ViewModels
{
    public class PendingOrderViewModel
    {
        public int EisOrderId { get; set; }
        public string OrderId { get; set; }
        public string Marketplace { get; set; }
        public int Quantity { get; set; }
    }
}
