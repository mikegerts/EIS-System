
using System.Collections.Generic;

namespace EIS.Inventory.Core.ViewModels
{
    public class ProductTypeCategoryViewModel
    {
        public int ProductTypeId { get; set; }

        public string Category { get; set; }

        public string AmazonMainCatClassName { get; set; }

        public string AmazonSubCatClassName { get; set; }

        /// <summary>
        /// Gets or sets the eisProduct names which belong to the category
        /// </summary>
        public List<string> Products { get; set; }
    }
}
