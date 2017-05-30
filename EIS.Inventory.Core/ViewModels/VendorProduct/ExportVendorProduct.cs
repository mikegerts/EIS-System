using System;
using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.Core.ViewModels
{
    public class ExportVendorProduct
    {
        public string SearchString { get; set; }
        public int? VendorId { get; set; }
        public int? CompanyId { get; set; }
        public int WithEisSKULink { get; set; }
        public int? QuantityFrom { get; set; }
        public int? QuantityTo { get; set; }
        public string ProductFields { get; set; }
        public string ShortDescription { get; set; }
        public string SelectedEisSKUs { get; set; }
        public string ExcludedEisSKUs { get; set; }
        public bool IsAllProductItems { get; set; }
        public string Delimiter { get; set; }
        public string SortBy { get; set; }
        public int WithImages { get; set; }
        public List<string> ProductFieldsArr
        {
            get
            {
                if (string.IsNullOrEmpty(ProductFields))
                    return new List<string>();

                var fieldList = ProductFields.Split(',').ToList();

                // remove the 1 & 2
                fieldList.Remove("1");
                fieldList.Remove("2");
                return fieldList;
            }
        }

        /// <summary>
        /// Determine where we want vendor product has or no EIS SKU link
        /// </summary>
        public bool IsWithEisSKULink
        {
            get { return WithEisSKULink != -1; }
        }

        /// <summary>
        /// Determine if EisSKU links is included for the export
        /// </summary>
        public bool IsIncludeEisSKULinks
        {
            get
            {
                return ProductFields.Split(',').Any(x => x == "2");
            }
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

        public List<string> SelectedEisSKUsArr
        {
            get
            {
                return string.IsNullOrEmpty(SelectedEisSKUs) ? new List<string>() 
                    : SelectedEisSKUs.Split(',').ToList();
            }
        }
        public List<string> ExcludedEisSKUsArr
        {
            get
            {
                return string.IsNullOrEmpty(ExcludedEisSKUs) ? new List<string>()
                    : ExcludedEisSKUs.Split(',').ToList();
            }
        }
        public DateTime RequestedDate { get; set; }
    }
}
