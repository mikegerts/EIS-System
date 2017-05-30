using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.ViewModels
{
    public class ShippingRateDto
    {
        public int Id { get; set; }
        public int? WeightFrom { get; set; }
        public int? WeightTo { get; set; }
        public string Unit { get; set; }
        public decimal? Rate { get; set; }
    }
}
