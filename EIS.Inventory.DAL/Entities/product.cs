using System;
using System.Linq;

namespace EIS.Inventory.DAL.Database
{
    public partial class product
    {
        private vendorproductlink _availableProductLink;
        private shadow _shadowDetail;

        /// <summary>
        /// This is the shadow product details
        /// </summary>
        public shadow ShadowDetail
        {
            get
            {
                if (_shadowDetail == null)
                    _shadowDetail = shadows1.FirstOrDefault();

                return _shadowDetail;
            }
        }

        public int KitQuantity
        {
            get
            {
                if (!IsKit || kit == null || !kit.kitdetails.Any())
                    return 0;

                return kit.kitdetails.Min(x => x.product.Quantity);
            }
        }

        public decimal KitSellerPrice
        {
            get
            {
                if (!IsKit || kit == null || !kit.kitdetails.Any())
                    return 0;

                return kit.kitdetails.Min(x => x.product.SellerPrice);
            }
        }

        public decimal KitSupplierPrice
        {
            get
            {
                if (!IsKit || kit == null || !kit.kitdetails.Any())
                    return 0;

                return kit.kitdetails.Min(x => x.product.SupplierPrice);
            }
        }

        public int FactorQuantity
        {
            get
            {
                return ShadowDetail == null ? 1 : ShadowDetail.FactorQuantity; 
            }
        }

        public int Quantity
        {
            get
            {
                if (AvailableVendorProduct == null)
                    return 0;

                return SkuType == Shared.Models.SkuType.Normal
                    ? AvailableVendorProduct.Quantity
                    : ((AvailableVendorProduct.Quantity * AvailableVendorProduct.MinPack) / FactorQuantity);
            }
        }

        public decimal SupplierPrice
        {
            get
            {
                if (AvailableVendorProduct == null)
                    return 0;

                return SkuType == Shared.Models.SkuType.Normal
                    ? AvailableVendorProduct.SupplierPrice
                    : AvailableVendorProduct.SupplierPrice * (decimal)(FactorQuantity / AvailableVendorProduct.MinPack);
            }
        }

        public bool IsAlwaysInStock
        {
            get { return AvailableVendorProduct == null ? false : AvailableVendorProduct.IsAlwaysInStock; }
        }

        public int AlwaysQuantity
        {
            get { return AvailableVendorProduct == null ? 0 : AvailableVendorProduct.AlwaysQuantity; }
        }

        public string VendorName
        {
            get { return AvailableVendorProduct == null ? string.Empty : AvailableVendorProduct.VendorName; }
        }

        public string EisSupplierSKU
        {
            get { return AvailableVendorProduct == null ? string.Empty : AvailableVendorProduct.EisSupplierSKU; }
        }

        public int VendorId
        {
            get { return AvailableVendorProduct == null ? 0 : AvailableVendorProduct.VendorId; }
        }

        public vendorproduct AvailableVendorProduct
        {
            get
            {
                if (_availableProductLink == null)
                {
                    _availableProductLink = vendorproductlinks
                        .OrderBy(x => x.vendorproduct.SupplierPrice)
                        .FirstOrDefault(x => x.vendorproduct.Quantity > 0 && x.IsActive);
                }

                return _availableProductLink == null ? null : _availableProductLink.vendorproduct;
            }
        }

        public void AppendWeights(decimal newWeight)
        {
            this.Weight_5 = this.Weight_4;
            this.Weight_4 = this.Weight_3;
            this.Weight_3 = this.Weight_2;
            this.Weight_2 = this.Weight_1;
            this.Weight_1 = newWeight;
        }

        public decimal GetMeanWeight()
        {
            decimal meanWeight = this.Weight_1.Value
                                    + ((this.Weight_2.HasValue) ? this.Weight_2.Value : 0)
                                    + ((this.Weight_3.HasValue) ? this.Weight_3.Value : 0)
                                    + ((this.Weight_4.HasValue) ? this.Weight_4.Value : 0)
                                    + ((this.Weight_5.HasValue) ? this.Weight_5.Value : 0);

            meanWeight = meanWeight / WeightMaxCount;

            return meanWeight;
        }

        private int _WeightMaxCount = 0;

        private int WeightMaxCount
        {
            get
            {
                if(_WeightMaxCount == 0)
                {
                    if (this.Weight_1.HasValue && this.Weight_1.Value != 0)
                        _WeightMaxCount++;
                    if (this.Weight_2.HasValue && this.Weight_2.Value != 0)
                        _WeightMaxCount++;
                    if (this.Weight_3.HasValue && this.Weight_3.Value != 0)
                        _WeightMaxCount++;
                    if (this.Weight_4.HasValue && this.Weight_4.Value != 0)
                        _WeightMaxCount++;
                    if (this.Weight_5.HasValue && this.Weight_5.Value != 0)
                        _WeightMaxCount++;
                }

                return _WeightMaxCount;
            }
        }
    }
}
