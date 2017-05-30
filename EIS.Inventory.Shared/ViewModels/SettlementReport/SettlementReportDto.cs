using System;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class SettlementReportDto
    {
        public List<SettlementOrderDto> Orders { get; set; }
        public List<SettlementRefundDto> Refunds { get; set; }
    }
}
