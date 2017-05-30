using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.DAL.Database
{
    public partial class marketplaceinventorytask
    {
        public List<string> MarketplacesList
        {
            get { return string.IsNullOrEmpty(Marketplaces) ? null : Marketplaces.Split(',').ToList(); }
        }
    }
}
