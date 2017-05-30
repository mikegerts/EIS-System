using EIS.Inventory.Shared.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.Core.ViewModels
{
    public class ProductGroupDto
    {
        public ProductGroupDto()
        {
            Products = new List<ProductDto>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public IEnumerable<ProductDto> Products { get; set; }
        public IEnumerable<string> EisSKUs
        {
            get { return Products.Select(x => x.EisSKU); }
        }
    }
}