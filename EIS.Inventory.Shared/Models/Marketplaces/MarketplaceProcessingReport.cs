using System.Collections.Generic;

namespace EIS.Inventory.Shared.Models
{
    public class MarketplaceProcessingReport
    {
        public string TransactionId { get; set; }
        public string MessageType { get; set; }
        public string StatusCode { get; set; }
        public int MessagesProcessed { get; set; }
        public int MessagesSuccessful { get; set; }
        public int MessagesWithError { get; set; }
        public int MessagesWithWarning { get; set; }
        public string MerchantId { get; set; }
        public string SubmittedBy { get; set; }
        public List<MarketplaceProcessingReportResult> ReportResults { get; set; }
    }

    public class MarketplaceProcessingReportResult
    {
        public string TransactionId { get; set; }
        public int MessageId { get; set; }
        public string Code { get; set; }
        public string MessageCode { get; set; }
        public string Description { get; set; }
        public string AdditionalInfo { get; set; }
    }
}
