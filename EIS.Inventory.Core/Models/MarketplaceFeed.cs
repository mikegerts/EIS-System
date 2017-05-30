using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.Core.Models
{
    public class MarketplaceFeed
    {
        public MarketplaceFeed()
        {
            Marketplaces = new List<string>();
            Companies = new List<int>();
            SelectedEisSKUs = new List<string>();
            ExcludedEisSKUs = new List<string>();
            WithImages = -1;
            IsKit = null;
            IsSKULinked = null;
        }

        public string SearchString { get; set; }
        public string Mode { get; set; }
        public long ProductGroupId { get; set; }
        public int? CompanyId { get; set; }
        //public int? VendorId { get; set; }
        //public int? QuantityFrom { get; set; }
        //public int? QuantityTo { get; set; }
        public List<string> Marketplaces { get; set; }
        public List<int> Companies { get; set; }
        public List<string> SelectedEisSKUs { get; set; }
        public List<string> ExcludedEisSKUs { get; set; }
        public bool IsAllProductItems { get; set; }
        public int WithImages { get; set; }
        public bool? IsKit { get; set; }
        public Shared.Models.SkuType? SkuType { get; set; }
        public bool? IsSKULinked { get; set; }
        public bool? IsAmazonEnabled { get; set; }
        public bool? HasASIN { get; set; }
    }
}
