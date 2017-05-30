using System;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Threading;
using EIS.OrdersServiceApp.ShippingServices;
using EIS.OrdersServiceApp.Helpers;

namespace EIS.OrdersServiceApp {
    public class Program
    {
        public static void Main(string[] args)
        {
            // configure the auto mappings
            AutoMapperConfig.CreateMappings();

            if (args.Length > 0)
            {
                string argument = args[0];

                switch (argument)
                {
                    case "RUNACCURATEWEIGHTPREVDATA":
                        Console.WriteLine("Enter the end date of fetching the accurate (yyyy-mm-dd).");
                        var dateStr = Console.ReadLine();

                        if (!string.IsNullOrEmpty(dateStr))
                        {
                            ExecuteOneTimeRunAccurateWeight(Convert.ToDateTime(dateStr));
                        }
                        break;
                    default:
                        ExecuteMainProcess();
                        break;
                }


                Console.ReadKey(true);
            }
            else
            {
                ExecuteMainProcess();
            }
        }

        private static void ExecuteMainProcess()
        {

            Console.Title = ConfigurationManager.AppSettings["AppName"].ToString();

            Console.WriteLine("Enter the start date of fetching the orders (yyyy-mm-dd).");
            var dateStr = Console.ReadLine();

            DateTime orderCreatedAfter = DateTime.Now.Date;
            if (!string.IsNullOrEmpty(dateStr))
                orderCreatedAfter = Convert.ToDateTime(dateStr);

            Console.WriteLine("Starting the MWS Orders server...");
            var ordersManager = initMarketplaceManager();
            var inventoryManager = new ProductInventoryManager();
            bool isKeepRunning = true;

            while (isKeepRunning)
            {
                Console.WriteLine("Fetch the orders from marketplaces and insert them to the database...");
                // fetch the orders from marketplaces and insert them to the database
                var orders = ordersManager.DownloadMarketplaceOrders(orderCreatedAfter.AddHours(-3));

                Console.WriteLine("Let's do the product inventory update based on the orders...");
                // let's do the vendor product inventory update based on the orders
                inventoryManager.UpdateOrderVendorProductInventory(orders);

                Console.WriteLine("Starting to post unshipped orders to ShipStation...");
                // post unshipped orders to ShipStation
                using (var shippingStationOrder = new ShipStationOrders())
                {
                    var result_task = shippingStationOrder.PostOrderToShippingStation();
                    result_task.Wait();
                    var result = result_task.Result; //result returns the number of unshipped orders posted

                    Console.WriteLine("Done posting unshipped orders to ShipStation.");
                    Console.WriteLine("Total items posted to ShipStation: " + result.ToString());

                    // get track number and cost
                    Console.WriteLine("Starting to get tracking number and cost from ShipStation...");
                    result_task = shippingStationOrder.PostTrackingNumberAndCost();
                    result = result_task.Result;

                    Console.WriteLine("Done retreving shipment data from ShipStation.");
                    Console.WriteLine("Total items acquired from ShipStation: " + result.ToString());
                }

                // send confirm shipment to all orders
                ordersManager.ConfirmOrdersShipment();

                // let's sleep for 1 minutes
                Console.WriteLine("The program goes to sleep for 1 minute...");
                Thread.Sleep(60000);

                // let's set the orderCreated to today's date
                orderCreatedAfter = DateTime.UtcNow.Date;
            }

            Console.WriteLine("MWS Orders manager server end.");
            Console.ReadKey(true);

        }

        private static void ExecuteOneTimeRunAccurateWeight(DateTime lastDate)
        {
            Console.WriteLine("Starting One Time Execute Accurate Weight...");

            var shippingStationOrder = new ShipStationOrders();

            var result_task = shippingStationOrder.GetAccurateWeight_PreviousDates(lastDate);
            result_task.Wait();
            var result = result_task.Result;

            Console.WriteLine("Total accurate weight items updated from ShipStation: " + result.ToString());

            Console.WriteLine("One Time Execute Accurate Weight end.");
            Console.ReadKey(true);
        }

        private static OrdersManager initMarketplaceManager () {
            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            //Adds all the parts found in the same assembly as the Program class
            catalog.Catalogs.Add(new AssemblyCatalog(typeof(Program).Assembly));

            //Create the CompositionContainer with the parts in the catalog
            var container = new CompositionContainer(catalog);

            return new OrdersManager(container);
        }
    }
}
