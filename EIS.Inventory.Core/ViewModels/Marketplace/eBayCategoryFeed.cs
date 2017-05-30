using System;

namespace EIS.Inventory.Core.ViewModels
{
    public class eBayCategoryFeed
    {
        public string EisSKU { get; set; }
        public string Name { get; set; }
        public string AmazonTitle { get; set; }
        public string UPC { get; set; }
        public string Category { get; set; }
        public string Description { get; set; }
        public string Keyword
        {
            get
            {
                try
                {
                    return !string.IsNullOrEmpty(UPC) ? UPC
                        : !string.IsNullOrEmpty(Name) ? Name
                        : !string.IsNullOrEmpty(AmazonTitle) ? AmazonTitle
                        : !string.IsNullOrEmpty(Description) ? Description
                        : Category;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }
        }
    }
}
