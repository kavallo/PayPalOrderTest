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
using System.Diagnostics.Contracts;

namespace SquidEyes.PayPal
{
    public class Detail
    {
        public Detail()
        {
            Quantity = 1;
        }

        public string SKU { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Shipping { get; set; }
        public double Tax { get; set; }

        public double SubTotal
        {
            get
            {
                return Math.Round(Price * Quantity, 2);
            }
        }

        public double Total
        {
            get
            {
                return Math.Round(
                    SubTotal + Shipping + Tax, 2);
            }
        }

        public void Validate()
        {
            Contract.Assert(SKU.IsTrimmed());
            Contract.Assert(Name.IsTrimmed());
            Contract.Assert(Quantity >= 1);
            Contract.Assert(Price > 0.0);
            Contract.Assert(Shipping >= 0.0);
            Contract.Assert(Tax >= 0.0);
        }
    }
}
