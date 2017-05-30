using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.ViewModels
{
    public class BigCommerceCustomFieldDto
    {
        public int Id { get; set; }
        public int? CustomFieldId { get; set; }
        public int? ProductId { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
    }
}
