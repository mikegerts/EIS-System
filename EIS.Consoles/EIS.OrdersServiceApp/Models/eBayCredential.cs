
namespace EIS.OrdersServiceApp.Models
{
    public class eBayCredential : Credential
    {
        public string ApplicationId { get; set; }
        public string DeveloperId { get; set; }
        public string CertificationId { get; set; }
        public string UserToken { get; set; }
    }
}
