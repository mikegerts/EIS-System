using System.Collections.Generic;


namespace EIS.Inventory.Shared.ViewModels
{
   public class CustomImportOrderTaskDto : ScheduledTaskDto
    {

        public CustomImportOrderTaskDto()
        {
            CustomFields = new List<string>();
        }
        /// <summary>
        ///  Gets or sets the the file name of the task to output file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file type
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets whether to include the file headers
        /// </summary>
        public bool HasHeader { get; set; }
        

        /// <summary>
        /// Flag to whethere drop the file if there's no order or not
        /// </summary>
        public bool IsDropNoOrderFile { get; set; }

        /// <summary>
        /// Gets or sets confirmation email TOs for the Order
        /// </summary>
        public string ConfirmationEmailTos { get; set; }

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

    }
}
