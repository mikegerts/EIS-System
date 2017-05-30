
namespace EIS.Inventory.Shared.ViewModels
{
    public class Shadow
    {
        public string ParentSKU { get; set; }
        public string SuffixSKU { get; set; }
        public int FactorQuantity { get; set; }
        public bool IsConnected { get; set; }
        public bool isConnectedSet { get; set; }
        public string ShadowSKU
        {
            get
            {
                return string.Format("{0}{1}", ParentSKU, SuffixSKU.Trim());
            }
        }
        public string Asin { get; set; }
    }
}
