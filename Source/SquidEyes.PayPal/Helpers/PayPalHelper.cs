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

namespace SquidEyes.PayPal
{
    public static class PayPalHelper
    {
        public static Outcome PlaceOrder(ILogger logger, Config config, Order order, bool validate = true)
        {
            const string OAUTHERROR =
                "An unexpected OAUTH validation error occured!  Please contact support for help in resolving this issue.";

            Contract.Requires(logger != null);
            Contract.Requires(config != null);
            Contract.Requires(order != null);

            if (validate)
            {
                config.Validate();
                order.Validate();
            }

            logger.Log(LogLevel.Info, "Placing Order (OrderId: {0})", order.OrderId);

            var oauth = new OAuthHelper(logger, config);

            var accessToken = oauth.GetAccessToken();

            if (accessToken == null)
            {
                logger.Log(LogLevel.Error, OAUTHERROR);

                throw new Exception(OAUTHERROR);
            }

            var context = new Context(accessToken, order.OrderId);

            var payment = GetPayment(logger, config, context, order, validate);

            var outcome = new Outcome()
            {
                CreatedOn = DateTime.Parse(payment.create_time),
                UpdatedOn = DateTime.Parse(payment.update_time),
                State = (OrderState)Enum.Parse(typeof(OrderState), payment.state, true),
                PaymentId = payment.id
            };

            logger.Log(LogLevel.Info, "OrderId: {0}, Outcome: {1}, PaymentId: {2}",
                order.OrderId, outcome.State, outcome.PaymentId);

            return outcome;
        }

        private static Payment GetPayment(ILogger logger, Config config,
            Context context, Order order, bool validate)
        {
            var billingAddress = new Address()
            {
                city = order.City,
                country_code = order.Country,
                line1 = order.Address1,
                postal_code = order.PostalCode,
                state = order.State,
            };

            var creditCard = new CreditCard()
            {
                billing_address = billingAddress,
                cvv2 = order.CVV2.ToString(),
                expire_month = order.ExpireMonth.ToString("00"),
                expire_year = order.ExpireYear.ToString(),
                first_name = order.FirstName,
                last_name = order.LastName,
                number = order.CardNumber,
                type = validate ? order.CardNumber.GetCardType().ToString().ToLower() : "visa"
            };

            var transactions = new List<Transaction>();

            var appInfo = new AppInfo();

            foreach (var thing in order.Details)
            {
                var transaction = new Transaction()
                {
                    amount = new Amount()
                    {
                        currency = "USD",
                        total = thing.Total.ToString("N2"),
                        details = new AmountDetails()
                        {
                            shipping = thing.Shipping.ToString("N2"),
                            subtotal = thing.SubTotal.ToString("N2"),
                            tax = thing.Tax.ToString("N2")
                        }
                    },
                    description = string.Format("Order: {0}, Product: {1}",
                        order.OrderId, appInfo.GetTitle())
                };

                transactions.Add(transaction);
            }

            var payment = new Payment()
            {
                intent = "sale",
                payer = new Payer()
                {
                    funding_instruments = new List<FundingInstrument>()
                    {
                        new FundingInstrument()
                        { 
                            credit_card = creditCard 
                        } 
                    },
                    payment_method = "credit_card"
                },
                transactions = transactions
            };

            return JsonConvert.DeserializeObject<Payment>(
                payment.Create(logger, config, context).ToJson());
        }
    }
}
