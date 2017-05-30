using System;

namespace EIS.Inventory.Shared.Models
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
        AmazonOrdersProvider = 3,
        AmazonReportProvider = 4,
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
        BigCommerceOrders = 25,
        eBayOrders = 26,
        EshopoOrders = 27,
        VendorProductService = 28,
        ExportDataService = 29,
        AmazonGetInfoWorker = 30,
        BlacklistedSKUFileUploadWorker = 31,
        BulkDeleteWorker = 32,
        eBaySuggestedCategoriesWorker = 33,
        KitFileUploadWorker = 34,
        ProductFileUploadWorker = 35,
        ShadowFileUploadWorker = 36,
        VendorProductFileUploadWorker = 37,
        AmazonProductInventory = 38,
        CustomExportProductTaskService = 39,
        ExportOrderTaskService = 40,
        FileInventoryTaskService = 41,
        GeneratePoTaskService = 42,
        MarketplaceInventoryTaskService = 43,
        FtpWebRequestor = 44,
        EmailSender = 45,
        BigCommerceProductListing = 46,
        BigCommerceReviseItem = 47,
        BigCommerceInventoryUpdate = 48,
        BigCommercePriceUpdate = 49,
        BigCommerceProductProvider = 50,
        ShippingFedEx = 51,
        ShippingEndicia = 52,
        eBayPriceUpdate = 53,
        BigCommerceEndListing = 54,
        ShippingRateFileUploadWorker = 55,
        CustomerService = 56
    }
}
