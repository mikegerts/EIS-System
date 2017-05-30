using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.ViewModels
{
    public class CustomerExportedFileDto
    {
        /// <summary>
        /// Gets or sets the ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the id of the scheduled task
        /// </summary>
        public int ScheduledTaskId { get; set; }

        /// <summary>
        /// Gets or sets the file name of the exported file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file size (in bytes) of the file
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// Gets or sets the date created of the file
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// Determine if the file is a CSV file
        /// </summary>
        public bool IsCsvFile
        {
            get { return FileName.EndsWith(".csv"); }
        }
    }
}