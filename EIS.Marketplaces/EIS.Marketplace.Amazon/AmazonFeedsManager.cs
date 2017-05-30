using StockManagement.Core.MarketPlaces;
using StockManagement.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.MarketPlace.Amazon
{
    public class AmazonFeedsManager : IFeedsManager
    {
        private readonly IProductService _productService;

        public AmazonFeedsManager(IProductService productService)
        {
            _productService = productService;
        }

        public void SubmitFeeds()
        {
            // get the items to send to amazon
            var products = _productService.GetAllProducts();
        }
    }
}
