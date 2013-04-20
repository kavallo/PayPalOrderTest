#region Copyright, Author Details and Related Context
//<notice lastUpdateOn="4/19/2013">
//  <assembly>SquidEyes.PayPal</assembly>
//  <description>A simple PayPal order-placement library</description>
//  <copyright>
//    Copyright (C) 2013 Louis S. Berman

//    This program is free software: you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation, either version 3 of the License, or
//    (at your option) any later version.

//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.

//    You should have received a copy of the GNU General Public License
//    along with this program.  If not, see http://www.gnu.org/licenses/.
//  </copyright>
//  <author>
//    <fullName>Louis S. Berman</fullName>
//    <email>louis@squideyes.com</email>
//    <website>http://squideyes.com</website>
//  </author>
//</notice>
#endregion 

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Net;

namespace SquidEyes.PayPal
{
    internal class Payment : AbstractResource
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string id { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string create_time { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string update_time { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string state { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string intent { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Payer payer { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Transaction> transactions { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public RedirectUrls redirect_urls { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Link> links { get; set; }

        public Payment Create(ILogger logger, Config config,
            Context context)
        {
            string resourcePath = "v1/payments/payment";

            string response = null;

            Dictionary<string, string> headers;
            Uri uri = null;
            Uri baseUri = config.Endpoint;

            var success = Uri.TryCreate(baseUri, resourcePath, out uri);

            headers = context.GetHeaders();

            var request = (HttpWebRequest)WebRequest.Create(uri);

            var json = ToJson();

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = json.Length;

            foreach (KeyValuePair<string, string> header in headers)
            {
                if (header.Key.Trim().Equals("User-Agent"))
                    request.UserAgent = header.Value;
                else
                    request.Headers.Add(header.Key, header.Value);
            }

            foreach (string header in request.Headers)
                logger.Log(LogLevel.Debug, header + ":" + request.Headers[header]);

            response = HttpHelper.GetJson(logger, json, request);

            return JsonConvert.DeserializeObject<Payment>(response);
        }
    }
}
