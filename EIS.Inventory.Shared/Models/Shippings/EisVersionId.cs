using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisVersionId
    {
        public string ServiceId { get; set; }
        public int Major { get; set; }
        public int Intermediate { get; set; }
        public int Minor { get; set; }
    }
}
