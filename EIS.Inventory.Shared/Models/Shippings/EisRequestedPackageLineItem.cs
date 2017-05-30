using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisRequestedPackageLineItem
    {
        public string SequenceNumber { get; set; }
        public string GroupPackageCount { get; set; }
        public EisWeight Weight { get; set; }
        public EisDimensions Dimensions { get; set; }
        public EisMoney InsuredValue { get; set; }
    }
}
