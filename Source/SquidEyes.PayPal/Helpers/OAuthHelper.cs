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
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Net;
using System.Text;

namespace SquidEyes.PayPal
{
    internal class OAuthHelper
    {
        private class OAuthResponse
        {
            public string scope { get; set; }
            public string access_token { get; set; }
            public string token_type { get; set; }
            public string app_id { get; set; }
            public int expires_in { get; set; }
        }

        private const string OAUTHTOKENPATH = "/v1/oauth2/token";

        private ILogger logger;
        private Config config;
        private string accessToken;
        private DateTime expiresAfter;

        public OAuthHelper(ILogger logger, Config config)
        {
            Contract.Requires(logger != null);
            Contract.Requires(config != null);

            this.logger = logger;
            this.config = config;
        }

        public string GetAccessToken()
        {
            if (accessToken != null)
            {
                if (DateTime.UtcNow > expiresAfter)
                    accessToken = null;
            }

            if (accessToken == null)
                accessToken = GetOAuthToken();

            return accessToken;
        }

        private string GetOAuthToken()
        {
            const string POSTREQUEST = "grant_type=client_credentials";

            var authorization = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(config.ClientId + ":" + config.Secret));

            string json = null;
            Dictionary<string, string> headers = null;

            try
            {
                Uri uri = null;

                var success = Uri.TryCreate(config.Endpoint, OAUTHTOKENPATH, out uri);

                var request = (HttpWebRequest)WebRequest.Create(uri);

                headers = new Dictionary<string, string>();

                headers.Add("Authorization", "Basic " + authorization);
                //headers.Add("PayPal-Request-Id", "AAA");  ???????????????????????????????????

                request.Method = "POST";
                request.Accept = "*/*";

                foreach (KeyValuePair<string, string> header in headers)
                    request.Headers.Add(header.Key, header.Value);

                using (var stream = new StreamWriter(request.GetRequestStream()))
                {
                    logger.Log(LogLevel.Debug, "OUTBOUND: " + POSTREQUEST);
                    logger.Log(LogLevel.Debug, "Authorization={0}", headers["Authorization"]);

                    stream.Write(POSTREQUEST);
                }

                using (var response = request.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream()))
                    {
                        json = stream.ReadToEnd();

                        logger.Log(LogLevel.Debug, "INBOUND: " + json.TrimJson());
                    }
                }

                var oauth = JsonConvert.DeserializeObject<OAuthResponse>(json);

                expiresAfter = DateTime.UtcNow.Add(
                    TimeSpan.FromSeconds(oauth.expires_in));

                // We never want a token to expire due to race conditions!!
                expiresAfter = expiresAfter.Add(new TimeSpan(
                    expiresAfter.Ticks - (int)(expiresAfter.Ticks * 0.1)));

                return oauth.token_type + " " + oauth.access_token;
            }
            catch (WebException error)
            {
                if (error.Response is HttpWebResponse)
                {
                    var response = ((HttpWebResponse)error.Response);

                    logger.Log(LogLevel.Warning,
                        "Authorization Failure (Code: {0}, Status: {1})",
                        response.StatusCode, response.StatusDescription);
                }

                accessToken = null;

                return null;
            }
        }
    }
}
