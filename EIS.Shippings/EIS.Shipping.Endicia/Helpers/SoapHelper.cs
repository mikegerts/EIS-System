using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using EIS.Shipping.Endicia.Service;

namespace EIS.Shipping.Endicia.Helpers
{

    public static class SoapHelper
    {
        private static readonly bool _isWriteToFile;
        private static readonly string _exportedDirectory;
        private const string servicePathTest = "https://elstestserver.endicia.com/LabelService/EwsLabelService.asmx";
        private const string servicePathAcceptance = "https://labelserver.endicia.com/LabelService/EwsLabelService.asmx";
        private const string servicePathLive = "https://labelserver.endicia.com/LabelService/EwsLabelService.asmx";

        static SoapHelper()
        {
            _isWriteToFile = Boolean.Parse(ConfigurationManager.AppSettings["IsWriteToFile"] ?? "false");
            _exportedDirectory = ConfigurationManager.AppSettings["ExportedFilesRoot"];
        }

        public static ResponseT ProcessRequest<ResponseT>(BaseRequest requestObject) where ResponseT : BaseResponse
        {
            string xmlBody;
            var objectType = requestObject.GetType();

            var xmlSerializer = new XmlSerializer(objectType);
            using (var ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, requestObject);
                ms.Position = 0;

                var sr = new StreamReader(ms, Encoding.UTF8);
                var xmlResult = sr.ReadToEnd();

                xmlBody = getInnerXmlBody(xmlResult);
            }

            try
            {
                var webRequest = createHttpWebRequest(objectType.Name, xmlBody);
                using (var response = webRequest.GetResponse())
                {
                    using (var gzipStream = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress))
                    {
                        using (var reader = new StreamReader(gzipStream, Encoding.UTF8))
                        {
                            var soapResult = reader.ReadToEnd();

                            return removeFromEnvelope<ResponseT>(soapResult);
                        }
                    }
                }
            }
            catch (Exception ex) { throw ex; }
        }

        private static HttpWebRequest createHttpWebRequest(string typeName, string methodBody)
        {
            // get the action name for this type
            var actionName = getActionName(typeName);
            var request = (HttpWebRequest)HttpWebRequest.Create(servicePath);

            // add the SOAPAction and Accep-Encoding to the request
            request.Headers.Add("SOAPAction", string.Format("www.envmgr.com/LabelService/{0}", actionName));
            request.Headers.Add("Accept-Encoding", "gzip, deflate");
            request.ContentType = "text/xml;charset=utf-8";
            request.Accept = "text/html, image/gif, image/jpeg, *; q=.2, */*; q=.2";
            request.Method = "POST";

            var envelope = @"<soap:Envelope xmlns:soap=""http://schemas.xmlsoap.org/soap/envelope/""
                                    xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                                    xmlns:xsd=""http://www.w3.org/2001/XMLSchema""
                                    soap:encodingStyle=""http://schemas.xmlsoap.org/soap/encoding/"">
							    <soap:Body>                                                             
								    <{0} xmlns=""www.envmgr.com/LabelService"">
                                        <{1}>
										    {2}
                                        </{1}>
								    </{0}>
								</soap:Body>
							</soap:Envelope>";

            var envelopeBody = string.Format(envelope, actionName, typeName, methodBody);

            // write the XML body of request as for debugging purposes
            if (_isWriteToFile)
                File.WriteAllText(string.Format(@"{0}\Endicia-{1}_{2:yyyyMMdd_HHmmss}.xml", _exportedDirectory, typeName, DateTime.Now), envelopeBody);

            var xml = new XmlDocument();
            xml.LoadXml(envelopeBody);

            using (var rs = request.GetRequestStream())
                xml.Save(rs);

            return request;
        }

        private static ResponseT removeFromEnvelope<ResponseT>(string response) where ResponseT : BaseResponse
        {
            var typeName = typeof(ResponseT).Name;
            var xmlBody = getInnerXmlBody(response, true);

            // save the XML response to a file for debugging purposes
            if (_isWriteToFile)
                File.WriteAllText(string.Format(@"{0}\Endicia-{1}_{2:yyyyMMdd_HHmmss}.xml", _exportedDirectory, typeName, DateTime.Now), xmlBody);

            try
            {
                var xmlSerializer = new XmlSerializer(typeof(ResponseT));
                using (var ms = new MemoryStream())
                {
                    var bytes = UTF8Encoding.UTF8.GetBytes(xmlBody);
                    ms.Write(bytes, 0, bytes.Length);
                    ms.Position = 0;

                    var output = xmlSerializer.Deserialize(ms);
                    return output as ResponseT;
                }
            }
            catch (Exception ex)
            {
                return default(ResponseT);
            }
        }

        private static string servicePath
        {
            get
            {
                var mode = ConfigurationManager.AppSettings["Endicia.Mode"];
                switch (mode.ToUpperInvariant())
                {
                    case "LIVE":
                        return servicePathLive;
                    case "LIVETEST":
                        return servicePathLive;
                    case "ACCT":
                        return servicePathAcceptance;
                    case "TEST":
                        return servicePathTest;
                    default:
                        throw new Exception("Invalid EndiciaMode set in config");
                }
            }
        }

        private static string getInnerXmlBody(string xml, bool isResponseResult = false)
        {
            // load to XML string to the doc
            var document = new XmlDocument();
            document.LoadXml(xml);

            if (isResponseResult)
                return document.ChildNodes[1].ChildNodes[0].ChildNodes[0].InnerXml;
            else
                return document.DocumentElement.InnerXml;
        }

        private static string getActionName(string typeName)
        {
            string actionName;
            switch (typeName)
            {
                case "ChangePassPhraseRequest":
                    actionName = "ChangePassPhrase";
                    break;
                case "RecreditRequest":
                    actionName = "Recredit";
                    break;
                case "PostageRateRequest":
                    actionName = "CalculatePostageRate";
                    break;
                case "PostageRatesRequest":
                    actionName = "CalculatePostageRates";
                    break;
                case "LabelRequest":
                    actionName = "GetPostageLabel";
                    break;
                default:
                    throw new Exception("Unknown type name for Endicia: " + typeName);
            }

            return actionName;
        }
    }
}
