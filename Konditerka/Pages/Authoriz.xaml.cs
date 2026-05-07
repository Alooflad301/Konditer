using Konditerka.AppData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Konditerka.Pages
{
    /// <summary>
    /// Логика взаимодействия для Authoriz.xaml
    /// </summary>
    public partial class Authoriz : Page
    {
        public Authoriz()
        {
            InitializeComponent();
        }

        private void OpenRegButton_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.framemain.Navigate(new RegPage());
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var userobj = AppData.AppConnect.model0db.Users.FirstOrDefault(x => x.NameUser == LoginBox.Text && x.Password == PassBox.Password);
                if (userobj == null)
                {
                    MessageBox.Show("Такого пользователя нет", "Ошибка авторизации", MessageBoxButton.OK, MessageBoxImage.Error);
                    PassBox.Clear();
                }
                else
                {
                    AppData.CurrentUser.User = userobj;
                    MessageBox.Show("Здравствуйте, " + userobj.NameUser + "!", "Уведомление", MessageBoxButton.OK, MessageBoxImage.Information);
                    AppFrame.framemain.Navigate(new PageOutput());
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка" + ex.Message.ToString(), "Критическая ошибка приложения", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}