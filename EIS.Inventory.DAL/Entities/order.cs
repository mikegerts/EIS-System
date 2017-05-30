using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.DAL.Database
{
    public partial class order
    {
        public int TotalItems
        {
            get { return orderitems.Count; }
        }

        public bool HasOrderProducts
        {
            get
            {
                if (!orderitems.Any())
                    return false;

                return orderitems.Any(x => x.orderproducts.Any());
            }
        }

        public bool IsExported
        {
            get
            {
                if (!orderitems.Any())
                    return false;

                return orderitems.Any(x => x.IsExported);
            }
        }

        public string OrderProductItemSKU
        {
            get
            {
                if (orderitems.Count > 1)
                    return "(Multiple Items)";

                return orderitems.First().OrderProductItemSKU;
            }
        }

        public string OrderProductItemName
        {
            get
            {
                if (orderitems.Count > 1)
                    return "(Multiple Items)";

                return orderitems.First().OrderProductItemName;
            }
        }

        public int OrderProductQuantity
        {
            get { return orderitems.Sum(x => x.OrderProductQuantity); }
        }

        public IEnumerable<orderproduct> OrderProducts
        {
            get { return orderitems.SelectMany(x => x.orderproducts); }
        }
    }
}
