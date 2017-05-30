using CsvHelper;
using EIS.Inventory.Core.Managers;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using EIS.Inventory.Models;
using System.Data.OleDb;
using System.Globalization;
using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Web.UI.WebControls;
using System.Security.Cryptography;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class ReportController : Controller
    {
        private readonly IProductService _productService;
        private readonly IVendorService _vendorService;


        public ReportController(IProductService productService, IVendorService vendorService)
        {
            _productService = productService;
            _vendorService = vendorService;
        }

        // GET: Report
        public ActionResult ProfitAndLossSummary()
        {
            return View();
        }

        public ActionResult DownloadProducts()
        {
            var vendors = _vendorService.GetAllVendors();

            return View(vendors);
        }

        [HttpPost]
        public FileStreamResult _DownloadProducts(List<int> vendorIds, DateTime requestDate)
        {
            var products = new List<ProductDto>(); //_productService.GetProductsByVendorId(vendorIds);
            var result = writeCsvToMemory(products);
            var memoryStream = new MemoryStream(result);

            return new FileStreamResult(memoryStream, "text/csv") { FileDownloadName = string.Format("EIS_Products-{0:yyyy}{0:MM}{0:dd}.csv", requestDate) };
        }

        private byte[] writeCsvToMemory(IEnumerable<ProductDto> products)
        {
            using (var memoryStream = new MemoryStream())
            using (var streamWriter = new StreamWriter(memoryStream))
            using (var csvWriter = new CsvWriter(streamWriter))
            {
                // let's write first its columns' header
                csvWriter.WriteField("EisSKU");
                //csvWriter.WriteField("SupplierSKU");
                //csvWriter.WriteField("VendorId");
                csvWriter.WriteField("Name");
                csvWriter.WriteField("Description");
                csvWriter.WriteField("ShortDescription");
                csvWriter.WriteField("Category");
                csvWriter.WriteField("UPC");
                //csvWriter.WriteField("Quantity");
                //csvWriter.WriteField("SupplierPrice");
                csvWriter.WriteField("SellerPrice");
                csvWriter.WriteField("PkgLength");
                csvWriter.WriteField("PkgWidth");
                csvWriter.WriteField("PkgHeight");
                csvWriter.WriteField("PkgLenghtUnit");
                csvWriter.WriteField("ItemLength");
                csvWriter.WriteField("ItemWidth");
                csvWriter.WriteField("ItemHeight");
                csvWriter.WriteField("ItemLenghtUnit");
                csvWriter.WriteField("EAN");
                csvWriter.WriteField("Brand");
                csvWriter.WriteField("Color");
                csvWriter.WriteField("Model");

                csvWriter.NextRecord();

                // then the product records
                foreach (var item in products)
                {
                    csvWriter.WriteField(item.EisSKU);
                    //csvWriter.WriteField(item.SupplierSKU);
                    //csvWriter.WriteField(item.VendorId);
                    csvWriter.WriteField(item.Name);
                    csvWriter.WriteField(item.Description ?? "");
                    csvWriter.WriteField(item.ShortDescription ?? "");
                    csvWriter.WriteField(item.Category ?? "");
                    csvWriter.WriteField(item.UPC ?? "");
                    //csvWriter.WriteField(item.Quantity ?? 0);
                    //csvWriter.WriteField(getValue(item.SupplierPrice));
                    csvWriter.WriteField(getValue(item.SellerPrice));
                    csvWriter.WriteField(getValue(item.PkgLength));
                    csvWriter.WriteField(getValue(item.PkgWidth));
                    csvWriter.WriteField(getValue(item.PkgHeight));
                    csvWriter.WriteField(item.PkgLenghtUnit ?? "");
                    csvWriter.WriteField(getValue(item.ItemLength));
                    csvWriter.WriteField(getValue(item.ItemWidth));
                    csvWriter.WriteField(getValue(item.ItemHeight));
                    csvWriter.WriteField(item.ItemLenghtUnit ?? "");
                    csvWriter.WriteField(item.EAN ?? "");
                    csvWriter.WriteField(item.Brand ?? "");
                    csvWriter.WriteField(item.Color ?? "");
                    csvWriter.WriteField(item.Model_ ?? "");

                    csvWriter.NextRecord();
                }

                streamWriter.Flush();
                return memoryStream.ToArray();
            }
        }

        private object getValue(decimal? value)
        {
            if (value == null)
                return string.Empty;
            else
                return value;
        }
        public PartialViewResult ShowError(String sErrorMessage)
        {
            return PartialView("ErrorMessageView");
        }

        public ActionResult UPCSearch()
        {
            if (TempData["sErrMsg"] == "")
            {

            }
            else
            {
                //ViewBag.sErrMsg = TempData["sErrMsg"];
                ViewBag.sErrMsg = TempData["sErrMsg"];
            }
            if (TempData["ProductDetailList"] == "" || TempData["ProductDetailList"] == null)
            {
                ViewBag.ProductDetaiList = "";
            }
            else
            {
                ViewBag.ProductDetailList = TempData["ProductDetailList"];
            }
            return View();
        }

        //Call Third Party API For Single Transaction UPC Code Detail
        public async Task<ActionResult> ConsumeExternalAPI(string UPCCode)
        {
            string _appKey = "/zDQJTHnn1dS";
            string _authKey = "Yj83A3i3i5Kc8Wi6";
            string _signature = GetDigitEyesVerificationCode(UPCCode, _authKey);
            string url = "https://www.digit-eyes.com/gtin/v2_0/?upcCode=" + UPCCode + "&field_names=all&language=en&app_key=" + _appKey + "&signature=" + _signature + "";
            var _retrunProductDetail = string.Empty;
            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
            {
                client.BaseAddress = new Uri(url);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                System.Net.Http.HttpResponseMessage response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    _retrunProductDetail = await response.Content.ReadAsStringAsync();
                }
            }
            return Json(_retrunProductDetail, JsonRequestBehavior.AllowGet);
        }

        //get bulk products details through webapi
        public async Task<ActionResult> UploadExternalAPI(HttpPostedFileBase file)
        {
            DataTable _upcCSVCode = new DataTable();
            string path = string.Empty;
            var fileName = string.Empty;
            var UPCCode = string.Empty;
            int _yes = 1;
            bool _isFirstRowHeader = Convert.ToBoolean(_yes);
            string _retrunProductDetail = string.Empty;
            ArrayList _upcCodeListForCSV = new ArrayList();
            List<dynamic> _list = new List<dynamic>();
            StringBuilder sb = new StringBuilder();
            DataTable dt = new DataTable();
            DataSet _productDataset = new DataSet();
            if (file.ContentLength > 0)
            {
                fileName = Path.GetFileName(file.FileName);
                path = Path.Combine(Server.MapPath("~/App_Data/uploads"), fileName);
                file.SaveAs(path);
                _upcCSVCode = GetDataTableFromCsv(path, _isFirstRowHeader);
                if (_upcCSVCode.Rows.Count > 0)
                {
                    for (int i = 0; i < _upcCSVCode.Rows.Count; i++)
                    {
                        UPCCode = _upcCSVCode.Rows[i][0].ToString();
                        if (UPCCode != null)
                        {
                            string _appKey = "/zDQJTHnn1dS";
                            string _authKey = "Yj83A3i3i5Kc8Wi6";
                            string _signature = GetDigitEyesVerificationCode(UPCCode, _authKey);//Third party eye method to get sample signature of particuler upc code 
                            string _url = "https://www.digit-eyes.com/gtin/v2_0/?upcCode=" + UPCCode + "&field_names=all&language=en&app_key=" + _appKey + "&signature=" + _signature + "";
                            string[] _jsonProductArray;
                            using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                            {
                                client.BaseAddress = new Uri(_url);
                                client.DefaultRequestHeaders.Accept.Clear();
                                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                                System.Net.Http.HttpResponseMessage response = await client.GetAsync(_url);
                                if (response.IsSuccessStatusCode)
                                {
                                    _retrunProductDetail = await response.Content.ReadAsStringAsync();
                                    _jsonProductArray = Regex.Split(_retrunProductDetail.Replace(", gcp", ""), "},{");
                                    string[] jsonStringArray = Regex.Split(_retrunProductDetail.Replace("[", "").Replace("]", "").Replace(", Inc.", " Inc."), "},{");
                                    if (dt.Columns.Count <= 0)
                                    {
                                        List<string> ColumnsName = new List<string>();
                                        foreach (string jSA in jsonStringArray)
                                        {
                                            string[] jsonStringData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");
                                            foreach (string ColumnsNameData in jsonStringData)
                                            {
                                                try
                                                {
                                                    int idx = ColumnsNameData.IndexOf(":");
                                                    string ColumnsNameString = ColumnsNameData.Substring(0, idx - 1).Replace("\"", "");
                                                    if (!ColumnsName.Contains(ColumnsNameString))
                                                    {
                                                        ColumnsName.Add(ColumnsNameString);
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new Exception(string.Format("Error Parsing Column Name : {0}", ColumnsNameData));
                                                }
                                            }
                                            break;
                                        }
                                        foreach (string AddColumnName in ColumnsName)
                                        {
                                            var _columnName = Regex.Replace(AddColumnName, @"\s+", "");
                                            if (dt.Columns.Contains(_columnName))
                                            {
                                                dt.Columns.Add("gcp1");
                                            }
                                            else
                                            {
                                                dt.Columns.Add(_columnName);
                                            }
                                        }
                                        dt.Rows.Add(JsonStringToDataTable(jsonStringArray, dt));
                                    }
                                    else
                                    {
                                        dt.Rows.Add(JsonStringToDataTable(jsonStringArray, dt));
                                    }
                                }
                            }
                        }
                    }
                    string _convertProductetailsToCSV = ConvertToCSV(dt);//convert to csv file 
                    _saveProductsDetailCSVFile(_convertProductetailsToCSV.ToString());
                    string _retrunProductsDetail = JsonConvert.SerializeObject(_productDataset, Formatting.Indented);
                    TempData["ProductDetailList"] = _retrunProductsDetail;
                    return RedirectToAction("UPCSearch");
                }
                else
                {
                    TempData["sErrMsg"] = "Please Add UPC Code in .csv file !";
                }
            }
            else
            {
                return RedirectToAction("UPCSearch");
            }

            return RedirectToAction("UPCSearch");

        }
        public void _saveProductsDetailCSVFile(string _saveProductetailsToCSV)
        {
            try
            {
                string path = string.Empty;
                var fileName = string.Empty;
                System.IO.File.WriteAllText(Server.MapPath("~/DownloadProductsDetail/demoDownloadList.csv"), _saveProductetailsToCSV.ToString());
                path = Path.Combine(Server.MapPath("~/DownloadProductsDetail/demoDownloadList.csv"));
                Download_File(path);
            }
            catch (Exception)
            {

                throw;
            }

        }
        private void Download_File(string FilePath)
        {
            Response.ContentType = "text/csv";
            Response.AppendHeader("Content-Disposition", "attachment; filename=" + Path.GetFileName(FilePath));
            Response.WriteFile(FilePath);
            Response.End();
        }
        private string GetDigitEyesVerificationCode(string UpcCode, string AuthKey)
        {
            var hmac = new HMACSHA1(Encoding.UTF8.GetBytes(AuthKey));
            var m = hmac.ComputeHash(Encoding.UTF8.GetBytes(UpcCode));
            return Convert.ToBase64String(m);
        }

        private string ConvertToCSV(DataTable table)
        {
            StringBuilder content = new StringBuilder();
            for (int j = 0; j < table.Rows.Count; j++)
            {

                DataRow dr1 = (DataRow)table.Rows[j];
                int intColumnCount = dr1.Table.Columns.Count;
                int index = 1;

                //add column names
                if (content.Length <= 0)
                {
                    foreach (DataColumn item in dr1.Table.Columns)
                    {
                        content.Append(String.Format("\"{0}\"", item.ColumnName));
                        if (index < intColumnCount)
                            content.Append(",");
                        else
                            content.Append(item + "\r\n");


                        index++;
                    }

                    //add column data
                    foreach (DataRow currentRow in table.Rows)
                    {
                        string strRow = string.Empty;
                        for (int y = 0; y <= intColumnCount - 1; y++)
                        {
                            strRow += "\"" + currentRow[y].ToString() + "\"";

                            if (y < intColumnCount - 1 && y >= 0)
                                strRow += ",";
                        }
                        content.Append(strRow + "\r\n");
                    }

                }
                else
                {

                }

            }
            return content.ToString();

        }

        public DataRow JsonStringToDataTable(string[] jsonStringArray, DataTable dt)
        {
            DataRow nr = dt.NewRow();
            foreach (string jSA in jsonStringArray)
            {
                string[] RowData = Regex.Split(jSA.Replace("{", "").Replace("}", ""), ",");

                foreach (string rowData in RowData)
                {
                    try
                    {

                        int idx = rowData.IndexOf(":");
                        string RowColumns = rowData.Substring(0, idx - 1).Replace("\"", "");
                        var _rowColumn = Regex.Replace(RowColumns, @"\s+", "");
                        string RowDataString = rowData.Substring(idx + 1).Replace("\"", "").Replace("\'\"", "");
                        nr[_rowColumn] = RowDataString;
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }
                }
                // dt.Rows.Add(nr);
            }
            return nr;
        }
        public static DataTable convertStringToDataTable(string data)
        {
            DataTable dataTable = new DataTable();
            bool columnsAdded = false;
            foreach (string row in data.Split('$'))
            {
                DataRow dataRow = dataTable.NewRow();
                foreach (string cell in row.Split('|'))
                {
                    string[] keyValue = cell.Split('~');
                    if (!columnsAdded)
                    {
                        DataColumn dataColumn = new DataColumn(keyValue[0]);
                        dataTable.Columns.Add(dataColumn);
                    }
                    dataRow[keyValue[0]] = keyValue[1];
                }
                columnsAdded = true;
                dataTable.Rows.Add(dataRow);
            }
            return dataTable;
        }

        //convert csv file to Datatable 
        static DataTable GetDataTableFromCsv(string path, bool isFirstRowHeader)
        {
            string header = isFirstRowHeader ? "Yes" : "No";

            string pathOnly = Path.GetDirectoryName(path);
            string fileName = Path.GetFileName(path);

            string sql = @"SELECT * FROM [" + fileName + "]";

            using (OleDbConnection connection = new OleDbConnection(
                      @"Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + pathOnly +
                      ";Extended Properties=\"Text;HDR=" + header + "\""))
            using (OleDbCommand command = new OleDbCommand(sql, connection))
            using (OleDbDataAdapter adapter = new OleDbDataAdapter(command))
            {
                DataTable dataTable = new DataTable();
                dataTable.Locale = CultureInfo.CurrentCulture;
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        //Convert json object to dynamic object  
        public sealed class DynamicJsonConverter : JavaScriptConverter
        {
            public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
            {
                if (dictionary == null)
                    throw new ArgumentNullException("dictionary");

                return type == typeof(object) ? new DynamicJsonObject(dictionary) : null;
            }

            public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
            {
                throw new NotImplementedException();
            }

            public override IEnumerable<Type> SupportedTypes
            {
                get { return new ReadOnlyCollection<Type>(new List<Type>(new[] { typeof(object) })); }
            }

            #region Nested type: DynamicJsonObject

            private sealed class DynamicJsonObject : DynamicObject
            {
                private readonly IDictionary<string, object> _dictionary;

                public DynamicJsonObject(IDictionary<string, object> dictionary)
                {
                    if (dictionary == null)
                        throw new ArgumentNullException("dictionary");
                    _dictionary = dictionary;
                }

                public override string ToString()
                {
                    var sb = new StringBuilder("{");
                    ToString(sb);
                    return sb.ToString();
                }

                private void ToString(StringBuilder sb)
                {
                    var firstInDictionary = true;
                    foreach (var pair in _dictionary)
                    {
                        if (!firstInDictionary)
                            sb.Append(",");
                        firstInDictionary = false;
                        var value = pair.Value;
                        var name = pair.Key;
                        if (value is string)
                        {
                            sb.AppendFormat("{0}:\"{1}\"", name, value);
                        }
                        else if (value is IDictionary<string, object>)
                        {
                            new DynamicJsonObject((IDictionary<string, object>)value).ToString(sb);
                        }
                        else if (value is ArrayList)
                        {
                            sb.Append(name + ":[");
                            var firstInArray = true;
                            foreach (var arrayValue in (ArrayList)value)
                            {
                                if (!firstInArray)
                                    sb.Append(",");
                                firstInArray = false;
                                if (arrayValue is IDictionary<string, object>)
                                    new DynamicJsonObject((IDictionary<string, object>)arrayValue).ToString(sb);
                                else if (arrayValue is string)
                                    sb.AppendFormat("\"{0}\"", arrayValue);
                                else
                                    sb.AppendFormat("{0}", arrayValue);

                            }
                            sb.Append("]");
                        }
                        else
                        {
                            sb.AppendFormat("{0}:{1}", name, value);
                        }
                    }
                    sb.Append("}");
                }

                public override bool TryGetMember(GetMemberBinder binder, out object result)
                {
                    if (!_dictionary.TryGetValue(binder.Name, out result))
                    {
                        // return null to avoid exception.  caller can check for null this way...
                        result = null;
                        return true;
                    }

                    result = WrapResultObject(result);
                    return true;
                }

                public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
                {
                    if (indexes.Length == 1 && indexes[0] != null)
                    {
                        if (!_dictionary.TryGetValue(indexes[0].ToString(), out result))
                        {
                            // return null to avoid exception.  caller can check for null this way...
                            result = null;
                            return true;
                        }

                        result = WrapResultObject(result);
                        return true;
                    }

                    return base.TryGetIndex(binder, indexes, out result);
                }

                private static object WrapResultObject(object result)
                {
                    var dictionary = result as IDictionary<string, object>;
                    if (dictionary != null)
                        return new DynamicJsonObject(dictionary);

                    var arrayList = result as ArrayList;
                    if (arrayList != null && arrayList.Count > 0)
                    {
                        return arrayList[0] is IDictionary<string, object>
                            ? new List<object>(arrayList.Cast<IDictionary<string, object>>().Select(x => new DynamicJsonObject(x)))
                            : new List<object>(arrayList.Cast<object>());
                    }

                    return result;
                }
            }

            #endregion
        }








    }
}