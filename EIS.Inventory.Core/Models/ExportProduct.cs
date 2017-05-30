using System;
using System.Collections.Generic;
using System.Linq;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Models
{
    public class ExportProduct
    {
        public long? ProductGroupId { get; set; }
        public int? VendorId { get; set; }
        public int? CompanyId { get; set; }
        public string SearchString { get; set; }
        public int WithImages { get; set; }
        public int? QuantityFrom { get; set; }
        public int? QuantityTo { get; set; }
        public string ProductFields { get; set; }
        public string SelectedEisSKUs { get; set; }
        public string ExcludedEisSKUs { get; set; }
        public bool IsAllProductItems { get; set; }
        public string Delimiter { get; set; }
        public string SortBy { get; set; }
        public bool? IsKit { get; set; }
        public bool? IsSKULinked { get; set; }
        public SkuType? SkuType { get; set; }
        public bool? IsAmazonEnabled { get; set; }
        public bool? HasASIN { get; set; }

        // helpers
        public bool IsQuantityFromSet
        {
            get { return QuantityFrom != null; }
        }

        public bool IsQuantityToSet
        {
            get { return QuantityTo != null; }
        }

        /// <summary>
        /// Determine if there is with images
        /// </summary>
        public bool IsWithImages
        {
            get { return WithImages != -1; }
        }

        /// <summary>
        /// Boolean to determine if images is included for export
        /// </summary>
        public bool IsIncludeImages
        {
            get { return ProductFields.Split(',').Any(x => x == "1"); }
        }
        public bool HasVendorProductInfo
        {
            get { return ProductFields.Split(',').Any(x => x.StartsWith("vendor_product")); }
        }
        public List<string> ProductFieldsArr
        {
            get
            {
                if (string.IsNullOrEmpty(ProductFields))
                    return new List<string>();

                var fieldList = ProductFields.Split(',').ToList();
                
                // remove the 1
                fieldList.Remove("1");
                return fieldList;
            }
        }
        public List<string> SelectedEisSKUsArr
        {
            get
            {
                return string.IsNullOrEmpty(SelectedEisSKUs) ? new List<string>() : SelectedEisSKUs.Split(',').ToList();
            }
        }
        public List<string> ExcludedEisSKUsArr
        {
            get
            {
                return string.IsNullOrEmpty(ExcludedEisSKUs) ? new List<string>() : ExcludedEisSKUs.Split(',').ToList();
            }
        }
        public DateTime RequestedDate { get; set; }
    }
}
