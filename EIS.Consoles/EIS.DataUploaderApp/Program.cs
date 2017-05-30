using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading;

namespace EIS.DataUploaderApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Title = "EIS Data File Service";
            Thread.CurrentThread.Name = "EIS Data File Service Main";

            var connectionString = ConfigurationManager.ConnectionStrings["VendorsConfigConnection"].ConnectionString;
            var productFileWorker = new ProductFileWorker(connectionString);
            var inventoryFileWorker = new InventoryFileWorker(connectionString);

            var keepRunning = true;

            while (keepRunning)
            {
                Console.WriteLine("Fetching product and inventory settings from database...");

                productFileWorker.ReadAndParsedFile();
                inventoryFileWorker.ReadAndParsedFile();

                // let's sleep the main thread for a while
                goToSleep();
            }

            Console.ReadKey();
        }

        private static void goToSleep()
        {
            var sleepTime = Convert.ToInt16(ConfigurationManager.AppSettings["SleepTime"]);
            try
            {
                // sleep in seconds
                Thread.Sleep(sleepTime * 1000);
            }
            catch { }
        }
    }
}
