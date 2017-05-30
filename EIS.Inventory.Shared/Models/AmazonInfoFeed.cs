namespace EIS.Inventory.Shared.Models
{
    public class AmazonInfoFeed
    {
        public string EisSKU { get; set; }
        public string ASIN { get; set; }
        public string UPC { get; set; }
        public string EAN { get; set; }

        // helper method
        public bool HasValidData
        {
            get {
                return !string.IsNullOrEmpty(ASIN)
                  || !string.IsNullOrEmpty(UPC)
                  || !string.IsNullOrEmpty(EAN);
            }
        }
    }
}
