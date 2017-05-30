using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.ViewModels
{
    public class ProductDto
    {
        public ProductDto()
        {
            // set the default values
            Images = new List<string>();
            Shadows = new List<ShadowDto>();
        }
        
        public string EisSKU { get; set; }
        [Required]
        public int CompanyId { get; set; }
        public string Brand { get; set; }
        public string EAN { get; set; }
        public string Color { get; set; }
        public string Model_ { get; set; }
        public decimal? GuessedWeight { get; set; }
        public string GuessedWeightUnit { get; set; }
        public decimal? AccurateWeight { get; set; }
        public string AccurateWeightUnit { get; set; }
        public string GuessedShipping { get; set; }
        public string AccurateShipping { get; set; }       
        [Display(Name = "Product Name")]
        public string Name { get; set; }
        public string Description { get; set; }

        [Display(Name = "Short Description")]
        public string ShortDescription { get; set; }
        public string Category { get; set; }
        public int? ProductTypeId { get; set; }
        public ProductTypeViewModel ProductType { get; set; }
        [Display(Name = "UPC Code")]
        public string UPC { get; set; }
        [Display(Name = "Seller Price")]
        public decimal? SellerPrice { get; set; }
        public decimal? PkgLength { get; set; }
        public decimal? PkgHeight { get; set; }
        public decimal? PkgWidth { get; set; }
        public string PkgLenghtUnit { get; set; }
        public decimal? PkgWeight { get; set; }
        public string PkgWeightUnit { get; set; }
        public decimal? ItemLength { get; set; }
        public decimal? ItemWidth { get; set; }
        public decimal? ItemHeight { get; set; }
        public string ItemLenghtUnit { get; set; }
        public decimal? ItemWeight { get; set; }
        public string ItemWeightUnit { get; set; }
        public bool IsKit { get; set; }
        public bool IsBlacklisted { get; set; }
        public SkuType SkuType { get; set; }
        public int FactorQuantity { get; set; }
        public int KitQuantity { get; set; }
        public decimal KitSellerPrice { get; set; }
        public decimal KitSupplierPrice { get; set; }
        public string ParentProductEisSKU { get; set; }        
        public List<string> Images { get; set; }
        public List<ShadowDto> Shadows { get; set; }

        // helper properties
        public int? Quantity { get; set; }
        public decimal? SupplierPrice { get; set; }
        public string ModifiedBy { get; set; }
    }
}
