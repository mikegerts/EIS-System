namespace EIS.Inventory.Core.ViewModels
{
    public class RegionViewModel
    {
        public int Id { get; set; }
        public int CountryId { get; set; }
        public string Name { get; set; }

        public virtual CountriesViewModel country { get; set; }
    }
}
