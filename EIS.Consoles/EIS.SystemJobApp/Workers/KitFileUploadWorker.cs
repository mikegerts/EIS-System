using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Linq;
using EIS.Inventory.Shared.ViewModels;
using EIS.SystemJobApp.Helpers;
using EIS.SystemJobApp.Models;
using MySql.Data.MySqlClient;
using EIS.Inventory.Shared.Models;

namespace EIS.SystemJobApp.Workers
{
    public class KitFileUploadWorker : JobWorker
    {
        private readonly string _connectionString;

        public KitFileUploadWorker(SystemJob job)
            : base(job)
        {
            _connectionString = ConfigurationManager.ConnectionStrings["InventoryConnection"].ConnectionString;    
        }

        protected override void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_isWorkerExecuted)
                return;

            // set the flag the this bw_DoWork has already called
            _isWorkerExecuted = true;

            // get the Kit details
            var kitDetails = new List<KitDetailDto>();
            var message = CsvFileDataParser.ParsedKitFile(_systemJob.Parameters, kitDetails, _systemJob.HasHeader);

            // log the total records to be processed
            var totalRecords = kitDetails.Count;
            setTotalItemsProcessed(totalRecords);
            _logger.LogInfo(LogEntryType.KitFileUploadWorker, string.Format("Uploading {0} kit details initiated by {1}", totalRecords, _systemJob.SubmittedBy));

            var affectedRows = 0;
            using (var connection = new MySqlConnection(_connectionString))
            {
                // get the unique parent's SKU
                var parentSKUs = kitDetails.Select(x => x.ParentKitSKU).Distinct();

                var command = new MySqlCommand("sp_updateKitComponents");
                command.CommandType = CommandType.StoredProcedure;

                // iterate to to the unique parent SKUs and update its kit details
                foreach (var parentSku in parentSKUs)
                {
                    if (string.IsNullOrEmpty(parentSku))
                        continue;

                    // open the connection if is not open yet
                    if (connection.State != ConnectionState.Open)
                        connection.Open();

                    // clear first the paramerters
                    command.Parameters.Clear();
                    command.Connection = connection;

                    var components = kitDetails.Where(x => x.ParentKitSKU == parentSku);
                    command.Parameters.Add(new MySqlParameter("@parent_kit_sku", parentSku));
                    command.Parameters.Add(new MySqlParameter("@child_kit_skus", string.Join("|", components.Select(x => x.ChildKitSKU))));

                    command.Parameters.Add(new MySqlParameter("@affected_rows", MySqlDbType.Int32));
                    command.Parameters["@affected_rows"].Direction = ParameterDirection.Output;

                    // execute the stored procedure
                    command.ExecuteNonQuery();

                    // get the affected rows
                    var retValue = (int)command.Parameters["@affected_rows"].Value;
                    affectedRows += retValue;

                    // log the progress
                    _bw.ReportProgress(affectedRows);
                    var percentage = (((double)affectedRows) / totalRecords) * 100.00;
                    Console.WriteLine(string.Format("{1:#0.00}% Updating Kit components for ParentKitSKU: {0}", parentSku, percentage));
                }
            }
        }

        protected override void DoPostWorkCompleted()
        {
            // for now, do nothing...
        }
    }
}
