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
using System.IO;
using System.Net;

namespace SquidEyes.PayPal
{
    internal static class HttpHelper
    {
        public static string GetJson(ILogger logger, string output, HttpWebRequest request)
        {
            try
            {
                using (var stream = new StreamWriter(request.GetRequestStream()))
                {
                    if (!string.IsNullOrEmpty(output))
                    {
                        stream.Write(output);
                        stream.Flush();
                        stream.Close();

                        logger.Log(LogLevel.Debug, "OUTBOUND: " + output.TrimJson());
                    }
                }
            }
            catch (Exception error)
            {
                logger.Log(LogLevel.Error, "HttpHelper.Execute Error: " + error.Message);

                throw new Exception("HttpHelper.Execute Error: " + error.Message, error);
            }

            try
            {
                using (var response = request.GetResponse())
                {
                    using (var stream = new StreamReader(response.GetResponseStream()))
                    {
                        var json = stream.ReadToEnd().Trim();

                        logger.Log(LogLevel.Debug, "INBOUND: " + json.TrimJson());

                        return json;
                    }
                }
            }
            catch (WebException we)
            {
                Error error = null;

                if (we.Response is HttpWebResponse)
                {
                    var response = ((HttpWebResponse)we.Response);

                    using (var stream = new StreamReader(response.GetResponseStream()))
                        error = JsonConvert.DeserializeObject<Error>(stream.ReadToEnd());

                    logger.Log(LogLevel.Error, "Status: {0}, Description: {1}",
                        response.StatusCode, response.StatusDescription);

                    foreach (string header in response.Headers.AllKeys)
                    {
                        var value = response.Headers[header];

                        logger.Log(LogLevel.Error,
                            string.Format("HEADER: {0}={1}", header, value));
                    }
                }

                logger.Log(LogLevel.Error, error.message);

                foreach (var detail in error.details)
                    logger.Log(LogLevel.Error, "{0}={1}", detail.field, detail.issue);

                throw new PayPalException(error);
            }
        }
    }
}
