
namespace EIS.Inventory.DAL.Database
{
    public partial class productebay
    {
        public string CategoryName
        {
            get
            {
                return (CategoryId ?? 0) == 0 
                    ? string.Empty :
                    string.Format("{0} - {1}", CategoryId, ebaystructuredcategory.Name);
            }
        }

        public bool IsItemEnded { get { return EndedItemDate != null; } }
        public bool IsItemReListed { get { return ReListedItemDate != null; } }
    }
}
