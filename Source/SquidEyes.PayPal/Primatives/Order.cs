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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Xml.Linq;

namespace SquidEyes.PayPal
{
    public class Order
    {
        public Order()
        {
            Details = new List<Detail>();
        }

        public readonly Guid OrderId = Guid.NewGuid();

        public string FirstName { get; set; }
        public char? Initial { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Country { get; set; }
        public string Address1 { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string CardNumber { get; set; }
        public int CVV2 { get; set; }
        public int ExpireYear { get; set; }
        public int ExpireMonth { get; set; }

        public List<Detail> Details { get; private set; }

        public CardType CardType
        {
            get
            {
                return CardNumber.GetCardType();
            }
        }

        public void Validate()
        {
            var expireOn = new DateTime(ExpireYear, ExpireMonth, 1).AddMonths(1).AddDays(-1);

            Contract.Assert(!string.IsNullOrWhiteSpace(FirstName));
            Contract.Assert((!Initial.HasValue) || char.IsUpper(Initial.Value));
            Contract.Assert(!string.IsNullOrWhiteSpace(LastName));
            Contract.Assert(Email.IsEmail());
            Contract.Assert(Phone.IsPhone(PhoneKind.EitherFormat));
            Contract.Assert(Global.CountryCodes.Contains(Country));
            Contract.Assert(Address1.IsTrimmed(100));
            Contract.Assert(string.IsNullOrEmpty(Address2) || Address2.IsTrimmed(100));
            Contract.Assert(City.IsTrimmed(50));
            Contract.Assert(State.IsTrimmed(100));
            Contract.Assert(PostalCode.IsTrimmed(100));
            Contract.Assert(CardNumber.IsCardNumber());
            Contract.Assert((CVV2 >= 100) && (CVV2 <= 9999));
            Contract.Assert(expireOn >= DateTime.Today);

            Details.ForEach(detail => detail.Validate());
        }

        public static Order Parse(XElement element, bool validate = true)
        {
            var order = new Order();

            var initial = element.Element("initial");

            order.FirstName = element.Element("firstName").Value;
            order.Initial = initial == null ? (char?)null : ((string)initial)[0];
            order.LastName = element.Element("lastName").Value;
            order.Email = element.Element("email").Value;
            order.Phone = element.Element("phone").Value;
            order.Country = element.Element("country").Value;
            order.Address1 = element.Element("address1").Value;
            order.Address2 = element.Element("address2").Value;
            order.City = element.Element("city").Value;
            order.State = element.Element("state").Value;
            order.PostalCode = element.Element("postalCode").Value;
            order.CardNumber = element.Element("cardNumber").Value;
            order.CVV2 = (int)element.Element("cvv2");
            order.ExpireMonth = (int)element.Element("expireMonth");
            order.ExpireYear = (int)element.Element("expireYear");

            foreach (var detail in element.Element("details").Elements("detail"))
            {
                var shipping = detail.Element("shipping");
                var tax = detail.Element("tax");

                order.Details.Add(new Detail()
                {
                    SKU = detail.Element("sku").Value,
                    Name = detail.Element("name").Value,
                    Price = (double)detail.Element("price"),
                    Shipping = shipping == null ? 0.0 : (double)shipping,
                    Tax = shipping == null ? 0.0 : (double)tax,
                    Quantity = (int)detail.Element("quantity")
                });
            }

            if (validate)
                order.Validate();

            return order;
        }
    }
}
