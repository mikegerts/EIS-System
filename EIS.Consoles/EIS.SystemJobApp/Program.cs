using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.SystemJobApp.Repositories;
using EIS.SystemJobApp.Workers;
using EIS.SystemJobApp.Helpers;
using EIS.SystemJobApp.Models;

namespace EIS.SystemJobApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // configure the auto mappings
            AutoMapperConfig.CreateMappings();

            Console.Title = ConfigurationManager.AppSettings["AppName"].ToString();
            var repositoy = new SystemJobRepository();
            var keepRunning = true;
            var jobWorkers = new List<JobWorker>();

            while (keepRunning)
            {
                Console.WriteLine("Reading system jobs from database...");

                var systemJobs = repositoy.GetAllIncompleteJobs();
                // iterate and execute the jobs
                foreach (var job in systemJobs)
                {
                    // check if the task is cancelled
                    if (job.Status == JobStatus.Canceled)
                    {
                        var cancelWorker = jobWorkers.FirstOrDefault(x => x.JobId == job.Id);
                        if (cancelWorker != null)
                        {
                            cancelWorker.CancelJob();
                            // remove it from the list
                            jobWorkers.Remove(cancelWorker);
                        }
                        continue;
                    }

                    // don't add if already in the list
                    var isAlreadyAdded = jobWorkers.Exists(x => x.JobId == job.Id);
                    if (isAlreadyAdded)
                        continue;

                    // create and add the job worker created
                    var jobWorker = createJobWorker(job);
                    jobWorkers.Add(jobWorker);

                    // start the job
                    jobWorker.StartJob();
                }
                
                // let's sleep the main thread for a while
                goToSleep();

                // remove the jobs that are done from the list
                jobWorkers.RemoveAll(x => x.IsJobDone);
            }

            Console.ReadKey();
        }

        private static void goToSleep()
        {
            var sleepTime = Convert.ToInt16(ConfigurationManager.AppSettings["SleepTime"]);
            try
            {
                Thread.Sleep(sleepTime * 1000);
            }
            catch { }
        }

        private static JobWorker createJobWorker(SystemJob job)
        {
            if (job.JobType == JobType.ProductFileUpload)
                return new ProductFileUploadWorker(job);
            else if (job.JobType == JobType.AmazonGetInfo)
                return new AmazonGetInfoWorker(job);
            else if (job.JobType == JobType.BulkDeleteProduct || job.JobType == JobType.BulkDeleteVendorProduct)
                return new BulkDeleteWorker(job);
            else if (job.JobType == JobType.KitFileUpload)
                return new KitFileUploadWorker(job);
            else if (job.JobType == JobType.ShadowFileUpload)
                return new ShadowFileUploadWorker(job);
            else if (job.JobType == JobType.BlacklistedSkuUpload)
                return new BlacklistedSKUFileUploadWorker(job);
            else if (job.JobType == JobType.BulkeBaySuggestedCategories)
                return new eBaySuggestedCategoriesWorker(job);
            else if (job.JobType == JobType.VendorProductFileUpload)
                return new VendorProductFileUploadWorker(job);
            else if (job.JobType == JobType.ShippingRateFileUpload)
                return new ShippingRateFileUploadWorker(job);
            else if (job.JobType == JobType.VendorInventoryFileUpload)
                return new VendorInventoryFileUploadWorker(job);
            else if (job.JobType == JobType.eBayProductsReListing)
                return new eBayProductsReListingWorker(job);
            else if (job.JobType == JobType.eBayProductsEndItem)
                return new eBayProductsEndItemWorker(job);
            else
                throw new ArgumentException("Unknown job type: " + job.JobType);
        }
    }
}
