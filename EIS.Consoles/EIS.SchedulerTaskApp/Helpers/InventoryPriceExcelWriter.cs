using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using CsvHelper.Configuration;
using EIS.SchedulerTaskApp.Models;
using NPOI.HSSF.UserModel;
using EIS.Inventory.Shared.Models;

namespace EIS.SchedulerTaskApp.Helpers
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

        public static string CreateAmazonFeedInventoryFile(List<MarketplaceInventoryUpdateItem> inventoryFeeds)
        {            
            //var template = @"D:\dev\kigwa\EShopo System\EIS.Inventory\EIS.Consoles\EIS.SchedulerTaskApp\Templates\Flat.File.PriceInventory.xls";
            var template = string.Format("{0}\\Flat.File.PriceInventory.xls", ConfigurationManager.AppSettings["TemplatesRoot"]);            
            var newFile = string.Format("{0}\\AmazonInventoryFeed-{1:yyyyMMdd_HHmmss}.xls",
                ConfigurationManager.AppSettings["MarketplaceFeedRoot"],
                DateTime.Now);

            HSSFWorkbook workbook;
            using (var fs = new FileStream(template, FileMode.Open, FileAccess.Read))
            {
                // Getting the complete workbook...
                workbook = new HSSFWorkbook(fs);
                fs.Close();
            }
            
            // Getting the worksheet by its name...             
            var sheet = workbook.GetSheet("Price Template"); 

            for (var index = 1; index <= inventoryFeeds.Count; index++)
            {
                var row = sheet.CreateRow(index);
                row.CreateCell(0).SetCellValue(inventoryFeeds[index - 1].SKU);
                //row.CreateCell(4).SetCellValue(inventoryFeeds[index - 1].InventoryQuantity);
                row.CreateCell(4).SetCellValue(101);
            }
                       
            // Writing the workbook content to the FileStream... 
            using (var fileStream = new FileStream(newFile, FileMode.Create))
            {
                workbook.Write(fileStream);
                fileStream.Flush();
                fileStream.Close();
            }

            return newFile;
        }
    }
}
