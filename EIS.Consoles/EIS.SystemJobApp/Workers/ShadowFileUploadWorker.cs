using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.SystemJobApp.Helpers;
using EIS.SystemJobApp.Models;

namespace EIS.SystemJobApp.Workers
{
    public class ShadowFileUploadWorker : JobWorker
    {
        private readonly string _connectionString;

        public ShadowFileUploadWorker(SystemJob job)
            : base(job)
        {
            _connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;
        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isWorkerExecuted)
                return;

            // set the flag the this bw_DoWork has already called
            _isWorkerExecuted = true;

            // parsed and get the shadows info from the file
            var shadows = new List<Shadow>();
            var message = CsvFileDataParser.ParsedShadowFile(_systemJob.Parameters, shadows, _systemJob.HasHeader);
            var affectedRows = 0;

            // log the total records to be processed
            var totalRecords = shadows.Count;
            setTotalItemsProcessed(totalRecords);
            _logger.LogInfo(LogEntryType.ShadowFileUploadWorker, string.Format("Uploading {0} shadow products initiated by {1}", totalRecords, _systemJob.SubmittedBy));

            // iterate to the parsed shadow products and insert into the database
            foreach (var shadow in shadows)
            {
                // do update or insert new product
                var hasShadowProduct = doInsertOrUpdateProduct(shadow, _systemJob.IsAddNewItem);

                if (hasShadowProduct)
                {
                    // do insert or update shadow
                    doInsertOrUpdateShadow(shadow, _systemJob.IsAddNewItem);

                    // do insert or update productamazon
                    if (!string.IsNullOrEmpty(shadow.Asin))
                        doInsertOrUpdateAmazon(shadow, _systemJob.IsAddNewItem);

                    affectedRows++;
                }

                // log the progress
                _bw.ReportProgress(affectedRows);
                var percentage = (((double)affectedRows) / totalRecords) * 100.00;
                Console.WriteLine(string.Format("{1:#0.00}% Updating Shadow product for ParentSKU: {0}", shadow.ParentSKU, percentage));
            }
        }

        private bool doInsertOrUpdateProduct(Shadow shadow, bool isCreate)
        {
            using (var context = new EisInventoryContext())
            {
                // let's check first if Shadow's ParentSKU exist
                var parentProduct = context.products.FirstOrDefault(x => x.EisSKU == shadow.ParentSKU);
                if (parentProduct == null)
                {
                    _logger.LogWarning(LogEntryType.ShadowFileUploadWorker,
                        string.Format("Parent SKU \"{0}\" for Shadow product \"{1}\" does not exist from the EIS products.",
                        shadow.ParentSKU, shadow.ShadowSKU));
                    return false;
                }

                // create new product if it doesn't exist to database
                var shadowProduct = context.products.FirstOrDefault(x => x.EisSKU == shadow.ShadowSKU);
                if (shadowProduct == null)
                {
                    if (isCreate == false) return false;
                    // create shadow product based on its parent Product details
                    context.products.Add(new product
                    {
                        EisSKU = shadow.ShadowSKU,
                        CompanyId = parentProduct.CompanyId,
                        Name = parentProduct.Name,
                        Description = parentProduct.Description,
                        ShortDescription = parentProduct.ShortDescription,
                        Category = parentProduct.Category,
                        ProductTypeId = parentProduct.ProductTypeId,
                        UPC = parentProduct.UPC,
                        SellerPrice = parentProduct.SellerPrice,
                        PkgLength = parentProduct.PkgLength,
                        PkgWidth = parentProduct.PkgWidth,
                        PkgHeight = parentProduct.PkgHeight,
                        PkgLenghtUnit = parentProduct.PkgLenghtUnit,
                        PkgWeight = parentProduct.PkgWeight,
                        PkgWeightUnit = parentProduct.PkgWeightUnit,
                        ItemLength = parentProduct.ItemLength,
                        ItemWidth = parentProduct.ItemWidth,
                        ItemHeight = parentProduct.ItemHeight,
                        ItemLenghtUnit = parentProduct.ItemLenghtUnit,
                        ItemWeight = parentProduct.ItemWeight,
                        ItemWeightUnit = parentProduct.ItemWeightUnit,
                        EAN = parentProduct.EAN,
                        Brand = parentProduct.Brand,
                        Color = parentProduct.Color,
                        Model = parentProduct.Model,
                        // exclude these data for copying shadows GuessedWeight, AccurateWeight, GuessedShipping, AccurateShipping and its Units
                        IsKit = false,
                        IsBlacklisted = parentProduct.IsBlacklisted,
                        SkuType = SkuType.Shadow,
                        CreatedBy = _systemJob.SubmittedBy,
                        Created = DateTime.UtcNow
                    });
                }
                else
                {
                    // let's update its sku type; just to ensure that this is really a shadow product
                    shadowProduct.SkuType = SkuType.Shadow;
                    shadowProduct.ModifiedBy = _systemJob.SubmittedBy;
                    shadowProduct.Modified = DateTime.UtcNow;
                }

                // save the changes first
                context.SaveChanges();

                return true;
            }
        }

        private bool doInsertOrUpdateAmazon(Shadow shadow, bool isCreate)
        {
            using (var context = new EisInventoryContext())
            {
                // let's check first if Shadow's ParentSKU exist
                var productamazon = context.productamazons.FirstOrDefault(x => x.EisSKU == shadow.ShadowSKU);

                if (productamazon == null)
                {
                    if (isCreate == false) return false;
                    productamazon = new productamazon()
                    {
                        EisSKU = shadow.ShadowSKU,
                        ASIN = shadow.Asin,
                        IsEnabled = false,
                        CreatedBy = _systemJob.SubmittedBy,
                        Created = DateTime.UtcNow
                    };
                    context.productamazons.Add(productamazon);
                }
                else
                {
                    productamazon.ASIN = shadow.Asin;
                    productamazon.ModifiedBy = _systemJob.SubmittedBy;
                    productamazon.Modified = DateTime.UtcNow;
                }

                // save the changes first
                context.SaveChanges();

                return true;
            }
        }

        protected override void DoPostWorkCompleted()
        {
            // for now, do nothing...
        }

        private void doInsertOrUpdateShadow(Shadow shadow, bool isCreate)
        {
            using (var context = new EisInventoryContext())
            {
                var existingShadow = context.shadows.FirstOrDefault(x => x.ShadowSKU == shadow.ShadowSKU);
                if (existingShadow != null)
                {
                    existingShadow.FactorQuantity = shadow.FactorQuantity;
                    if (shadow.isConnectedSet)
                        existingShadow.IsConnected = shadow.IsConnected;
                }
                else
                {
                    if (isCreate == false) return;
                    // create new shadow item
                    context.shadows.Add(new shadow
                    {
                        ParentSKU = shadow.ParentSKU,
                        ShadowSKU = shadow.ShadowSKU,
                        FactorQuantity = shadow.FactorQuantity,
                        IsConnected = shadow.IsConnected
                    });
                }

                // save the chagnes
                context.SaveChanges();
            }
        }

    }
}