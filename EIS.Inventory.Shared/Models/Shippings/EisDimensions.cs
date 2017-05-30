using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisDimensions
    {
        public string Length { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public EisLinearUnits Units { get; set; }
        public bool UnitsSpecified { get; set; }
    }

    public enum EisLinearUnits
    {
        CM, IN
    }
}
