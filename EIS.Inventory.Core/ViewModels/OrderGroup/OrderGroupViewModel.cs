using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.Core.ViewModels
{
    public class OrderGroupViewModel
    {
        public OrderGroupViewModel()
        {
            Orders = new List<OrderViewModel>();
        }
        public long Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int NumOfItems { get; set; }

        public IEnumerable<OrderViewModel> Orders { get; set; }
        public IEnumerable<string> OrderIds
        {
            get { return Orders.Select(x => x.OrderId); }
        }
    }
}
