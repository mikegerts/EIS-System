using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.ViewModels
{
  public  class CustomerScheduledTaskDto
    {
        public CustomerScheduledTaskDto()
        {
            CustomFields = new List<string>();
        }
        /// <summary>
        /// Gets or sets the id of the task
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the customer id of the task
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the type of the task
        /// </summary>
        public string TaskType { get; set; }

        /// <summary>
        /// Gets or sets the file type
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        ///  Gets or sets the name of the task
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether to include the file headers
        /// </summary>
        public bool HasHeader { get; set; }

        /// <summary>
        ///  Gets or sets the name of the task
        /// </summary>
        public string ImportFileName { get; set; }

        public List<string> FileHeaders { get; set; }

        /// <summary>
        /// Gets or sets the list of name of fields where the vendor is interested in
        /// </summary>
        public List<string> CustomFields { get; set; }


        /// <summary>
        /// Gets or sets the path for FTP server
        /// </summary>
        public string FtpServer { get; set; }

        /// <summary>
        /// Gets or sets the user credential for FTP
        /// </summary>
        public string FtpUser { get; set; }

        /// <summary>
        /// Gets or sets the password credential for FTP
        /// </summary>
        public string FtpPassword { get; set; }

        /// <summary>
        /// Gets or sets the FTP port
        /// </summary>
        public int? FtpPort { get; set; }

        /// <summary>
        /// Gets or sets the remote folder path
        /// </summary>
        public string RemoteFolder { get; set; }

        /// <summary>
        /// Gets or sets confirmation email TOs for the Order
        /// </summary>
        public string ConfirmationEmailTos { get; set; }

        /// <summary>
        ///  Gets or sets the start date of the task to run
        /// </summary>
        public DateTime StartDate { get; set; }

        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// Helper property for the StartTime
        /// </summary>
        public DateTime StartTimeDate { get; set; }

        /// <summary>
        /// Gets or sets the last execute date time of the task
        /// </summary>
        public DateTime? LastExecutedOn { get; set; }

        /// <summary>
        /// Gets or sets the flag to tell the task to execute immediately
        /// </summary>
        public bool IsRunNow { get; set; }

        /// <summary>
        /// Gets or sets the recurrence of the scheduled task
        /// </summary>
        public string Recurrence { get; set; }
        
        /// <summary>
        /// Gets or sets the every other day occurence
        /// </summary>
        public int OccurrAt { get; set; }

        /// <summary>
        /// Gets or sets the list of days the scheduler task will run
        /// </summary>
        public List<string> Days { get; set; }

        /// <summary>
        /// Gets or sets the number of task run
        /// </summary>
        public int History { get; set; }

        /// <summary>
        /// Gets or sets the flag if the task is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        public string ModifiedBy { get; set; }

        public byte[] ExportFile { get; set; }
    }
}
