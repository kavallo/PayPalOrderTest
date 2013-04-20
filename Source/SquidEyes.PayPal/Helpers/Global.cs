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

using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace SquidEyes.PayPal
{
    internal static class Global
    {
        static Global()
        {
            var doc = XDocument.Parse(Properties.Resources.ISO_3166_1_List_en);

            var q = from x in doc.Element("ISO_3166-1_List_en").Elements("ISO_3166-1_Entry")
                    select x.Element("ISO_3166-1_Alpha-2_Code_element").Value;

            CountryCodes = q.ToList();
        }

        public static List<string> CountryCodes { get; private set; }
    }
}
