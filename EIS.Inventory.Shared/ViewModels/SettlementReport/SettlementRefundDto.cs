using System;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class SettlementRefundDto
    {
        public SettlementRefundDto()
        {
            Items = new List<SettlementItemDto>();
        }
        public string OrderId { get; set; }
        public string Marketplace { get; set; }
        public string AdjustmentId { get; set; }
        public string MerchantFulfillmentId { get; set; }
        public DateTime PostedDate { get; set; }
        public string SettlementId { get; set; }
        public List<SettlementItemDto> Items { get; set; }
    }
}
