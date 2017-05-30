using System;

namespace EIS.Inventory.Core.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public LogEntrySeverity Severity { get; set; }
        public LogEntryType EntryType { get; set; }
        public string Description { get; set; }
        public string StackTrace { get; set; }
    }

    public enum LogEntrySeverity
    {
        Information = 1,
        Warning = 2,
        Error = 3
    }

    public enum LogEntryType
    {
        General = 1,
        AmazonListing = 2,
        AmazonOrders = 3,
        AmazonReport = 4,
        eBayProductListing = 5,
        eBayInventoryUpdate = 6,
        eBayReport = 7,
        ProductService = 8,
        LogService = 9,
        MarketplaceSettingService = 10,
        OrderService = 11,
        ProductTypeService = 12,
        ReportLogService = 13,
        VendorService = 14,
        AmazonProduct = 15,
        MarketplaceProductManager = 16,
        AmazonPriceUpdate = 17,
        AmazonInventoryUpdate = 18,
        AmazonProductProvider = 19,
        eBayProductProvider = 20,
        MarketplaceInventoryManager = 21,
        eBayProductRevise = 22,
        eBayEndListing = 23,
        AmazonReviseItem = 24,
        BigCommerceOrders = 25
    }
}
