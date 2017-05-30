namespace EIS.Inventory.Shared.Models
{
    public enum MessageType
    {
        eBayDesription = 0,
        CustomExportOrder = 1,
        CustomExportProduct = 2,
        GeneratePO = 3,
        EmailNotification = 4,
        ErrorNotification = 5,
        InsufficientVendorProduct = 6,
    }
}
