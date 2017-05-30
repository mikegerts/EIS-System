using System;
using System.Configuration;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using EIS.SchedulerTaskApp.Models;

namespace EIS.SchedulerTaskApp.Helpers
{
    public static class XmlParser
    {
        /// <summary>
        /// The the XML string to write into XML file with the specified file name
        /// </summary>
        /// <param name="envelope"></param>
        /// <param name="fileName"></param>
        /// <returns>The full file name information</returns>
        public static string WriteXmlToFile(AmazonEnvelope envelope, string fileName)
        {
            var xmlString = string.Empty;
            var xmlSerializer = new XmlSerializer(envelope.GetType());
            using (var ms = new MemoryStream())
            {
                xmlSerializer.Serialize(ms, envelope);
                ms.Position = 0;

                var sr = new StreamReader(ms, Encoding.UTF8);
                xmlString = sr.ReadToEnd();
            }

            var documentFileName = Path.Combine(ConfigurationManager.AppSettings["MarketplaceFeedRoot"],
                string.Format("{1}_{0:yyyyMMdd_HHmmss}.xml", DateTime.Now, fileName));

            // save it to the directory
            File.WriteAllText(documentFileName, xmlString);

            return documentFileName;
        }
    }
}
