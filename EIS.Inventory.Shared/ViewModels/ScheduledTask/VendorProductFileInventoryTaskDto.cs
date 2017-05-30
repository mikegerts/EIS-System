
namespace EIS.Inventory.Shared.ViewModels
{
    public class VendorProductFileInventoryTaskDto : ScheduledTaskDto
    {
        /// <summary>
        ///  Gets or sets the id of the vendors
        /// </summary>
        public int? VendorId { get; set; }

        public string FileName { get; set; }
        public string FileType { get; set; }
        public bool HasHeader { get; set; }
        public bool IsDeleteFile { get; set; }

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
        public bool IsZeroOutQty { get; set; }
        public bool IsAddNewItem { get; set; }
        public bool IsLinkEisSKU { get; set; }
        public bool IsCreateEisSKUAndLink { get; set; }
        public int? SKU { get; set; }
        public int? Quantity { get; set; }
        public int? SupplierPrice { get; set; }
        public int? ProductName { get; set; }
        public int? Description { get; set; }
        public int? Category { get; set; }
        public int? UPC { get; set; }
        public int? MinPack { get; set; }
    }
}
