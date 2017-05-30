using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.Shared.Models
{
    public class OrderProductResult
    {
        public OrderProductResult()
        {
            OrderProducts = new List<OrderProduct>();
        }
        public int ItemPack { get; set; }
        public MarketplaceOrderItem OrderItem { get; set; }
        public List<OrderProduct> OrderProducts { get; set; }
        public int TotalOrderedItems { get { return OrderItem.QtyOrdered * ItemPack; } }
        public int TotalAvailableItems { get { return OrderProducts.Sum(o => (o.Quantity * o.Pack)); } }
    }

    public class OrderProduct
    {
        public string EisSupplierSKU { get;set;}
        public int Quantity { get;set;}
        public int Pack { get; set; }
    }
}
