namespace EIS.Inventory.Shared.ViewModels
{
    public class OrderProductListDto
    {
        public int EisOrderId { get; set; }
        public string OrderId { get; set; }
        public string ItemSKU { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public string BuyerName { get; set; }
    }
}
