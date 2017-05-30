using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using CsvHelper;
using eBay.Service.Call;
using eBay.Service.Core.Sdk;
using eBay.Service.Core.Soap;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.SystemJobApp.Models;
using EIS.SystemJobApp.Repositories;
using EIS.Marketplace.eBay.Helpers;

namespace EIS.SystemJobApp.Workers
{
    public class eBayProductsEndItemWorker : JobWorker
    {
        private readonly ProductRepository _repo;
        private List<ItemFeed> _itemFeeds;

        public eBayProductsEndItemWorker(SystemJob job) : base(job)
        {
            _repo = new ProductRepository();
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
            _logger.LogInfo(LogEntryType.eBayEndListing, string.Format("{0} items for eBay EndListing has started...", _itemFeeds.Count));

            // iterate and submit end item feed to the eBay
            for (var i = 0; i < _itemFeeds.Count; i++)
            {
                var itemFeed = _itemFeeds[i];
                var percentage = (((double)i + 1) / _itemFeeds.Count) * 100.00;
                Console.WriteLine(string.Format("{0:#0.00}% Sending feed 1 x 1 for eBay Product EndItem...", percentage));

                // report the progress
                counter++;
                _bw.ReportProgress(counter);

                try
                {
                    if (string.IsNullOrEmpty(itemFeed.ItemId))
                    {
                        itemFeed.Status = Status.NOT_PROCESSED;
                        itemFeed.Message = "ItemId is NULL or empty";
                        continue;
                    }

                    // create the end item request
                    var apiCall = new EndItemCall(context);
                    apiCall.EndItem(itemFeed.ItemId, EndReasonCodeType.NotAvailable);

                    // let's set the eBay product ItemId to null
                    _repo.UpdateeBayEndedItem(itemFeed.EisSKU);
                    itemFeed.Status = Status.SUCCESS;

                    // sleep for 1 second - throttle limit
                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error in submitting eBay product end listing {0}. Error Message: {1}", _itemFeeds[i].EisSKU, EisHelper.GetExceptionMessage(ex));
                    itemFeed.Status = Status.FAILED;
                    itemFeed.Message = string.Format("Error in sending endlisting feed. Message: {0}", EisHelper.GetExceptionMessage(ex));
                }
            }

            _logger.LogInfo(LogEntryType.eBayEndListing, string.Format("{0} items for eBay EndListing has finished!", _itemFeeds.Count));
        }

        protected override void DoPostWorkCompleted()
        {
            // write the item feed status into the CSV file
            var resultFilePath = string.Format("{0}\\eBayProductsEndItemResults{1}.csv", _resultFileDirecctory, _systemJob.Id);

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
