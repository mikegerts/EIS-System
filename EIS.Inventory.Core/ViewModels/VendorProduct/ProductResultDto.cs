namespace EIS.Inventory.Core.ViewModels
{
    public class ProductResultDto
    {
        public string EisSKU { get; set; }
        public string Name { get; set; }
        public string DisplayName
        {
            get { return string.Format("{0} - {1}", EisSKU, Name); }
        }
    }
}
