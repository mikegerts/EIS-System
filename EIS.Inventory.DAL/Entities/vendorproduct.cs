namespace EIS.Inventory.DAL.Database
{
    public partial class vendorproduct
    {
        public string VendorName
        {
            get { return vendor.Name; }
        }
        public string CompanyName
        {
            get { return vendor.company.Name; }
        }

        public bool IsAlwaysInStock
        {
            get { return vendor.InventoryUpdateFrequency == "AlwaysInStock"; }
        }

        public int AlwaysQuantity
        {
            get { return vendor.AlwaysQuantity ?? 0; }
        }

        public int SafetyQty
        {
            get { return vendor.SafetyQty; }
        }
        public string ReturnsAcceptedOption
        {
            get { return vendor.ReturnsAcceptedOption; }
        }
        public string RefundOption
        {
            get { return vendor.RefundOption; }
        }
        public string ReturnsWithinOption
        {
            get { return vendor.ReturnsWithinOption; }
        }
        public string ReturnPolicyDescription
        {
            get { return vendor.ReturnPolicyDescription; }
        }
        public string ShippingType
        {
            get { return vendor.ShippingType; }
        }
        public string ShippingService
        {
            get { return vendor.ShippingService; }
        }
        public decimal ShippingServiceCost
        {
            get { return vendor.ShippingServiceCost; }
        }
    }
}
