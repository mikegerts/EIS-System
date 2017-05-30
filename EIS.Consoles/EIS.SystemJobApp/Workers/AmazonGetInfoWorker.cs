using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Threading;
using CsvHelper;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.SystemJobApp.Helpers;
using EIS.SystemJobApp.Marketplaces;
using EIS.SystemJobApp.Models;
using EIS.SystemJobApp.Repositories;

namespace EIS.SystemJobApp.Workers
{
    public class AmazonGetInfoWorker : JobWorker
    {
        private const string _AssociateKey = "balijewe-20";
        private const string _AccessKey = "AKIAJKL2L4RI7IYLPX2Q";
        private const string _SecretKey = "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";
        private readonly ProductRepository _repository;

        public AmazonGetInfoWorker(SystemJob job)
            : base(job)
        {
            _repository = new ProductRepository(_logger);
        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isWorkerExecuted)
                return;

            // set the flag the this bw_DoWork has already called
            _isWorkerExecuted = true;
            var requestor = new AmazonProductAdvertisingRequestor(_AccessKey, _SecretKey, _AssociateKey);
            
            // get the list of ASINs from the file
            var infoFeeds = getProducctAmazonInfoFeeds(_systemJob.Parameters);
            var products = new List<ProductAmazon>();
            var counter = 0;

            setTotalItemsProcessed(infoFeeds.Count);
            _logger.LogInfo(LogEntryType.AmazonGetInfoWorker, string.Format("{0} items for Amazon Get Info has started...", infoFeeds.Count));
            
            try
            {
                // iterate 1 by 1 for the Amazon Get Info feed
                for (var i = 0; i < infoFeeds.Count; i++)
                {
                    var percentage = (((double)i + 1) / infoFeeds.Count) * 100.00;
                    Console.WriteLine(string.Format("{0:#0.00}% Sending feed 1 x 1 for Amazon Get Info...", percentage));
                    try
                    {
                        if (string.IsNullOrEmpty(infoFeeds[i].ASIN))
                            continue;

                        // try to search by UPC
                        var response = requestor.GetProductItemResponse(infoFeeds[i]);
                        if (response.OperationRequest.Errors != null)
                        {
                            _logger.LogError(LogEntryType.AmazonGetInfoWorker, response.OperationRequest.Errors[0].Message,
                                string.Format("{1}: Cannot refind item for EisSKU [\'{0}\']", infoFeeds[i].EisSKU, response.OperationRequest.Errors[0].Code));

                            // let's sleep for a while before going for another request
                            try { Thread.Sleep(500); }
                            catch (Exception) { }

                            continue;
                        }

                        var itemResult = AmazonResponseHelper.GetValidItemResult(response.Items);
                        if (itemResult == null)
                            continue;

                        // parsed and get the product info the the result
                        var productInfo = AmazonResponseHelper.ConstructProductAmazonFromLookupReult(itemResult, infoFeeds[i]);

                        // update the Amazon product detail
                        updateProductsDetail(new List<ProductAmazon> { productInfo });

                        // report the progress
                        counter++;
                        _bw.ReportProgress(counter);

                        // sleep for 1 second - throttle limit
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Error in getting Amazon Get Info for {0}. Error Message: {1}", infoFeeds[i].EisSKU, EisHelper.GetExceptionMessage(ex));
                        _logger.LogError(LogEntryType.AmazonGetInfoWorker, string.Format("Error in getting Amazon Get Info for {0}. Error Message: {1}", infoFeeds[i].EisSKU, EisHelper.GetExceptionMessage(ex)), ex.StackTrace);
                    }
                }

                // log info that the Amazon Get Info is complete
                _logger.LogInfo(LogEntryType.AmazonGetInfoWorker, string.Format("{0}/{1} has been successfully found for \'Get Info from Amazon\'.",
                    counter, infoFeeds.Count));
            }
            catch (Exception ex)
            {
                _hasError = true;
                _logger.LogError(LogEntryType.AmazonGetInfoWorker, "Error in processing Amazon Get Info -> " + EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
            }

            // close the repo db connection
            _repository.CloseDbConnection();
        }

        private void updateProductsDetail(List<ProductAmazon> amazonProducts)
        {
            try
            {
                // iterate and update the EIS product info with data from marketplace
                foreach (var amazonProduct in amazonProducts)
                {
                    _repository.UpdateEisProduct(amazonProduct, _systemJob.SubmittedBy);
                    _repository.DoUpdateOrInsertAmazon(amazonProduct, _systemJob.SubmittedBy);

                    // parsed and save its images
                    if(amazonProduct.Images != null || amazonProduct.Images.Any())
                        _repository.UpdateProductImages(amazonProduct.Images.Select(x => x.Url).ToList(),
                            amazonProduct.EisSKU);
                }
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.AmazonGetInfoWorker, errorMsg, ex.StackTrace);

                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.AmazonGetInfoWorker,
                    string.Format("Error in updating products. <br/> Error message: {0}", ex.InnerException != null ? ex.InnerException.Message : ex.Message),
                    ex.StackTrace);

                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
            }
        }

        protected override void DoPostWorkCompleted()
        {
            // for now, do nothing...
        }

        private List<AmazonInfoFeed> getProducctAmazonInfoFeeds(string filePath)
        {
            var records = new List<AmazonInfoFeed>();
            using (var streamReader = new StreamReader(_systemJob.Parameters))
            {
                var csvReader = new CsvReader(streamReader);
                csvReader.Configuration.HasHeaderRecord = false;
                while (csvReader.Read())
                {
                    var feed = new AmazonInfoFeed
                    {
                        EisSKU = csvReader.GetField(0),
                        ASIN = csvReader.GetField(1),
                        UPC = csvReader.GetField(2),
                    };
                    
                    // only add that has valid data
                    if (feed.HasValidData)
                        records.Add(feed);
                }
            }
            return records;
        }
    }
}
