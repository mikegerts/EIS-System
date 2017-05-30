
namespace EIS.Inventory.Core.ViewModels
{
    public class ProductAmazonDto
    {
        public ProductAmazonDto()
        {
            WeightBox1Unit = "pounds";
            WeightBox2Unit = "ounces";
        }
        public string EisSKU { get; set; }
        public string ASIN { get; set; }
        public bool IsEnabled { get; set; }
        public string ProductTitle { get; set; }
        public decimal? Price { get; set; }
        public int? LeadtimeShip { get; set; }
        public int SafetyQty { get; set; }
        public int? PackageQty { get; set; }
        public int? NumOfItems { get; set; }
        public int? MaxOrderQty { get; set; }
        public decimal MapPrice { get; set; }
        public bool IsAllowGiftWrap { get; set; }
        public bool IsAllowGiftMsg { get; set; }
        public string TaxCode { get; set; }
        public string Condition { get; set; }
        public string ConditionNote { get; set; }
        public string FulfilledBy { get; set; }
        public string FbaSKU { get; set; }
        public string ProductGroup { get; set; }
        public string ProductTypeName { get; set; }
        public decimal? WeightBox1 { get; set; }
        public string WeightBox1Unit { get; set; }
        public decimal? WeightBox2 { get; set; }
        public string WeightBox2Unit { get; set; }
        public string ModifiedBy { get; set; }
    }
}
