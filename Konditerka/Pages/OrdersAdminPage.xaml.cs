using Konditerka.AppData;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    public partial class OrdersAdminPage : Page
    {
        private Orders _selectedOrder;

        public OrdersAdminPage()
        {
            InitializeComponent();
            StatusBox.ItemsSource = AppConnect.model0db.StatusOrders.ToList();
            RefreshData();
        }

        private void RefreshData()
        {
            OrdersGrid.ItemsSource = AppConnect.model0db.Orders.ToList();
            OrdersGrid.SelectedItem = null;
            _selectedOrder = null;
            UserIdBox.Text = string.Empty;
            OrderDateBox.SelectedDate = null;
            OrderPriceBox.Text = string.Empty;
            StatusBox.SelectedIndex = -1;
        }

        private bool ValidateForm()
        {
            if (!int.TryParse(UserIdBox.Text, out var userId) || userId <= 0)
            {
                MessageBox.Show("Введите корректный ID клиента");
                return false;
            }
            if (OrderDateBox.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату заказа");
                return false;
            }
            if (!decimal.TryParse(OrderPriceBox.Text, out var total) || total <= 0)
            {
                MessageBox.Show("Сумма должна быть больше 0");
                return false;
            }
            if (StatusBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите статус заказа");
                return false;
            }
            return true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;
            AppConnect.model0db.Orders.Add(new Orders
            {
                IdUser = int.Parse(UserIdBox.Text),
                Data = OrderDateBox.SelectedDate.Value,
                Price = decimal.Parse(OrderPriceBox.Text),
                IdStatusOrder = (int)StatusBox.SelectedValue
            });
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для редактирования");
                return;
            }
            if (!ValidateForm()) return;

            _selectedOrder.IdUser = int.Parse(UserIdBox.Text);
            _selectedOrder.Data = OrderDateBox.SelectedDate.Value;
            _selectedOrder.Price = decimal.Parse(OrderPriceBox.Text);
            _selectedOrder.IdStatusOrder = (int)StatusBox.SelectedValue;
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedOrder == null)
            {
                MessageBox.Show("Выберите заказ для удаления");
                return;
            }
            var result = MessageBox.Show("Удалить выбранный заказ?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            AppConnect.model0db.Orders.Remove(_selectedOrder);
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void OrdersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedOrder = OrdersGrid.SelectedItem as Orders;
            if (_selectedOrder == null) return;
            UserIdBox.Text = _selectedOrder.IdUser.ToString();
            OrderDateBox.SelectedDate = _selectedOrder.Data;
            OrderPriceBox.Text = _selectedOrder.Price.ToString();
            StatusBox.SelectedValue = _selectedOrder.IdStatusOrder;
        }
    }
}
