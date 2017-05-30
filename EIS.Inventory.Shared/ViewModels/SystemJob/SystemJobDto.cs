using System;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Shared.ViewModels
{
    public class SystemJobDto
    {
        public int Id { get; set; }
        public JobType JobType { get; set; }
        public bool IsNotified { get; set; }
        public JobStatus Status { get; set; }
        public Nullable<int> CurrentNumOfItems { get; set; }
        public Nullable<int> TotalNumOfItems { get; set; }
        public bool IsAddNewItem { get; set; }
        public bool HasHeader { get; set; }
        public bool HasPostAction_1 { get; set; }
        public bool HasPostAction_2 { get; set; }
        public string SubmittedBy { get; set; }
        public string Parameters { get; set; }
        public string ParametersOut { get; set; }
        public string SupportiveParameters { get; set; }
    }
}
