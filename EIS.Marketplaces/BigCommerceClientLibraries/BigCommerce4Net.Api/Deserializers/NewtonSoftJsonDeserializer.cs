﻿#region License
//   Copyright 2013 Ken Worst - R.C. Worst & Company Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License. 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp.Deserializers;
using RestSharp;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace BigCommerce4Net.Api.Deserializers
{
    public class NewtonSoftJsonDeserializer : IDeserializer
    {
        public string DateFormat { get; set; }

        public string Namespace { get; set; }

        public string RootElement { get; set; }

        public T Deserialize<T>(IRestResponse response) {
            if (string.IsNullOrEmpty(response.Content)) {
                return default(T);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK ||
                response.StatusCode == System.Net.HttpStatusCode.Created ||
                response.StatusCode == System.Net.HttpStatusCode.Accepted ||
                response.StatusCode == System.Net.HttpStatusCode.NoContent) {
                try {
                    return JsonConvert.DeserializeObject<T>(response.Content);
                } catch (JsonSerializationException ex) {
                    log.ErrorFormat("NewtonSoftJsonDeserializer | {0}", response.ResponseUri, ex);
                    log.ErrorFormat("NewtonSoftJsonDeserializer InnerException | {0}",
                        response.ResponseUri, (ex.InnerException != null ? ex.InnerException.Message : ""));
                    log.ErrorFormat("Response.Content | {0}", response.Content, ex);
                    throw new NewtonSoftJsonDeserializerException(ex.Message) { RestResponse = response };
                }
            }
            return default(T);
        }
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger
            (System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    }
}
