using CsvHelper;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.Marketplace.eBay.Helpers;
using EIS.SystemJobApp.Models;
using EIS.SystemJobApp.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace EIS.SystemJobApp.Workers
{
    public class eBayProductsReListingWorker : JobWorker
    {
        private List<ItemFeed> _itemFeeds;
        private readonly IProductService _service;
        private readonly ProductRepository _repo;

        public eBayProductsReListingWorker(SystemJob job) : base(job)
        {
            _repo = new ProductRepository();
            _service = new ProductService(new ImageHelper(new PersistenceHelper()), new LogService());
        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isWorkerExecuted)
                return;

            // set the flag the this bw_DoWork has already called
            _isWorkerExecuted = true;
            var counter = 0;
            var service = new CredentialService();
            var credential = (eBayCredentialDto)service.GetCredential(CredentialType.EBAY, _systemJob.SupportiveParameters);
            _itemFeeds = readProductItemFeeds(_systemJob.Parameters);

            // init the eBay API
            RequestHelper.SetCredentials(credential);

            // init the context
            var context = new ApiContext();
            context.ApiCredential = RequestHelper.ApiCredential;
            context.SoapApiServerUrl = RequestHelper.ServiceUrl;

            setTotalItemsProcessed(_itemFeeds.Count);
            _logger.LogInfo(LogEntryType.eBayEndListing, string.Format("{0} items for eBay Product ReListing has started...", _itemFeeds.Count));

            // iterate and submit end item feed to the eBay
            for (var i = 0; i < _itemFeeds.Count; i++)
            {
                var percentage = (((double)i + 1) / _itemFeeds.Count) * 100.00;
                Console.WriteLine(string.Format("{0:#0.00}% Sending feed 1 x 1 for eBay Product ReListing...", percentage));

                // report the progress
                counter++;
                _bw.ReportProgress(counter);
                var itemFeed = _itemFeeds[i];

                try
                {
                    // get the product detailf or the product relisting
                    var productFeed = _service.GetProductPostFeedByEisSku(_itemFeeds[i].EisSKU);

                    if (string.IsNullOrEmpty(itemFeed.ItemId))
                    {
                        itemFeed.Status = Status.NOT_PROCESSED;
                        itemFeed.Message = "ItemId is NULL or empty";
                        continue;
                    }

                    // build item type 
                    var itemType = RequestHelper.CreateItemType(productFeed, credential.eBayDescriptionTemplate);
                    itemType.ItemID = productFeed.eBayProductFeed.ItemId;

                    // submit the item for relisting
                    var apiCall = new RelistFixedPriceItemCall(context);
                    apiCall.Item = itemType;
                    apiCall.Execute();

                    // update the product ItemId
                    _repo.UpdateReListedeBayProduct(itemFeed.EisSKU, apiCall.ItemID);

                    itemFeed.Status = Status.SUCCESS;
                    itemFeed.Message = string.Format("{0} - New relisted ItemId: {1}", apiCall.ApiResponse.Ack, apiCall.ItemID);

                    // sleep for 1 second - throttle limit
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in submitting eBay Product ReListing {0}. Error Message: {1}", _itemFeeds[i].EisSKU, EisHelper.GetExceptionMessage(ex));
                    itemFeed.Status = Status.FAILED;
                    itemFeed.Message = string.Format("Error in sending relisting feed. Message: {0}", EisHelper.GetExceptionMessage(ex));
                }
            }

            _logger.LogInfo(LogEntryType.eBayEndListing, string.Format("{0} items for eBay EndListing has finished!", _itemFeeds.Count));
        }

        protected override void DoPostWorkCompleted()
        {
            // write the item feed status into the CSV file
            var resultFilePath = string.Format("{0}\\eBayProductsReListingItemResults{1}.csv", _resultFileDirecctory, _systemJob.Id);

            // write into the file
            using (var streamWriter = new StreamWriter(resultFilePath))
            {
                var writer = new CsvHelper.CsvWriter(streamWriter);

                // write the column headers
                writer.WriteField("EisSKU");
                writer.WriteField("ItemId");
                writer.WriteField("Status");
                writer.WriteField("Message");
                writer.NextRecord();

                foreach (var item in _itemFeeds)
                {
                    writer.WriteField(item.EisSKU);
                    writer.WriteField(item.ItemId);
                    writer.WriteField(item.Status);
                    writer.WriteField(item.Message);

                    // move to the next row
                    writer.NextRecord();
                }
            }

            // update the system job parameters out for the file path of the result file
            _jobRepository.UpdateSystemJobParametersOut(_systemJob.Id, resultFilePath);
        }

        private List<ItemFeed> readProductItemFeeds(string filePath)
        {
            var results = new List<ItemFeed>();

            // read the file
            using (var streamReader = new StreamReader(filePath))
            {
                var csvReader = new CsvReader(streamReader);
                csvReader.Configuration.HasHeaderRecord = false;
                while (csvReader.Read())
                {
                    results.Add(new ItemFeed
                    {
                        EisSKU = csvReader.GetField(0),
                        ItemId = csvReader.GetField(1)
                    });
                }
            }

            return results;
        }
    }
}
