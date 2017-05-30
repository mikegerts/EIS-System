namespace EIS.Inventory.Shared.Models
{
    public class PackageDetail
    {
        public string RequestedBy { get; set; }
        public int EisOrderId { get; set; }
        public string OrderId { get; set; }
        public string OrderItemId { get; set; }
        public MailClass MailClass { get; set; }
        
        // recepient's information
        public string FromName { get; set; }
        public string FromCompany { get; set; }
        public string FromPhone { get; set; }
        public string FromEmail { get; set; }
        public Address FromAddress { get; set; }

        // sender's information
        public string ToName { get; set; }
        public string ToCompany { get; set; }
        public string ToPhone { get; set; }
        public string ToEmail { get; set; }
        public Address ToAddress { get; set; }
        public Dimension ItemDimension { get; set; }
    }
}
