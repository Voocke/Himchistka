using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    /// <summary>
    /// Логика взаимодействия для AuthorizationPage.xaml
    /// </summary>
    public partial class AuthorizationPage : Page
    {
        public AuthorizationPage()
        {
            InitializeComponent();
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            string login = txtLogin.Text.Trim();
            string password = txtPassword.Password.Trim();

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Пожалуйста, введите логин и пароль.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var u = AppConnect.model0db.user.FirstOrDefault(x =>
                    (x.email == login || x.phoneNumber == login) && x.password == password);

                if (u == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден.", "Ошибка входа",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                AppConnect.user = u;

                var mw = Application.Current.MainWindow as MainWindow;
                if (mw != null)
                {
                    mw.UpdateNavigation();
                }

                // 1 = Администратор, 2 = Менеджер, 3 = Клиент
                if (u.roleId == 1 || u.roleId == 2)
                    AppFrame.mainFrame.Navigate(new OrderPage());
                else
                    AppFrame.mainFrame.Navigate(new ProductsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подключения к базе данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Guest_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.user = null;

            var mw = Application.Current.MainWindow as MainWindow;
            if (mw != null)
            {
                mw.UpdateNavigation();
            }

            AppFrame.mainFrame.Navigate(new ProductsPage());
        }
    }
}
