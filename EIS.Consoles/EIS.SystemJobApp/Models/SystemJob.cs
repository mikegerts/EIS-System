using EIS.Inventory.Shared.Models;

namespace EIS.SystemJobApp.Models
{
    public class SystemJob
    {
        public int Id { get; set; }
        public JobType JobType { get; set; }
        public bool IsNotified { get; set; }
        public JobStatus Status { get; set; }
        public int? CurrentNumOfItems { get; set; }
        public int? TotalNumOfItems { get; set; }
        public bool IsAddNewItem { get; set; }
        public bool HasPostAction_1 { get; set; }
        public bool HasPostAction_2 { get; set; }
        public bool HasHeader { get; set; }
        public string SubmittedBy { get; set; }
        public string Parameters { get; set; }
        public string ParametersOut { get; set; }
        public string SupportiveParameters { get; set; }
    }
}
