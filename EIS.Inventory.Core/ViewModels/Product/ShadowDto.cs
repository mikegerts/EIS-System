namespace EIS.Inventory.Core.ViewModels
{
    public class ShadowDto
    {
        public string ParentSKU { get; set; }
        public string ShadowSKU { get; set; }
        public int FactorQuantity { get; set; }
        public bool IsConnected { get; set; }
    }
}
