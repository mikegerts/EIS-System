using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class CustomExportProductTaskDto : ScheduledTaskDto
    {
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

        public bool IsAddDropShipFee { get; set; }
        public bool IsUseGuessedWeight { get; set; }
        public bool IsUseGuessedShipping { get; set; }

        /// <summary>
        /// Gets or sets where the file to be exported
        /// </summary>
        public string ExportTo { get; set; }

        /// <summary>
        /// Gets or sets the subject of the email
        /// </summary>
        public string EmailSubject { get; set; }

        /// <summary>
        /// Gets or sets the emails for To
        /// </summary>
        public string EmailTo { get; set; }

        /// <summary>
        /// Gets or sets the emails for CC
        /// </summary>
        public string EmailCc { get; set; }

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
        public List<int> CompanyIds { get; set; }
        public List<string> FileHeaders { get; set; }
        public List<string> CustomFields { get; set; }

    }
}
