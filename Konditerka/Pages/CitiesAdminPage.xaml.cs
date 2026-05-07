using Konditerka.AppData;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    public partial class CitiesAdminPage : Page
    {
        private Cities _selectedCity;

        public CitiesAdminPage()
        {
            InitializeComponent();
            RefreshData();
        }

        private void RefreshData()
        {
            CitiesGrid.ItemsSource = AppConnect.model0db.Cities.ToList();
            CitiesGrid.SelectedItem = null;
            _selectedCity = null;
            CityNameBox.Text = string.Empty;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(CityNameBox.Text))
            {
                MessageBox.Show("Введите название города");
                return false;
            }

            return true;
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm()) return;
            AppConnect.model0db.Cities.Add(new Cities { NameCity = CityNameBox.Text.Trim() });
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCity == null)
            {
                MessageBox.Show("Выберите город для редактирования");
                return;
            }
            if (!ValidateForm()) return;
            _selectedCity.NameCity = CityNameBox.Text.Trim();
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedCity == null)
            {
                MessageBox.Show("Выберите город для удаления");
                return;
            }
            var result = MessageBox.Show("Удалить выбранный город?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            AppConnect.model0db.Cities.Remove(_selectedCity);
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void CitiesGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedCity = CitiesGrid.SelectedItem as Cities;
            if (_selectedCity != null)
                CityNameBox.Text = _selectedCity.NameCity;
        }
    }
}
