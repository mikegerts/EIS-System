using EIS.SchedulerTaskApp.Helpers;
using EIS.SchedulerTaskApp.Models;
using System.Collections.Generic;

namespace EIS.Inventory.Test
{
    //[TestClass]
    public class CsvHelperExcelTests
    {
        //[TestMethod]
        public void ShouldWriteExcelFile()
        {
            var items = new List<MarketplaceInventoryUpdateItem>
            {
                new MarketplaceInventoryUpdateItem{
                    SKU = "1233123132",
                    SafetyQty = 2,
                    Quantity = 23
                },
                new MarketplaceInventoryUpdateItem{
                    SKU = "12331223442",
                    SafetyQty = 12,
                    Quantity = 56
                },
                new MarketplaceInventoryUpdateItem{
                    SKU = "1233123145",
                    SafetyQty = 12,
                    Quantity = 52
                },
            };
            InventoryPriceExcelWriter.CreateAmazonFeedInventoryFile(items);
        }
    }
}
