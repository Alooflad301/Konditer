using Konditerka.AppData;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Konditerka.Pages
{
    public partial class UsersAdminPage : Page
    {
        private Users _selectedUser;

        public UsersAdminPage()
        {
            InitializeComponent();
            RoleBox.ItemsSource = AppConnect.model0db.Roles.ToList();
            CityBox.ItemsSource = AppConnect.model0db.Cities.ToList();
            RefreshData();
        }

        private void RefreshData()
        {
            UsersGrid.ItemsSource = AppConnect.model0db.Users.ToList();
            UsersGrid.SelectedItem = null;
            _selectedUser = null;
            UserNameBox.Text = string.Empty;
            EmailBox.Text = string.Empty;
            PasswordBox.Text = string.Empty;
            RoleBox.SelectedIndex = -1;
            CityBox.SelectedIndex = -1;
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(UserNameBox.Text))
            {
                MessageBox.Show("Введите имя пользователя");
                return false;
            }

            if (string.IsNullOrWhiteSpace(EmailBox.Text) || !ValidationPhoneAndEmail.IsValidEmail(EmailBox.Text.Trim()))
            {
                MessageBox.Show("Введите корректный email");
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Text) || PasswordBox.Text.Length < 6)
            {
                MessageBox.Show("Пароль должен быть не менее 6 символов");
                return false;
            }

            if (RoleBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите роль");
                return false;
            }

            if (CityBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите город");
                return false;
            }

            return true;
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для редактирования");
                return;
            }
            if (!ValidateForm()) return;

            _selectedUser.NameUser = UserNameBox.Text.Trim();
            _selectedUser.Email = EmailBox.Text.Trim();
            _selectedUser.Password = PasswordBox.Text;
            _selectedUser.IdRole = (int)RoleBox.SelectedValue;
            _selectedUser.IdCity = (int)CityBox.SelectedValue;
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedUser == null)
            {
                MessageBox.Show("Выберите пользователя для удаления");
                return;
            }
            var result = MessageBox.Show("Удалить выбранного пользователя?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            AppConnect.model0db.Users.Remove(_selectedUser);
            AppConnect.model0db.SaveChanges();
            RefreshData();
        }

        private void UsersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedUser = UsersGrid.SelectedItem as Users;
            if (_selectedUser == null) return;
            UserNameBox.Text = _selectedUser.NameUser;
            EmailBox.Text = _selectedUser.Email;
            PasswordBox.Text = _selectedUser.Password;
            RoleBox.SelectedValue = _selectedUser.IdRole;
            CityBox.SelectedValue = _selectedUser.IdCity;
        }
    }
}
