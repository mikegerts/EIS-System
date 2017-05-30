using System;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class SettlementOrderDto
    {
        public SettlementOrderDto()
        {
            Items = new List<SettlementItemDto>();
        }
        public string OrderId { get; set; }
        public string Marketplace { get; set; }
        public string ShipmentId { get; set; }
        public string MerchantFulfillmentId { get; set; }
        public DateTime PostedDate { get; set; }
        public string SettlementId { get; set; }
        public List<SettlementItemDto> Items { get; set; }
    }
}
