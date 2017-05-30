using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CsvHelper;
using EIS.SystemJobApp.Marketplaces;
using EIS.SystemJobApp.Models;
using EIS.Inventory.Shared.Models;

namespace EIS.SystemJobApp.Workers
{
    public class eBaySuggestedCategoriesWorker : JobWorker
    {
        private const string _AppId = "MikeGert-EshopoIn-PRD-c99eeec0d-398c2471";
        private const string _DevId = "d8e0124a-5a63-4ed9-837e-7007ab23dc97";
        private const string _CertId = "PRD-99eeec0d62f3-d4b0-452e-975e-3260";
        private const string _UserToken = "AgAAAA**AQAAAA**aAAAAA**+693Vw**nY+sHZ2PrBmdj6wVnY+sEZ2PrA2dj6AFkYunCpGAog6dj6x9nY+seQ**b1gDAA**AAMAAA**YLcBqF5Immyn9wi6om4+5CzSH1cTG1FaiVyYEN7Zj6++w6ccm8gvrnw06sUcHeGDPoYXaut5r5O1hVj3/uX+qbuibMSaO2Qd+K3rZkCeHy/cNRwsaOwx7ierR9bE5l6dITf/RS5VfFIZ9b4f4ARXmzycnuCuIveceOwPInSJOuFhALyCT6gkrgZ5N1iFDNN8KqaiIIusDNAUAmOfdHByCqeEv8E44ktkJGgmrvYSCMaLnjQYn5VYVFLeOQmhtQVwZWOUxnnUTOEvDUdBRiuHTrqVIUy7CP0LGKpuWeqay2xLSYLcgvcKTeZykHJErimP/suhOMHB9PlQAc+OIeVzsp95cusuMJNXOqb3XwPKzJHMa3zUiLdr17lIkkTe83uEXVhHv0DG5tWIp1+1JWQxCTbRInoEFN6wat+kijS2nwWjPDDPylI8ToYefc2jVdhEwkUwPFv0OXPmp4QQoNMh437f+1xwKi6p6KEP+Q21pr4krDsE0XLh36aHNmg0WTj3Wf1ehMTmeeLoExK+YG2TnPKBWIzXsd67Q2e1ZONe/efQZaUMbTy3UR2uqTPcZsWEpiShiCOppWGc3zCX7C/XgmMjgSWTMejhLyh8Y2eqf+xkXFfj3iQGuKr50kAp0TzpzBZG1LSJuLNBI+gxfXwteT8yX5NCHXX/MCWSIobL+Kb7Cq0r2x63oumUJkkCi2hXK5rckLbar+ukpR/1hpAxlOUckgs87qOimwN+p7z27LelPoohzTfJR3RJ9Ccq+fjg";
        private readonly string _systemJobDirectory;
            
        public eBaySuggestedCategoriesWorker(SystemJob job)
            : base(job)
        {
            _systemJobDirectory = ConfigurationManager.AppSettings["SystemJobsRoot"].ToString();
        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isWorkerExecuted)
                return;

            // set the flag the this bw_DoWork has already called
            _isWorkerExecuted = true;
            var productCategoryFeeds = parseeBayProductCategoryFile(_systemJob.Parameters);

            // get the null or empty keywords
            var noKeywordsProducts = productCategoryFeeds.Where(x => string.IsNullOrEmpty(x.Keyword));

            if (noKeywordsProducts.Any())
            {
                _logger.LogInfo(LogEntryType.eBaySuggestedCategoriesWorker, string.Format("{0} out of {1} product items will not included in getting suggested categories due to no keywords data.", noKeywordsProducts.Count(), productCategoryFeeds.Count));
                productCategoryFeeds.RemoveAll(x => string.IsNullOrEmpty(x.Keyword));
            }
            
            // get the total records to be processed
            var totalRecords = productCategoryFeeds.Count;
            setTotalItemsProcessed(totalRecords);

            Console.WriteLine("Starting to get eBay suggested categories for {0} product items.", productCategoryFeeds.Count);
            var eBayProvider = new eBayProductCategoryRequestor(_AppId, _DevId, _CertId, _UserToken);
            var counter = 0;
            var tasks = new List<Task>();
            var results = new List<eBayCategoryResult>();

            foreach (var item in productCategoryFeeds)
            {
                var result = eBayProvider.GeteBaySuggestedCategories((string)item.EisSKU, (string)item.Keyword);
                if (result != null)
                    results.Add(result);

                // report the progress
                counter++;
                _bw.ReportProgress(counter);
                var percentage = (((double)counter) / totalRecords) * 100.00;
                Console.WriteLine(string.Format("{2:#0.00}% Done retrieving eBay Categories for: {0} \t Keyword: {1}", (object)item.EisSKU, (object)item.Keyword, percentage));
            }
            
            // write suggested categories result into a file and get the file name
            var fileName = writeSuggestesCategoriesToFile(results);
            _jobRepository.UpdateSystemJobParametersOut(_systemJob.Id, fileName);

            Console.WriteLine("Done eBay getting suggested categories for {0} product items", totalRecords);
            _logger.LogInfo(LogEntryType.eBaySuggestedCategoriesWorker, string.Format("{0}/{1} have been successfully get eBay suggestted categories.", results.Count, totalRecords));
        }

        private string writeSuggestesCategoriesToFile(List<eBayCategoryResult> categoryResults)
        {
            // create the file to save the product categories
            var fileFullPath = string.Format("{0}\\eBaySuggestedCateories_{1:yyyyMMdd_HHmmss}.csv", _systemJobDirectory, DateTime.Now);
            
            // write into the file
            using (var streamWriter = new StreamWriter(fileFullPath))
            {
                var writer = new CsvHelper.CsvWriter(streamWriter);

                // let's write first the column's header for the file
                writer.WriteField("EisSKU");
                for (var i = 1; i <= 10; i++)
                {
                    writer.WriteField(string.Format("eBayCategory{0}", i));
                    writer.WriteField(string.Format("FullPath{0}", i));
                }

                // let's move to next row
                writer.NextRecord();

                // then le't write its content
                foreach (var result in categoryResults)
                {
                    writer.WriteField(result.EisSKU);
                    foreach (var category in result.Categories)
                    {
                        writer.WriteField(category.Id);
                        writer.WriteField(category.OptionName);
                    }
                    
                    writer.NextRecord();
                }
            }

            return fileFullPath;
        }

        protected override void DoPostWorkCompleted()
        {
            // for now, do nothing...
        }

        private List<eBayCategoryFeed> parseeBayProductCategoryFile(string filePath)
        {
            var results = new List<eBayCategoryFeed>();
            
            // read the file
            using (var streamReader = new StreamReader(filePath))
            {
                var csvReader = new CsvReader(streamReader);
                csvReader.Configuration.HasHeaderRecord = false;
                while (csvReader.Read())
                {
                    results.Add(new eBayCategoryFeed
                    {
                        EisSKU = csvReader.GetField(0),
                        Keyword = csvReader.GetField(1)
                    });
                }
            }

            return results;
        }
    }
}
