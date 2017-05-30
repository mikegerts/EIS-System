namespace EIS.Inventory.DAL.Database
{
    public partial class vendor
    {
        public bool IsAlwaysInStock
        {
            get { return InventoryUpdateFrequency == "AlwaysInStock"; }
        }
    }
}
