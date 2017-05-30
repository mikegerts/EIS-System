using System;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.Models
{
    public class ProductAmazon : MarketplaceProduct
    {
        public string ASIN { get; set; }
        public int? PackageQty { get; set; }
        public string ProductGroup { get; set; }
        public string ProductTypeName { get; set; }
        public int? SafetyQty { get; set; }
        public decimal? Price { get; set; }
        //public int? AvailableQty { get; set; }
        public int? LeadtimeShip { get; set; }
        public int? NumOfItems { get; set; }
        public int? MaxOrderQty { get; set; }
        public decimal MapPrice { get; set; }
        public bool IsAllowGiftWrap { get; set; }
        public bool isAllowGiftWrapSet { get; set; }
        public bool IsAllowGiftMsg { get; set; }
        public bool isAllowGiftMsgSet { get; set; }
        public string Condition { get; set; }
        public string ConditionNote { get; set; }
        public string FulfilledBy { get; set; }
        public string FbaSKU { get; set; }
        public string TaxCode { get; set; }
        //public decimal? WeightBox1 { get; set; }
        //public string WeightBox1Unit { get; set; }
        //public decimal? WeightBox2 { get; set; }
        //public string WeightBox2Unit { get; set; }
        public List<MediaContent> Images { get; set; }
        public bool IsEnabled { get; set; }
        public bool isEnabledSet { get; set; }

        public bool HasAnyChanges
        {
            get
            {
                return (
                    //SafetyQty != null ||
                    //AvailableQty != null ||
                    ASIN != null ||
                    Price != null ||
                    LeadtimeShip != null ||
                    PackageQty != null ||
                    NumOfItems != null ||
                    MaxOrderQty != null ||
                    ProductTitle != null ||
                    isAllowGiftWrapSet ||
                    isAllowGiftMsgSet ||
                    Condition != null ||
                    ConditionNote != null ||
                    FulfilledBy != null ||
                    FbaSKU != null ||
                    isEnabledSet ||
                    TaxCode != null ||
                    ProductGroup != null ||
                    ProductTypeName != null);
            }
        }
    }
}
