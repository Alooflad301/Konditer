using Konditerka.AppData;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Win32;
using QRCoder;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    /// <summary>
    /// Логика взаимодействия для BasketPage.xaml
    /// </summary>
    public partial class BasketPage : Page
    {
        private List<BasketItemViewModel> _items = new List<BasketItemViewModel>();

        public BasketPage()
        {
            InitializeComponent();
            LoadBasket();
            WindowSizeHelper.SetMinSize(450, 800);
        }

        private void LoadBasket()
        {
            try
            {
                _items = BasketManager.GetCurrentBasketItems();
                BasketGrid.ItemsSource = _items;
                TotalTextBlock.Text = $"Итого: {_items.Sum(x => x.PositionTotal):N2} ₽";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки корзины: " + ex.Message);
            }
        }

        private void RemoveItemButton_Click(object sender, RoutedEventArgs e)
        {
            BasketItemViewModel item = (sender as Button)?.DataContext as BasketItemViewModel;
            if (item == null)
            {
                return;
            }

            BasketManager.RemoveItem(item.IdBasketCatalog);
            LoadBasket();
        }

        private void ClearBasketButton_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Корзина уже пуста.");
                return;
            }

            MessageBoxResult result = MessageBox.Show("Очистить корзину?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            BasketManager.ClearCurrentBasket();
            LoadBasket();
        }

        private void CheckoutButton_Click(object sender, RoutedEventArgs e)
        {
            if (_items.Count == 0)
            {
                MessageBox.Show("Корзина пуста.");
                return;
            }
            decimal total = _items.Sum(x => x.PositionTotal);
            AppFrame.framemain?.Navigate(new CheckoutPage(_items, total));
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.Navigate(new PageOutput());
        }
    }
}
