using EIS.Inventory.Shared.Models;
using System.Collections.Generic;

namespace EIS.Inventory.Core.ViewModels
{
    public class MarketplaceProductFeedDto
    {
        public MarketplaceProductFeedDto()
        {
            ImageUrls = new List<string>();
        }
        public string EisSKU { get; set; }
        public int? ProductTypeId { get; set; }
        public ProductTypeViewModel ProductType { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Brand { get; set; }
        public string UPC { get; set; }
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
        public List<string> ImageUrls { get; set; }
        public bool IsBlacklisted { get; set; }
        public AmazonProductFeed AmazonProductFeed { get; set; }
        public eBayProductFeed eBayProductFeed { get; set; }
        public eBayInventoryFeed eBayInventoryFeed { get; set; }
        public BigCommerceProductFeed BigCommerceProductFeed { get; set; }
        public int FactorQuantity { get; set; }
    }

    public class AmazonProductFeed
    {
        public string ASIN { get; set; }
        public int? PackageQty { get; set; }
        public int? SafetyQty { get; set; }
        public int? NumOfItems { get; set; }
        public int? MaxOrderQty { get; set; }
        public bool IsAllowGiftWrap { get; set; }
        public bool IsAllowGiftMsg { get; set; }
        public string Condition { get; set; }
        public string ConditionNote { get; set; }
        public bool IsEnabled { get; set; }
        public string TaxCode { get; set; }
        public decimal? WeightBox1 { get; set; }
        public string WeightBox1Unit { get; set; }
        public decimal? WeightBox2 { get; set; }
        public string WeightBox2Unit { get; set; }
    }

    public class eBayProductFeed
    {
        public string ItemId { get; set; }
        public int? CategoryId { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public string ListType { get; set; }
        public string Duration { get; set; }
        public string Location { get; set; }
        public int Condition_ { get; set; }
        public int DispatchTimeMax { get; set; }
        public bool IsOutOfStockListing { get; set; }
        public bool IsBoldTitle { get; set; }
        public string ReturnsAcceptedOption { get; set; }
        public string ShippingCostPaidByOption { get; set; }
        public string RefundOption { get; set; }
        public string ReturnsWithinOption { get; set; }
        public string ReturnPolicyDescription { get; set; }
        public string ShippingType { get; set; }
        public string ShippingService { get; set; }
        public double ShippingServiceCost { get; set; }
        public bool IsRequireAutoPayment { get; set; }
        public bool IsEnabled { get; set; }
    }

    public class BigCommerceProductFeed
    {
        public string EisSKU { get; set; }
        public int? ProductId { get; set; }
        public decimal? Price { get; set; }
        public decimal? RetailPrice { get; set; }
        public decimal? FixedCostShippingPrice { get; set; }
        public string Condition { get; set; }
        public string Categories { get; set; }
        public int? Brand { get; set; }
        public int ProductsType { get; set; }
        public int? CategoryId { get; set; }
        public int? InventoryLevel { get; set; }
        public int? InventoryWarningLevel { get; set; }
        public int? InventoryTracking { get; set; }
        public int? OrderQuantityMinimum { get; set; }
        public int? OrderQuantityMaximum { get; set; }
        public int ProductQuantity { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public bool IsEnabled { get; set; }
    }
}
