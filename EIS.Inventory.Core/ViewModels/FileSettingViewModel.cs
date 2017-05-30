using System;
using System.ComponentModel.DataAnnotations;
using EIS.Inventory.Core.Helpers;

namespace EIS.Inventory.Core.ViewModels
{
    public class FileSettingViewModel
    {
        public FileSettingViewModel()
        {
            Delimiter = ',';
        }

        /// <summary>
        /// Gets or sets the name of the vendor
        /// </summary>
        [Required]
        public int VendorId { get; set; }
        
        /// <summary>
        /// Gets or sets the file name of vendor inventory file
        /// </summary>
        [Required]
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
        [Required]
        public int RowAt { get; set; }

        /// <summary>
        /// Gets or sets the file type
        /// </summary>
        [Required]
        public FileType FileType { get; set; }

        /// <summary>
        /// Gets or sets the next run upload date of the system
        /// </summary>
        public DateTime? NextRunDate { get; set; }

        /// <summary>
        /// Gets or sets the column index of Supplier SKU
        /// </summary>
        [Required]
        public int SKU { get; set; }

        /// <summary>
        /// Gets or sets the column index of eisProduct name
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
        /// Gets or sets the column index of eisProduct category
        /// </summary>
        public int? Category { get; set; }

        /// <summary>
        /// Gets or sets the column index of  eisProduct UPC code
        /// </summary>
        public int? UPC { get; set; }

        /// <summary>
        /// Gets or sets the column index of eisProduct cost
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
        /// An helper property vendor name
        /// </summary>
        public string VendorName { get; set; }

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
        /// Gets or sets flag whether to delete the file or not
        /// </summary>
        public bool IsDeleteFile { get; set; }
    }
}
