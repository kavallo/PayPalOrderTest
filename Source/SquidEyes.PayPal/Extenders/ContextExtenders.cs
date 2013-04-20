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

using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics.Contracts;

namespace SquidEyes.PayPal
{
    internal static class ContextExtenders
    {
        public static Dictionary<string, string> GetHeaders(this Context context)
        {
            Contract.Requires(context != null);

            var headers = new Dictionary<string, string>();

            headers.Add("Authorization", context.AccessToken);
            headers.Add("User-Agent", GetUserAgent());
            headers.Add("PayPal-Request-Id", context.RequestId.ToString());

            return headers;
        }

        private static string GetUserAgent()
        {
            const string SDK_ID = "rest-sdk-dotnet";
            const string SDK_VERSION = "0.5.0";

            string header = null;

            var sb = new StringBuilder("SquidEyes.PayPal/" +
                SDK_ID + " v" + SDK_VERSION + " ");

            var dotNETVersion = GetDotNetVersionHeader();

            sb.Append(";").Append(dotNETVersion);

            string osVersion = GetOSHeader();

            if (osVersion.Length > 0)
                sb.Append(";").Append(osVersion);

            header = sb.ToString();

            return header;
        }

        private static string GetOSHeader()
        {
            string header = string.Empty;

            if (OSInfo.OSBits.Equals(OSInfo.SoftwareArchitecture.Bit64))
                header += "bit=" + 64 + ";";
            else if (OSInfo.OSBits.Equals(OSInfo.SoftwareArchitecture.Bit32))
                header += "bit=" + 32 + ";";
            else
                header += "bit=" + "Unknown" + ";";

            header += "os=" + OSInfo.Name + " " + OSInfo.Version + ";";

            return header;
        }

        private static string GetDotNetVersionHeader()
        {
            return "lang=" + "DOTNET;" + "v=" + Environment.Version.ToString().Trim();
        }
    }
}
