using System;

namespace EIS.DataUploaderApp.Helpers
{
    public class FileResult
    {
        /// <summary>
        /// Gets or sets the owner id, this might be a vendor id
        /// </summary>
        public long OwnerId { get; set; }

        /// <summary>
        /// Gets or sets the total records read by the file manager
        /// </summary>
        public int TotalRecords { get; set; }

        /// <summary>
        /// Gets or sets the result date
        /// </summary>
        public DateTime ResultDate { get; set; }

        /// <summary>
        /// Gets or sets the error message reported by the file manager
        /// </summary>
        public string ErrorMessage { get; set; }

        /// <summary>
        /// Gets or sets the error flag property
        /// </summary>
        public bool HasError { get; set; }
    }
}
