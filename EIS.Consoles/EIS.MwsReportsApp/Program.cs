using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.MwsReportsApp
{
    public class Program
    {
        [ImportMany(typeof(IMarketplaceReport))]
        private static List<IMarketplaceReport> _marketplaceReports;

        public static void Main(string[] args)
        {
            Console.Title = "EIS MWS Reports Service";
            Thread.CurrentThread.Name = "EIS MWS Reports Service";

            // init the bindings for report manager
            initMarketplaceReportManagers();
            var isKeepRunning = true;

            Console.WriteLine("Enter the start date for fetching the settlement report (yyyy-mm-dd).");
            var dateStr = Console.ReadLine();

            DateTime createdAfter = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(dateStr))
                createdAfter = Convert.ToDateTime(dateStr);

            Console.WriteLine("Please enter \'kigwa\' if do you want to include the acknowledged reports");
            var kigwa = Console.ReadLine();
            var isIncludeAcknowledgedReport = kigwa == "kigwa";

            while (isKeepRunning)
            {
                var settlementItems = new List<SettlementReportDto>();
                var tasks = new List<Task>();
                for (var index = 0; index < _marketplaceReports.Count(); index++)
                {
                    // create temp for the counter
                    var tempIndex = index;
                    var task = Task.Factory.StartNew(() =>
                    {
                        var worker = new MarketplaceWorker(_marketplaceReports[tempIndex]);
                        var result = worker.GetSettlementReport(createdAfter, isIncludeAcknowledgedReport);
                        if (result != null)
                            settlementItems.Add(result);

                    });

                    tasks.Add(task);
                }

                //Parent thread will be terminated after timeout anyway
                Task.WaitAll(tasks.ToArray());
                tasks.ForEach(t => t.Dispose());

                if (settlementItems.Any())
                    SettlementDataManager.InsertSettlementReportToDb(settlementItems);

                // let's sleep for 1 minutes
                Console.WriteLine("{0:HH:mm:ss}: The program goes to sleep for 1 hour...", DateTime.Now);
                try { Thread.Sleep(1000 * 3600); }
                catch { }

                // let's set the created to today's date
                createdAfter = DateTime.UtcNow.Date;
                isIncludeAcknowledgedReport = false;
            }

            Console.WriteLine("MWS Reports service app terminated.");
            Console.ReadKey(true);
        }

        private static void initMarketplaceReportManagers()
        {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));

            //Create the CompositionContainer with the parts in the catalog
            var container = new CompositionContainer(catalog);

            // get the marketplace report manager
            _marketplaceReports = container.GetExportedValues<IMarketplaceReport>().ToList();
        }
    }
}
