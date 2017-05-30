
namespace EIS.Inventory.Shared.Helpers
{   
    public enum MarketplaceMode
    {
        TEST,
        LIVE
    }

    public static class Status
    {
        public const string NOT_PROCESSED = "Not processed";
        public const string FAILED = "Failed";
        public const string SUCCESS = "Success";
    }

    public static class ExportTo
    {
        public const string FTP = "FTP";
        public const string EMAIL = "Email";
    }

    public static class CredentialType
    {
        public const string EBAY = "eBay";
        public const string AMAZON = "Amazon";
        public const string SHIP_STATION = "ShipStation";
        public const string BIG_COMMERCE = "BigCommerce";
    }

    public static class ScheduledTaskType
    {
        public const string GENERATE_PO = "GeneratePO";
        public const string MARKETPLACE_INVENTORY = "MarketplaceInventory";
        public const string VENDOR_PRODUCT_FILE_INVENTORY = "VendorProductFileInventory";
        public const string CUSTOM_EXPORT_PRODUCT = "CustomExportProduct";
        public const string CUSTOM_EXPORT_ORDER = "CustomExportOrder";
        public const string CUSTOM_IMPORT_ORDER = "CustomImportOrder";
    }

    public static class CustomerScheduledTaskType
    {
        public const string CUSTOMER_EXPORT_SKU = "CustomerExportSku";
        
    }

    public static class Apps
    {
        public const string EIS_WEBSITE = "EIS Website";
    }
}
