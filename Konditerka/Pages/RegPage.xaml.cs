using Konditerka.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Konditerka.Pages
{
    /// <summary>
    /// Логика взаимодействия для RegPage.xaml
    /// </summary>
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            Fill();
        }

        public void Fill()
        {
            CityBox.SelectedIndex = 0;
            var city = AppConnect.model0db.Cities;
            CityBox.Items.Add("Город");
            foreach (var item in city)
            {
                CityBox.Items.Add(item.NameCity);
            }
        }

        private void GoToAutogizButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.GoBack();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (AppConnect.model0db.Users.Count(x => x.NameUser == LoginBox.Text) > 0)
                {
                    MessageBox.Show("Пользователь с таким логином уже есть!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (String.IsNullOrEmpty(LoginBox.Text) || String.IsNullOrEmpty(PassBox.Password) || String.IsNullOrEmpty(PassBoxV.Password) || String.IsNullOrEmpty(EmailBox.Text) ||
                    String.IsNullOrWhiteSpace(LoginBox.Text) || String.IsNullOrWhiteSpace(PassBox.Password) || String.IsNullOrWhiteSpace(PassBoxV.Password) || String.IsNullOrWhiteSpace(EmailBox.Text))
                {
                    MessageBox.Show("Заполнены не все поля!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (PassBox.Password.Length < 6)
                {
                    MessageBox.Show("Пароль должен быть не менее 6 символов!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (AppConnect.model0db.Users.Count(x => x.Password == PassBox.Password) > 0)
                {
                    MessageBox.Show("Этот пароль уже занят!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (AppConnect.model0db.Users.Count(x => x.Email == EmailBox.Text) > 0)
                {
                    MessageBox.Show("Эта почта уже занята!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (!AppData.ValidationPhoneAndEmail.IsValidEmail(EmailBox.Text))
                {
                    MessageBox.Show("Некорректно набрана почта(test@exapmle.com)!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (CityBox.SelectedIndex == 0)
                {
                    MessageBox.Show("Выберите город", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                Users userobj = new Users()
                {
                    NameUser = LoginBox.Text,
                    Password = PassBox.Password,
                    IdRole = 1,
                    IdCity = CityBox.SelectedIndex,
                    Email = EmailBox.Text
                };
                AppConnect.model0db.Users.Add(userobj);
                AppConnect.model0db.SaveChanges();
                MessageBox.Show("Данные успешно добавлены!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                AppData.AppFrame.framemain.GoBack();
            }
            catch
            {
                MessageBox.Show("Ошибка при добавлении данных!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PassBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PassBox.Password != PassBoxV.Password)
            {
                AddButton.IsEnabled = false;
                PassBoxV.Background = Brushes.LightCoral;
                PassBoxV.BorderBrush = Brushes.Red;
            }
            else
            {
                AddButton.IsEnabled = true;
                PassBoxV.Background = Brushes.LightGreen;
                PassBoxV.BorderBrush = Brushes.Green;
            }
        }

        private void PassBoxV_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (PassBox.Password != PassBoxV.Password)
            {
                AddButton.IsEnabled = false;
                PassBoxV.Background = Brushes.LightCoral;
                PassBoxV.BorderBrush = Brushes.Red;
            }
            else
            {
                AddButton.IsEnabled = true;
                PassBoxV.Background = Brushes.LightGreen;
                PassBoxV.BorderBrush = Brushes.Green;
            }
        }
    }
}
