using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.ViewModels
{
    public class SavedSearchFilterDto
    {
        public int Id { get; set; }
        public int SavedSearchFilterId { get; set; }
        public string SavedSearchFilterName { get; set; }
        public string SearchString { get; set; }
        public string CreatedBy { get; set; }
        public DateTime Created { get; set; }
    }
}
