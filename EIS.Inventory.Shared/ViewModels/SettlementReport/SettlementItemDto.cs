using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class SettlementItemDto
    {
        public SettlementItemDto()
        {
            ItemPrices = new List<ItemChargeDto>();
            ItemFees = new List<ItemChargeDto>();
        }
        public string OrderId { get; set; }
        public string OrderItemCode { get; set; }
        public string SKU { get; set; }
        public int Quantity { get; set; }
        public string MerchantAdjustmentItemId { get; set; }
        public List<ItemChargeDto> ItemPrices { get; set; }
        public List<ItemChargeDto> ItemFees { get; set; }
    }
}
