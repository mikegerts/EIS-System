using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace EIS.Marketplace.ConsoleApp
{
    public class WebRequestor
    {
        /// <summary>
        /// Send POST request to the specified URL with its parameters
        /// </summary>
        /// <param name="url">The URL where POST web request will be sent</param>
        /// <param name="parameters">The post data of the POST request</param>
        /// <returns>Returns HTML strings results of the POST request</returns>
        public string SendPostRequest(string url, Dictionary<string, string> parameters)
        {
            var pageContent = string.Empty;
            try
            {
                // build the post form data for the request
                var postData = new StringBuilder();
                if (parameters != null)
                {
                    foreach (var key in parameters.Keys)
                    {
                        postData.AppendFormat("{0}={1}&",
                            HttpUtility.UrlEncode(key),
                            HttpUtility.UrlEncode(parameters[key]));
                    }
                }

                var request = (HttpWebRequest)WebRequest.Create(url);
                var data = Encoding.ASCII.GetBytes(postData.ToString());

                request.Method = "POST";
                request.ContentType = "application/x-www-form-urlencoded";
                request.ContentLength = data.Length;

                var requestStream = request.GetRequestStream();
                requestStream.Write(data, 0, data.Length);
                requestStream.Close();

                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();

                // returns empty if there's no connection
                if (responseStream == null)
                    return string.Empty;

                var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                pageContent = streamReader.ReadToEnd();

                // close the streams
                streamReader.Close();
                responseStream.Close();
            }
            catch (Exception ex)
            {
                // ignored
            }

            return pageContent;
        }

        /// <summary>
        /// Send GET request to the specified URL
        /// </summary>
        /// <param name="url">The URL where GET request will be send</param>
        /// <returns>Returns HTML string result of the GET request</returns>
        public string SendGetRequest(string url)
        {
            var pageContent = string.Empty;
            try
            {
                var request = (HttpWebRequest)WebRequest.Create(url);
                request.Method = "GET";
                
                var response = (HttpWebResponse)request.GetResponse();
                var responseStream = response.GetResponseStream();

                // returns empty if there's no connection
                if (responseStream == null)
                    return string.Empty;

                var streamReader = new StreamReader(responseStream, Encoding.UTF8);
                pageContent = streamReader.ReadToEnd();

                // close the streams
                streamReader.Close();
                responseStream.Close();
            }
            catch (WebException ex)
            {
                // return a 404 status message to the caller, it means 'page not found' dude!
                return "404";
            }
            catch (Exception ex)
            {
                // ignored
            }

            return pageContent;
        }
    }
}
