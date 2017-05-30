
namespace EIS.Inventory.Shared.ViewModels
{
    public class ProductTypeDto
    {
        public int Id { get; set; }
        public string TypeName { get; set; }
        public string AmazonMainCategoryCode { get; set; }
        public string AmazonSubCategoryCode { get; set; }
        public string EbayMainCategoryCode { get; set; }
        public string EbaySubCategoryCode { get; set; }

        // helper properties
        public string AmazonMainCategoryName { get; set; }
        public string AmazonMainClassName { get; set; }
        public string AmazonSubCategoryName { get; set; }
        public string AmazonSubClassName { get; set; }
    }
}
