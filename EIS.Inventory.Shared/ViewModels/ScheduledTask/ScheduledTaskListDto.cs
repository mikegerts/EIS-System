using System;
using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class ScheduledTaskListDto
    { /// <summary>
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
        /// Gets or sets the last execute date time of the task
        /// </summary>
        public DateTime? LastExecutedOn { get; set; }

        /// <summary>
        /// Gets or sets the number of task run
        /// </summary>
        public int? History { get; set; }

        /// <summary>
        /// Gets or sets the recurrence of the scheduled task
        /// </summary>
        public string Recurrence { get; set; }
    }
}
