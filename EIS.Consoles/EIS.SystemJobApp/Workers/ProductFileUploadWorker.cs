using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.Linq;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.SystemJobApp.Helpers;
using EIS.SystemJobApp.Models;
using EIS.SystemJobApp.Repositories;

namespace EIS.SystemJobApp.Workers
{
    public class ProductFileUploadWorker : JobWorker
    {
        public ProductFileUploadWorker(SystemJob job)
            : base(job)
        {
        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isWorkerExecuted)
                return;

            // set the flag the this bw_DoWork has already called
            _isWorkerExecuted = true;

            var eisProducts = new List<Product>();
            var amazonProducts = new List<ProductAmazon>();
            var eBayProducts = new List<ProducteBayDto>();
            var bigCommerceProucts = new List<ProductBigCommerceDto>();
            var repository = new ProductRepository(_logger);
            var affectedProducts = 0;
            var isCreateNew = _systemJob.IsAddNewItem;

            // get the Product and/or Amazon details from the file path
            var message = CsvFileDataParser.ParseProductFile(_systemJob.Parameters,
                eisProducts,
                amazonProducts,
                eBayProducts,
                bigCommerceProucts,
                _systemJob.HasHeader);
            var defaultCompany = repository.GetDefaultCompany();
            var totalProducts = eisProducts.Count;
            setTotalItemsProcessed(totalProducts);
            _logger.LogInfo(LogEntryType.ProductFileUploadWorker, string.Format("Uploading {0} products initiated by {1}", totalProducts, _systemJob.SubmittedBy));

            try
            {
                var retValue = 0;
                foreach (var eisProduct in eisProducts)
                {
                    // get the product EIS SKU
                    var eisSKU = eisProduct.EisSKU;
                    if (!isCreateNew && string.IsNullOrEmpty(eisSKU))
                        continue;

                    // update the EIS Product details if there's any
                    if (eisProduct.HasAnyChanges)
                    {
                        var isUpdate = true;
                        if (string.IsNullOrEmpty(eisSKU) && isCreateNew)
                        {
                            eisSKU = repository.GetNextEisSKUForCompany(eisProduct.CompanyId);
                            isUpdate = false;
                        }

                        eisProduct.EisSKU = eisSKU;
                        retValue = repository.DoUpadateOrInsertProduct(eisProduct, isUpdate, _systemJob.SubmittedBy);
                    }

                    // then update the Amazon product details, if there's any
                    var amazonModel = amazonProducts.FirstOrDefault(x => x.EisSKU == eisProduct.EisSKU);
                    if (amazonModel != null && amazonModel.HasAnyChanges)
                    {
                        // set the EIS SKU for the product Amzon
                        amazonModel.EisSKU = eisSKU;
                        retValue = repository.DoUpdateOrInsertAmazon(amazonModel, _systemJob.SubmittedBy);
                    }

                    // update eBay product details, if there's any
                    var eBayModel = eBayProducts.FirstOrDefault(x => x.EisSKU == eisProduct.EisSKU);
                    if (eBayModel != null && eBayModel.HasAnyChanged)
                    {
                        // set the EIS SKU for the product eBay
                        eBayModel.EisSKU = eisSKU;
                        retValue = repository.DoUpadateOrInserteBay(eBayModel, _systemJob.SubmittedBy);
                    }

                    // lastly, update the bigCommerce details, if there's any changed
                    var bigCommerce = bigCommerceProucts.FirstOrDefault(x => x.EisSKU == eisProduct.EisSKU);
                    if(bigCommerce != null && bigCommerce.HasAnyChanged)
                    {
                        // set the EIS SKU for this model
                        bigCommerce.EisSKU = eisSKU;
                        retValue = repository.DoUpdateOrInsertBigCommerce(bigCommerce, _systemJob.SubmittedBy);
                    }

                    affectedProducts += retValue;
                    _bw.ReportProgress(affectedProducts);
                    var percentage = (((double)affectedProducts) / totalProducts) * 100.00;
                    Console.WriteLine(string.Format("{1:#0.00}% Updating EIS/Amazon Product EisSKU: {0}", eisProduct.EisSKU, percentage));
                }

                // close the repo db connection
                repository.CloseDbConnection();
            }
            catch (DbEntityValidationException ex)
            {
                _hasError = true;
                _logger.LogError(LogEntryType.ProductFileUploadWorker, EisHelper.ParseDbEntityValidationException(ex), ex.StackTrace);
                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
            }
            catch (Exception ex)
            {
                _hasError = true;
                _logger.LogError(LogEntryType.ProductFileUploadWorker,
                    string.Format("Error in uploading products. <br/> Error message: {0}", EisHelper.GetExceptionMessage(ex)),
                    ex.StackTrace);
                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
            }

            _logger.LogInfo(LogEntryType.ProductFileUploadWorker, string.Format("{0} products have been successfully uploaded by {1}", affectedProducts, _systemJob.SubmittedBy));
        }

        protected override void DoPostWorkCompleted()
        {
            // for now, do nothing...
        }
    }
}
