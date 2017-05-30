using System;

namespace EIS.Inventory.Shared.Models
{
    public class MarketplaceRequestReport
    {
        public string RequestId { get; set; }
        public string ReportRequestId { get; set; }
        public string FeedType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsScheduled { get; set; }
        public string ProcessingStatus { get; set; }
        public string SubmittedDate { get; set; }
        public string SubmittedBy { get; set; }
    }
}
