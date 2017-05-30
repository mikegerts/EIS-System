
namespace EIS.Inventory.Core.ViewModels
{
    public class MarketplaceCategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ParentName { get; set; }
        public string OptionName
        {
            get { return string.Format("{0} - {1}", Id, Name); }
        }
    }
}
