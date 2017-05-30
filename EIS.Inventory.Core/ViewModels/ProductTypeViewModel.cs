
using System.ComponentModel.DataAnnotations;
namespace EIS.Inventory.Core.ViewModels
{
    public class ProductTypeViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Type Name")]
        public string TypeName { get; set; }

        [Display(Name = "Amazon Main Category")]
        public string AmazonMainCategoryCode { get; set; }
        
        [Display(Name = "Amazon Sub Category")]
        public string AmazonSubCategoryCode { get; set; } 
        
        [Display(Name = "Ebay Main Category")]
        public string EbayMainCategoryCode { get; set; }

        [Display(Name = "Ebay Sub Category")]
        public string EbaySubCategoryCode { get; set; }

        // helper properties
        public string AmazonMainCategoryName { get; set; }

        // helper properties
        public string AmazonMainClassName { get; set; }

        public string AmazonSubCategoryName { get; set; }

        public string AmazonSubClassName { get; set; }
    }
}
