using System.Linq;

namespace EIS.Inventory.DAL.Database
{
    public partial class orderitem
    {
        private orderproduct _orderProduct;
        
        public bool IsExported
        {
            get
            {
                if (!orderproducts.Any())
                    return false;

                return orderproducts.Any(x => x.IsExported);
            }
        }

        public string OrderProductItemSKU
        {
            get
            {
                if (orderproducts.Count > 1)
                    return "(Multiple Items)";

                return FirstOrderProduct.EisSupplierSKU;
            }
        }

        public string OrderProductItemName
        {
            get
            {
                if (orderproducts.Count > 1)
                    return "(Multiple Items)";

                return FirstOrderProduct.vendorproduct.Name;

            }
        }

        public int OrderProductQuantity
        {
            get { return orderproducts.Sum(x => x.Quantity); }
        }

        public orderproduct FirstOrderProduct
        {
            get
            {
                if (_orderProduct == null)
                    _orderProduct = orderproducts.FirstOrDefault();

                return _orderProduct;
            }
        }
    }
}
