
namespace EIS.Inventory.Shared.ViewModels
{
    public class CredentialDto
    {
        public int Id { get; set; }
        public string Mode { get; set; }
        public string Name { get; set; }
        public string MarketplaceType { get; set; }
        public string MarketplaceId { get; set; }
        public int CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string ServiceEndPoint { get; set; }
        public bool IsDefault { get; set; }
        public bool IsEnabled { get; set; }
        public string ModifiedBy { get; set; }
    }
}
