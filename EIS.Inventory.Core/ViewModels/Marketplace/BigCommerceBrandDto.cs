using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.ViewModels
{
    public class BigCommerceBrandDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PageTitle { get; set; }
        public string ImageFile { get; set; }
        public string OptionName
        {
            get
            {
                return this.Id + " - " + this.Name;
            }
        }
    }
}
