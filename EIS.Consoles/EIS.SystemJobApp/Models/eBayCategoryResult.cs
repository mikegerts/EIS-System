using System.Collections.Generic;

namespace EIS.SystemJobApp.Models
{
    public class eBayCategoryResult
    {
        public string EisSKU { get; set; }
        public List<eBayCategory> Categories { get; set; }
    }
}
