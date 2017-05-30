using System;
using EIS.DataUploaderApp.Helpers;

namespace EIS.DataUploaderApp.Models
{
    public class FileSetting
    {
        private string _fileName;

        /// <summary>
        /// Gets or sets the name of the vendor
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// Gets or sets the file name of vendor inventory file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the file extension of the file i.e .xls, .xlsx, .csv, etc
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// Gets or sets of the path of the file where it can be found
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Gets or sets the new directory of the parsed file
        /// </summary>
        public string TransferPath { get; set; }

        /// <summary>
        /// Gets or sets the time when the system start to read
        /// </summary>
        public DateTime ReadTime { get; set; }

        /// <summary>
        /// Gets or sets the starting row to read at
        /// </summary>
        public int RowAt { get; set; }

        /// <summary>
        /// Gets or sets the file type
        /// </summary>
        public FileType FileType { get; set; }

        /// <summary>
        /// Gets or sets the next run upload date of the system
        /// </summary>
        public DateTime? NextRunDate { get; set; }
        
        /// <summary>
        /// Gets or sets the column index of Supplier SKU
        /// </summary>
        public int SKU { get; set; }

        /// <summary>
        /// Gets or sets the column index of product name
        /// </summary>
        public int? Name { get; set; }

        /// <summary>
        /// Gets or sets the column index of description
        /// </summary>
        public int? Description { get; set; }

        /// <summary>
        /// Gets or sets the column index of short description
        /// </summary>
        public int? ShortDescription { get; set; }

        /// <summary>
        /// Gets or sets the column index of product category
        /// </summary>
        public int? Category { get; set; }

        /// <summary>
        /// Gets or sets the column index of  product UPC code
        /// </summary>
        public int? UPCCode { get; set; }

        /// <summary>
        /// Gets or sets the column index of product cost
        /// </summary>
        public int? Cost { get; set; }

        /// <summary>
        /// Gets or sets the column index of quantity
        /// </summary>
        public int? Quantity { get; set; }

        /// <summary>
        /// Gets or sets the delimiter of the file
        /// </summary>
        public char Delimiter { get; set; }
        
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
        /// Gets or sets the confirmation of the FTP password
        /// </summary>
        public string ConfirmPassword { get; set; }

        /// <summary>
        /// Gets or sets the FTP port
        /// </summary>
        public int? FtpPort { get; set; }

        /// <summary>
        /// Gets or sets the FTP remote folder path
        /// </summary>
        public string RemoteFolder { get; set; }

        /// <summary>
        /// An helper property
        /// </summary>
        public DateTime ResultDate { get; set; }

        /// <summary>
        /// Gets or sets the flag for delete file
        /// </summary>
        public bool IsDeleteFile { get; set; }

        /// <summary>
        /// Gets or sets the flag to run task immediately
        /// </summary>
        public bool IsRunNow { get; set; }

        /// <summary>
        /// Gets or sets the last date time of execution
        /// </summary>
        public DateTime? LastExecutedOn { get; set; } 

        /// <summary>
        /// Get the time stamped file name
        /// </summary>
        public string FormattedFileName
        {
            get
            {
                if(!string.IsNullOrEmpty(_fileName))
                    return _fileName;

                _fileName = string.Format(FileName, DateTime.Now) + Extension;
                return _fileName;
            }
        }

        /// <summary>
        /// Get the file full path with formatted file name from the FTP server
        /// </summary>
        public string FtpFileFullPath
        {
            get
            {
                return string.Format("ftp://{0}:{1}/{2}/{3}", FtpServer,
                FtpPort == null ? 21 : FtpPort,
                string.IsNullOrEmpty(RemoteFolder) ? string.Empty : RemoteFolder,
                FormattedFileName);
            }
        }
    }
}
