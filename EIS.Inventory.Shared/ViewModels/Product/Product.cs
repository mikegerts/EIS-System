using EIS.Inventory.Shared.Models;
using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.Shared.ViewModels
{
    public class Product
    {
        public Product()
        {
            // set the default values
            Images = new List<string>();
        }
        public string EisSKU { get; set; }
        public string Brand { get; set; }
        public string EAN { get; set; }
        public string Color { get; set; }
        public string Model_ { get; set; }
        public int CompanyId { get; set; }
        public bool isCompanySet { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ShortDescription { get; set; }
        public string Category { get; set; }
        public int? ProductTypeId { get; set; }
        //public ProductTypeDto ProductType { get; set; }
        public string UPC { get; set; }
        public decimal SellerPrice { get; set; }
        public bool isSellerPriceSet { get; set; }
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
        public decimal? GuessedWeight { get; set; }
        public decimal? AccurateWeight { get; set; }
        public string AccurateWeightUnit { get; set; }
        public string GuessedShipping { get; set; }
        public string GuessedWeightUnit { get; set; }
        public string AccurateShipping { get; set; }
        public bool IsKit { get; set; }
        public bool isKitSet { get; set; }
        public SkuType SkuType { get; set; }
        public bool isSkuTypeSet { get; set; }
        public List<string> Images { get; set; }
        public bool IsBlacklisted { get; set; }
        public bool isBlacklistedSet { get; set; }

        /// <summary>
        /// Determine if it has Product details
        /// </summary>
        public bool HasAnyChanges
        {
            get
            {
                return (Name != null ||
                    Description != null ||
                    ShortDescription != null ||
                    Category != null ||
                    ProductTypeId != null ||
                    UPC != null ||
                    isSellerPriceSet ||
                    PkgLength != null ||
                    PkgWidth != null ||
                    PkgHeight != null ||
                    PkgLenghtUnit != null ||
                    PkgWeight != null ||
                    PkgWeightUnit != null ||
                    ItemLength != null ||
                    ItemWidth != null ||
                    ItemHeight != null ||
                    ItemLenghtUnit != null ||
                    ItemWeight != null ||
                    ItemWeightUnit != null ||
                    GuessedWeight != null ||
                    AccurateWeight != null ||
                    GuessedWeightUnit != null ||
                    AccurateWeightUnit != null ||
                    GuessedShipping != null ||
                    AccurateShipping != null ||
                    isCompanySet || 
                    isKitSet ||
                    IsBlacklisted ||
                    isSkuTypeSet ||
                    Images.Any());
            }
        }
    }
}
