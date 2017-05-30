namespace EIS.Inventory.Shared.Models
{
    public enum MailClass
    {
        // for multiple postage calculations
        Domestic,
        International,

        // for single postage calculations
        PriorityExpress,
        First,
        Priority,
        LibraryMail,
        MediaMail,
        ParcelSelect,
        RetailGround,
        PriorityMailExpressInternational,
        FirstClassMailInternational,
        FirstClassPackageInternationalService,
        PriorityMailInternational,
    }
}
