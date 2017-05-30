
namespace EIS.Inventory.Shared.ViewModels
{
    public class eBayCredentialDto : CredentialDto
    {
        public string ApplicationId { get; set; }
        public string DeveloperId { get; set; }
        public string CertificationId { get; set; }
        public string UserToken { get; set; }
        public int? DescriptionTemplateId { get; set; }
        public string eBayDescriptionTemplate { get; set; }
        public string PayPalEmailAddress { get; set; }
    }
}
