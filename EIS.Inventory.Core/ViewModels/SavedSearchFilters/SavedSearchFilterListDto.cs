using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.ViewModels
{
    public class SavedSearchFilterListDto
    {
        public int Id { get; set; }
        public int SavedSearchFilterId { get; set; }
        public int SavedSearchFilterName { get; set; }
        public string SearchString { get; set; }
    }
}
