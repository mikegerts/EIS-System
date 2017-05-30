using EIS.Inventory.Shared.Helpers;
using System;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class ScheduledTaskDto
    {
        /// <summary>
        /// Gets or sets the id of the task
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the flag if the task is enabled
        /// </summary>
        public bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the type of the task
        /// </summary>
        public string TaskType { get; set; }

        /// <summary>
        ///  Gets or sets the name of the task
        /// </summary>
        public string Name { get; set; }
        
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
        /// Gets or sets the flag to tell the task to execute immediately
        /// </summary>
        public bool IsRunNow { get; set; }

        /// <summary>
        /// Gets or sets the list of days the scheduler task will run
        /// </summary>
        public List<string> Days { get; set; }

        /// <summary>
        /// Gets or sets the recurrence of the scheduled task
        /// </summary>
        public string Recurrence { get; set; }

        /// <summary>
        /// Gets or sets the every other day occurence
        /// </summary>
        public int OccurrAt { get; set; }

        /// <summary>
        /// Gets or sets the last execute date time of the task
        /// </summary>
        public DateTime? LastExecutedOn { get; set; }

        /// <summary>
        /// Gets or sets the number of task run
        /// </summary>
        public int History { get; set; }

        public string ModifiedBy { get; set; }
    }
}
