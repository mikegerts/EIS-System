
namespace EIS.Inventory.Shared.Models
{
    public class MarketplaceProduct
    {
        public string EisSKU { get; set; }
        public string Brand { get; set; }
        public string Color { get; set; }
        public string EAN { get; set; }
        public string Label { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string ProductTitle { get; set; }
        public Dimension ItemDimension { get; set; }
        public Dimension PackageDimension { get; set; }
        public string Size { get; set; }
    }
}
