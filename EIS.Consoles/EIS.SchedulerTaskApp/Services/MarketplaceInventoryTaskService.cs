using System;
using System.Configuration;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.SchedulerTaskApp.Marketplaces;
using EIS.SchedulerTaskApp.Repositories;
using EIS.Inventory.Shared.Models;

namespace EIS.SchedulerTaskApp.Services
{
    public class MarketplaceInventoryTaskService : TaskService
    {
        private readonly string _marketplaceMode;
        private readonly marketplaceinventorytask _task;

        public MarketplaceInventoryTaskService(marketplaceinventorytask task)
        {
            _task = task;
            _marketplaceMode = ConfigurationManager.AppSettings["MarketplaceMode"].ToString();
        }

        public override string TaskType
        {
            get { return ScheduledTaskType.MARKETPLACE_INVENTORY; }
        }

        public override bool Execute()
        {
            // get the list of products feed by CompanyID
            var filePath = string.Format("{0}\\{1}", _exportedFileFolder, TaskType);
            var repo = new MarketplaceFeedRepository();
            var productInventoryFeeds = repo.GetProductsByCompany(_task.CompanyId);

            // iterate to each selected marketplace to update the inventory
            foreach (var credentialType in _task.MarketplacesList)
            {
                try
                {
                    // get the list of product items by vendor
                    var credential = repo.GetCredentialsByCompany(_task.CompanyId, credentialType, _marketplaceMode);
                    if(credential == null)
                    {
                        Logger.LogWarning(LogEntryType.MarketplaceInventoryTaskService,
                            string.Format("No credentials set for {0} to Company ID: {1}", credentialType, _task.CompanyId));
                        continue;
                    }

                    // create the inventory provider
                    var inventoryProvider = createMarketplaceProductInventory(credentialType, filePath);
                    if (inventoryProvider == null)
                        continue;

                    // set its credential
                    inventoryProvider.Credential = credential;

                    // submit the feed
                    inventoryProvider.SubmitProductsInventoryFeed(productInventoryFeeds);
                }
                catch (Exception ex)
                {
                    Logger.LogError(LogEntryType.MarketplaceInventoryTaskService,
                        string.Format("Error in submitting inventory feed for {0}. Error message: {1}", credentialType, EisHelper.GetExceptionMessage(ex)),
                        ex.StackTrace);
                }
            }
            return true;
        }

        public override void DoPostExecution()
        {
            // create export file record for this taks for the generated file
            createExportedFilesRecord(_task.Id);
        }

        private IMarketplaceProductInventory createMarketplaceProductInventory(string marketplace, string filePath)
        {
            var triggeredBy = string.Format("{0}-{1}", Constants.APP_NAME, _task.TaskType);

            if (marketplace == "Amazon")
                return new AmazonProductInventory(triggeredBy, filePath);
            else if (marketplace == "BigCommerce")
                return new BigCommerceProductInventory(triggeredBy, filePath);
            else if (marketplace == "eBay")
                return new eBayProductInventory(triggeredBy, filePath);

            return null;
        }
    }
}
