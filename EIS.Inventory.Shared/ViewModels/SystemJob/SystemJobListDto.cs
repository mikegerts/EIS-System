using System;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Shared.ViewModels
{
    public class SystemJobListDto
    {
        public int Id { get; set; }
        public JobType JobType { get; set; }
        public JobStatus Status { get; set; }
        public Nullable<int> CurrentNumOfItems { get; set; }
        public Nullable<int> TotalNumOfItems { get; set; }
        public string ParametersOut { get; set; }
        public string SubmittedBy { get; set; }
        public DateTime Created { get; set; }
        public bool HasResultFile
        {
            get { return !string.IsNullOrEmpty(ParametersOut); }
        }
        public string Text
        {
            get
            {
                var text = string.Empty;
                switch (Status)
                {
                    case JobStatus.Canceled:
                        text = "{0} canceled!\n{1}";
                        break;
                    case JobStatus.Pending:
                        text = "{0} is in pending!\n{1}";
                        break;
                    case JobStatus.Inprogress:
                        text = "{0} has started!\n{1}";
                        break;
                    case JobStatus.Completed:
                        text = "{0} completed!\n{1}";
                        break;
                    case JobStatus.Failed:
                        text = "{0} has failed!\n{1}";
                        break;
                    default:
                        text = "{0} don't know!\n{1}";
                        break;
                }

                return string.Format(text, JobType.GetDescription(),
                    string.Format("Initiated by: {0}", SubmittedBy));
            }
        }
    }
}
