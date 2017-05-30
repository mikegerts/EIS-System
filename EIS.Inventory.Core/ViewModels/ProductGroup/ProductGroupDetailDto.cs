using System.Collections.Generic;
using X.PagedList;

namespace EIS.Inventory.Core.ViewModels
{
    public class ProductGroupDetailDto
    {
        public ProductGroupDetailDto()
        {
            AddedItems = new List<string>();
            DeletedItems = new List<string>();
        }

        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> AddedItems { get; set; }
        public List<string> DeletedItems { get; set; }
        public IPagedList<ProductListDto> Products { get; set; }
    }    
}
