using System;
using System.Linq;
using AutoMapper;
using EIS.Inventory.Core.Models;
using EIS.Inventory.DAL.Database;
using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public class LogService : ILogService
    {
        public void Add(LogEntrySeverity severity, LogEntryType entryType, string description, string stackTrace)
        {
            using (var context = new EisInventoryContext())
            {
                context.logs.Add(new log
                {
                    Created = DateTime.Now,
                    Severity = (int)severity,
                    EntryType = (int)entryType,
                    Description = description,
                    StackTrace = stackTrace
                });

                context.SaveChanges();
            }    
        }

        public void AddRequestReport(MarketplaceRequestReport requestReportModel)
        {
            var report = Mapper.Map<MarketplaceRequestReport, requestreport>(requestReportModel);            

            using (var context = new EisInventoryContext())
            {
                context.requestreports.Add(report);
                context.SaveChanges();
            }            
        }

        public void UpdateRequestReport(MarketplaceRequestReport requestReportModel)
        { 
            using (var context = new EisInventoryContext())
            {
                // find the existing report request by id
                var oldRequestReport = context.requestreports
                        .FirstOrDefault(x => x.ReportRequestId == requestReportModel.ReportRequestId);

                context.Entry(oldRequestReport).CurrentValues.SetValues(requestReportModel);
                context.SaveChanges();
            }   
        }

        public void AddProcessingReport(MarketplaceProcessingReport processingReportModel)
        {
            var report = Mapper.Map<MarketplaceProcessingReport, processingreport>(processingReportModel);
            report.Created = DateTime.UtcNow;

            using (var context = new EisInventoryContext())
            {
                context.processingreports.Add(report);

                foreach (var resultModel in processingReportModel.ReportResults)
                {
                    var result = Mapper.Map<MarketplaceProcessingReportResult, processingreportresult>(resultModel);
                    context.processingreportresults.Add(result);
                }

                context.SaveChanges();
            }
        }

        public void AddProductItemFees(ItemContainer item, bool isUpdateItemId = true)
        {
            using (var context = new EisInventoryContext())
            {
                // let's update first the product ebay' Item Id
                if (isUpdateItemId)
                {
                    var eBayProduct = context.productebays.FirstOrDefault(x => x.EisSKU == item.EisSKU);
                    eBayProduct.ItemId = item.ItemId;

                    // save the changes first for the product eBay
                    context.SaveChanges();
                }

                var created = DateTime.UtcNow;

                // add the item fees
                foreach (var fee in item.Fees)
                {
                    context.postingfees.Add(new postingfee
                    {
                        ItemId = item.ItemId,
                        FeeName = fee.Name,
                        CurrencyCode = fee.CurrencyCode,
                        Amount = (decimal)fee.Amount,
                        ActionFeedType = fee.ActionFeedType,
                        Created = created
                    });
                }

                // save the changes
                context.SaveChanges();
            }
        }

        public void SeteBayItemIdToNull(List<ItemFeed> endItems)
        {
            using (var context = new EisInventoryContext())
            {
                var eisSkus = endItems.Select(x => x.EisSKU);

                // get the product eBay to update
                var eBayProducts = context.productebays.Where(x => eisSkus.Contains(x.EisSKU));
                foreach (var product in eBayProducts)
                {
                    product.ReListedItemDate = null;
                    product.EndedItemDate = DateTime.Now;
                }

                // let's save the changes
                context.SaveChanges();
            }
        }

        public void LogWarning(LogEntryType entryType, string description)
        {
            Add(LogEntrySeverity.Warning, entryType, description, string.Empty);
        }

        public void LogError(LogEntryType entryType, string description, string stackTrace)
        {
            Add(LogEntrySeverity.Error, entryType, description, stackTrace);
        }

        public void LogInfo(LogEntryType entryType, string description)
        {
            Add(LogEntrySeverity.Information, entryType, description, string.Empty);
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            // Dispose other managed resources
        }
        #endregion
    }
}
