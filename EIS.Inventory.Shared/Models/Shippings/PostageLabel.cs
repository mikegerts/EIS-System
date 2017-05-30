using System;

namespace EIS.Inventory.Shared.Models
{
    public class PostageLabel
    {
        public string ErrorMessage { get; set; }
        public string Base64LabelImage { get; set; }
        public string TrackingNumber { get; set; }
        public long TransactionId { get; set; }
        public string TransactionDateTime { get; set; }
        public string PostmarkDate { get; set; }
        public decimal PostageBalance { get; set; }
        public decimal PostageTotalPrice { get; set; }
    }
}
