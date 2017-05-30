using System;

namespace EIS.Inventory.Core.ViewModels
{
    public class RequestReportViewModel
    {
        public int Id { get; set; }

        public string ReportRequestId { get; set; }

        public string FeedType { get; set; }

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public string ProcessingStatus { get; set; }

        public DateTime SubmittedDate { get; set; }

        public string SubmittedBy { get; set; }
    }
}
