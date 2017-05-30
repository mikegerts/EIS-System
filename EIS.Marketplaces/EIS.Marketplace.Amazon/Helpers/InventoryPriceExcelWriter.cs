using CsvHelper.Configuration;
using EIS.Inventory.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace EIS.Marketplace.Amazon.Helpers
{
    public class InventoryPriceExcelWriter
    {
        public static string CreateAmazonFeedInventoryTextFile(List<AmazonInventoryFeed> inventoryItems, string filePath)
        {
            var fileFullPath = string.Format("{0}\\AmazonInventoryFeed-{1:yyyyMMdd_HHmmss}.txt",
                filePath,
                DateTime.Now);

            // remove if there's any null and also Amazon is not enabled


            // iterate to each item and put it into a file
            using (var streamWriter = new StreamWriter(fileFullPath))
            {
                var configuration = new CsvConfiguration();
                configuration.HasHeaderRecord = true;
                configuration.Delimiter = "\t";
                var writer = new CsvHelper.CsvWriter(streamWriter, configuration);

                // write first the file heardes
                writer.WriteField("sku");
                writer.WriteField("price");
                writer.WriteField("minimum-seller-allowed-price");
                writer.WriteField("maximum-seller-allowed-price");
                writer.WriteField("quantity");
                writer.WriteField("leadtime-to-ship");
                writer.WriteField("fulfillment-channel");
                writer.NextRecord();

                foreach (var item in inventoryItems)
                {
                    writer.WriteField(item.SKU);
                    writer.WriteField(null);
                    writer.WriteField(null);
                    writer.WriteField(null);
                    writer.WriteField(item.InventoryQuantity); // quantity
                    writer.WriteField(null);
                    writer.WriteField(null);
                    writer.NextRecord();
                }
            }

            return fileFullPath;
        }
    }
}
