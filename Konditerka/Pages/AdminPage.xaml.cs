using Konditerka.AppData;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    /// <summary>
    /// Логика взаимодействия для AdminPage.xaml
    /// </summary>
    public partial class AdminPage : Page
    {
        public AdminPage()
        {
            InitializeComponent();
            WindowSizeHelper.SetMinSize(450, 550);
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new ProductsAdminPage(null));
        }

        private void CategoriesButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new DictionaryAdminPage("Категории",
    DictionaryAdminPage.DictionaryType.Categories));
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new OrdersAdminPage());
        }

        private void UsersButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new UsersAdminPage());
        }

        private void GoToCatalogButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.Navigate(new PageOutput());
        }

        private void CitiesButton_Click(object sender, RoutedEventArgs e)
        {
            AdminContentFrame.Navigate(new DictionaryAdminPage("Города",
    DictionaryAdminPage.DictionaryType.Cities));
        }
        private void UnitsButton_Click(object sender, RoutedEventArgs e)
    => AdminContentFrame.Navigate(new DictionaryAdminPage("Единицы измерения",
           DictionaryAdminPage.DictionaryType.Units));

        private void PaymentButton_Click(object sender, RoutedEventArgs e)
            => AdminContentFrame.Navigate(new DictionaryAdminPage("Способы оплаты",
                   DictionaryAdminPage.DictionaryType.PaymentMethods));

        private void DeliveryButton_Click(object sender, RoutedEventArgs e)
            => AdminContentFrame.Navigate(new DictionaryAdminPage("Способы доставки",
                   DictionaryAdminPage.DictionaryType.DeliveryMethods));
    }
}
