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
using System.Collections.Generic;

namespace SquidEyes.PayPal
{
	internal class Refund : AbstractResource  
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
        public Amount amount { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string sale_id { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string capture_id { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string parent_payment { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string description { get; set; }

		[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<Link> links { get; set; }
	}
}
