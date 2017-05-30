using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Linq;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.MwsChannels;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Core.Managers
{
    public class MarketplaceProductManager : IMarketplaceProductManager
    {
        private readonly ICredentialService _credentialService;
        private readonly IProductTypeService _productTypeService;
        private readonly IProductService _productService;
        private readonly ILogService _logger;
        private readonly string _marketplaceMode;

        public MarketplaceProductManager(ICredentialService credentialService,
            IProductTypeService productTypeService,
            IProductService productService,
            ILogService logger)
        {
            Core.Container.SatisfyImportsOnce(this);
            _credentialService = credentialService;
            _productTypeService = productTypeService;
            _productService = productService;
            _logger = logger;
            _marketplaceMode = ConfigurationManager.AppSettings["MarketplaceMode"];
        }

        [ImportMany(typeof(IMarketplaceProductProvider))]
        protected List<IMarketplaceProductProvider> _marketplaeProductProviders { get; set; }

        public MarketplaceProduct GetMarketplaceProductInfo(string marketplaceType, string eisSku)
        {
            // get the eisProduct provider
            var productProvider = getMarketplaceProductProvider(marketplaceType);
            productProvider.MarketplaceCredential = _credentialService.GetCredential(marketplaceType, "LIVE");

            // get the product info feed
            var infoFeed = _productService.GetProductInfoFeeds(new List<string>{ eisSku});
            if(!infoFeed.Any())
                return null;

            // get the product info from the marketplace
            var marketplaceProduct = productProvider.GetProductInfo(infoFeed.FirstOrDefault());
            if (marketplaceProduct == null)
                return null;

            // update the EIS product info with data from marketplace
            updateEisProduct(marketplaceProduct);
            updateProductAmazon(marketplaceProduct as ProductAmazon);

            // parsed and save its images
            _productService.UpdateProductImages((marketplaceProduct as ProductAmazon).Images, marketplaceProduct.EisSKU);

            return marketplaceProduct;
        }

        public List<MarketplaceCategoryDto> GetSuggestedCategories(string marketplaceType, string keyword)
        {
            var provider = getMarketplaceProductProvider(marketplaceType);
            if (provider == null)
            {
                _logger.LogError(LogEntryType.MarketplaceProductManager, string.Format("Unable to find product provider named: {0}", marketplaceType), null);
                throw new ArgumentException(string.Format("Unable to find product provider named: {0}", marketplaceType));
            }

            // let's set first its credential
            provider.MarketplaceCredential = _credentialService.GetCredential(marketplaceType, "LIVE"/**_marketplaceMode**/);

            return provider.GetSuggestedCategories(keyword);
        }

        private IMarketplaceProductProvider getMarketplaceProductProvider(string marketplace)
        {
            return _marketplaeProductProviders.FirstOrDefault(x => x.ChannelName == marketplace);
        }

        private void updateEisProduct(MarketplaceProduct product)
        {
            try
            {
                var eisProduct = _productService.GetProductByEisSKU(product.EisSKU);
                if (eisProduct == null)
                    return;
                
                product.EisSKU = eisProduct.EisSKU;

                //eisProduct.Name = product.ProductTitle;

                // set the EIS product Package's Dimension
                eisProduct.Brand = product.Brand;
                eisProduct.Color = product.Color;
                eisProduct.EAN = product.EAN;
                eisProduct.Model_ = product.Model;

                // determine the product type id of the marketplace product
                eisProduct.ProductTypeId = _productTypeService.ConfigureProductTypeName(((ProductAmazon)product).ProductTypeName, ((ProductAmazon)product).ProductGroup);

                // set the product' package dimension
                if (product.PackageDimension != null)
                {
                    eisProduct.PkgLength = product.PackageDimension.Length.Value;
                    eisProduct.PkgWidth = product.PackageDimension.Width.Value;
                    eisProduct.PkgHeight = product.PackageDimension.Height.Value;
                    eisProduct.PkgLenghtUnit = product.PackageDimension.Length.Unit;

                    // parse the weigh and its unit
                    eisProduct.PkgWeight = product.PackageDimension.Weight.Value;
                    eisProduct.PkgWeightUnit = product.PackageDimension.Weight.Unit;
                }

                // set the EIS product Item's dimension
                if (product.ItemDimension != null)
                {
                    eisProduct.ItemLength = product.ItemDimension.Length.Value;
                    eisProduct.ItemWidth = product.ItemDimension.Width.Value;
                    eisProduct.ItemHeight = product.ItemDimension.Height.Value;
                    eisProduct.ItemLenghtUnit = product.ItemDimension.Length.Unit;

                    // parse the weigh and its unit
                    eisProduct.ItemWeight = product.ItemDimension.Weight.Value;
                    eisProduct.ItemWeightUnit = product.ItemDimension.Weight.Unit;
                }

                // save the chnages
                _productService.UpdateProduct(eisProduct.EisSKU, eisProduct);
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.MarketplaceProductManager, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
            }
        }

        private void updateProductAmazon(ProductAmazon amazon)
        {
            try
            {
                var isExistAmazonProduct = true;
                var existingAmazonProduct = (ProductAmazon)_productService.GetMarketplaceProductInfo("Amazon", amazon.EisSKU);
                if (existingAmazonProduct == null)
                {
                    isExistAmazonProduct = false;
                    existingAmazonProduct = new ProductAmazon { EisSKU = amazon.EisSKU };
                }

                // these are only known properties return from Amazon
                existingAmazonProduct.PackageQty = amazon.PackageQty;
                existingAmazonProduct.ProductTitle = amazon.ProductTitle;
                existingAmazonProduct.MapPrice = amazon.MapPrice;
                existingAmazonProduct.ProductGroup = amazon.ProductGroup;
                existingAmazonProduct.ProductTypeName = amazon.ProductTypeName;
                existingAmazonProduct.Condition = amazon.Condition;
                //existingAmazonProduct.WeightBox1 = amazon.WeightBox1;
                //existingAmazonProduct.WeightBox1Unit = amazon.WeightBox1Unit;
                //existingAmazonProduct.WeightBox2 = amazon.WeightBox2;
                //existingAmazonProduct.WeightBox2Unit = amazon.WeightBox2Unit;

               // convert it into Amazon product dto
                var amazonProductDto = new ProductAmazonDto();
                CopyObject.CopyFields(existingAmazonProduct, amazonProductDto);
                amazonProductDto.ModifiedBy = Apps.EIS_WEBSITE;

                // save the changes
                if (isExistAmazonProduct)        
                    _productService.UpdateProductAmazon(amazon.EisSKU, amazonProductDto);
                else
                    _productService.SaveProductAmazon(amazonProductDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.MarketplaceProductManager, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
            }
        }
    }
}
