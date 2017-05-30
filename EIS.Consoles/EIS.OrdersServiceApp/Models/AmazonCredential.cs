
namespace EIS.OrdersServiceApp.Models
{
    public class AmazonCredential : Credential
    {
        public string MerchantId { get; set; }
        public string AccessKeyId { get; set; }
        public string SecretKey { get; set; }
        public string AssociateId { get; set; }
        public string SearchAccessKeyId { get; set; }
        public string SearchSecretKey { get; set; }
    }
}
