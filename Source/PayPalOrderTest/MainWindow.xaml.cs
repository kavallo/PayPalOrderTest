#region Copyright, Author Details and Related Context
//<notice lastUpdateOn="4/19/2013">
//  <assembly>PayPalOrderTest</assembly>
//  <description>An interactive order-placement demo for the SquidEyes.PayPal library</description>
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
using SquidEyes.PayPal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace PayPalOrderTest
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool openLogWhenDown;
        private string selectedOrder;
        private bool canEdit;

        public event PropertyChangedEventHandler PropertyChanged;

        public MainWindow()
        {
            InitializeComponent();

            DataContext = this;

            Orders = GetOrders();

            SelectedOrder = Orders.Keys.ToList()[0];

            CanEdit = true;

            LoadMyCredentials();
        }

        public Dictionary<string, Order> Orders { get; private set; }
        public Order CurrentOrder { get; private set; }

        public bool CanEdit
        {
            get
            {
                return canEdit;
            }
            set
            {
                canEdit = value;

                NotifyPropertyChanged("CanEdit");
                NotifyPropertyChanged("PlaceOrderCommand");
                NotifyPropertyChanged("CloseCommand");
            }
        }

        public string SelectedOrder
        {
            get
            {
                return selectedOrder;
            }
            set
            {
                selectedOrder = value;

                CurrentOrder = Orders[value];

                NotifyPropertyChanged("SelectedOrder");
            }
        }

        public bool OpenLogWhenDone
        {
            get
            {
                return openLogWhenDown;
            }
            set
            {
                openLogWhenDown = value;

                NotifyPropertyChanged("OpenLogWhenDone");
            }
        }

        private Dictionary<string, Order> GetOrders()
        {
            var orders = new Dictionary<string, Order>();

            var doc = XDocument.Load("Orders.xml");

            foreach (var order in doc.Element("orders").Elements("order"))
            {
                var name = order.Attribute("name").Value;

                orders.Add(name, Order.Parse(order, false));
            }

            return orders;
        }

        public DelegateCommand PlaceOrderCommand
        {
            get
            {
                return new DelegateCommand(state => PlaceOrder(), state => CanEdit);
            }
        }

        public DelegateCommand CloseCommand
        {
            get
            {
                return new DelegateCommand(state => Close(), state => CanEdit);
            }
        }

        private void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void PlaceOrder()
        {
            try
            {
                CanEdit = false;

                var logger = new FileLogger(LogLevel.Debug, "PayPal.log");

                if (logger.Exists())
                    logger.AddBlankLine();

                var config = new Config()
                {
                    Endpoint = Properties.Settings.Default.PayPalEndpoint,
                    ClientId = Properties.Settings.Default.PayPalClientId,
                    Secret = Properties.Settings.Default.PayPalSecret
                };

                var task = Task<Outcome>.Factory.StartNew(
                    () => PayPalHelper.PlaceOrder(logger, config, CurrentOrder, false));

                await task;

                if (OpenLogWhenDone)
                    Process.Start("PayPal.log");

                MessageBox.Show(string.Format("{0} (PaymentId: {1})",
                    task.Result.State, task.Result.PaymentId));
            }
            catch (PayPalException error)
            {
                var details = new StringBuilder();

                foreach (var detail in error.Details)
                {
                    if (details.Length != 0)
                        details.Append(", ");

                    details.Append(detail.Key);
                    details.Append('=');
                    details.Append(detail.Value);
                }

                MessageBox.Show(string.Format("{0} (DebugId: {1}, Details: {2})",
                    error.Name, error.DebugId, details));
            }
            catch (Exception error)
            {
                MessageBox.Show("Error: " + error.Message);
            }
            finally
            {
                CanEdit = true;
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            e.Cancel = !CanEdit;
        }

        // Hack to make sure I don't accidentally share my PayPal credentials with the world!!
        private void LoadMyCredentials()
        {
            if (File.Exists("Credentials.xml"))
            {
                var doc = XDocument.Load("Credentials.xml");

                var root = doc.Element("credentials");

                Properties.Settings.Default.PayPalClientId = root.Element("clientId").Value;
                Properties.Settings.Default.PayPalSecret = root.Element("secret").Value;
            }
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
