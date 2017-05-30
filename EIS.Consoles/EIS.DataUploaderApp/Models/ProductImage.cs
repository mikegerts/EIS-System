
namespace EIS.ConsoleApp.Models
{
    public class ProductImage
    {
        public long Id { get; set; }

        public long VendorProductId { get; set; }

        public string ImagePath { get; set; }

        public string ImageType { get; set; }
    }
}
