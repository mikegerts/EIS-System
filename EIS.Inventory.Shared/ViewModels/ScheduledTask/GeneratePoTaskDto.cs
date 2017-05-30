
namespace EIS.Inventory.Shared.ViewModels
{
    public class GeneratePoTaskDto : ScheduledTaskDto
    {
        /// <summary>
        ///  Gets or sets the id of the vendors
        /// </summary>
        public int? VendorId { get; set; }

        /// <summary>
        ///  Gets or sets the the file name of the task to output file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file type
        /// </summary>
        public string FileType { get; set; }

        /// <summary>
        /// Gets or sets where the file to be exported
        /// </summary>
        public string ExportTo { get; set; }

        /// <summary>
        /// Gets or sets the Orders as PO generated
        /// </summary>
        public bool MarkPoGenerated { get; set; }

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
    }
}
