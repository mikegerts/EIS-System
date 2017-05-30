
namespace EIS.Inventory.Shared.ViewModels
{
    public class ProducteBayDto
    {
        public ProducteBayDto()
        {
            ListType = "FixedPriceItem";
        }

        public string EisSKU { get; set; }
        public string ItemId { get; set; }
        public int? CategoryId { get; set; }
        public string CategoryName { get; set; }
        public string Title { get; set; }
        public string SubTitle { get; set; }
        public string Description { get; set; }
        public int ListingQuantity { get; set; }
        public int SafetyQty { get; set; }
        public decimal StartPrice { get; set; }
        public decimal ReservePrice { get; set; }
        public decimal BinPrice { get; set; }
        public string ListType { get; set; }
        public string Duration { get; set; }
        public string Location { get; set; }
        public int Condition_ { get; set; }
        public int DispatchTimeMax { get; set; }
        public bool IsOutOfStockListing { get; set; }
        public bool IsBoldTitle { get; set; }
        public bool IsRequireAutoPayment { get; set; }
        public bool IsEnabled { get; set; }
        public bool isBoldTitleSet { get; set; }
        public bool isOutOfStockListingSet { get; set; }
        public bool isRequireAutoPaymentSet { get; set; }
        public bool isEnabledSet { get; set; }
        public bool isListingQuantitySet { get; set; }
        public bool isSafetyQtySet { get; set; }
        public bool isCategoryIdSet { get; set; }
        public bool isStartPriceSet { get; set; }
        public bool isReservePriceSet { get; set; }
        public bool isBinPriceSet { get; set; }
        public bool isConditionSet { get; set; }
        public bool isDispatchTimeMaxSet { get; set; }

        public bool HasAnyChanged
        {
            get
            {
                return ItemId != null ||
                    Title != null ||
                    SubTitle != null ||
                    Description != null ||
                    isListingQuantitySet ||
                    isSafetyQtySet ||
                    isStartPriceSet ||
                    isReservePriceSet ||
                    isBinPriceSet ||
                    ListType != null ||
                    Duration != null ||
                    Location != null ||
                    isCategoryIdSet || 
                    isConditionSet ||
                    isDispatchTimeMaxSet ||
                    isBoldTitleSet ||
                    isOutOfStockListingSet ||
                    isEnabledSet;
            }
        }

    }
}
