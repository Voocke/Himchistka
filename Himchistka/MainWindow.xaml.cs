using System;
using System.Windows;
using System.Windows.Controls;
using Himchistka.AppData;
using Himchistka.Pages;

namespace Himchistka
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            AppConnect.model0db = new himchistkaEntities();
            AppFrame.mainFrame = MainFrame;

            MainFrame.Navigated += (s, e) => UpdateNavigation();

            MainFrame.Navigate(new AuthorizationPage());
        }

        public void UpdateNavigation()
        {
            bool isAuthPage = MainFrame.Content is AuthorizationPage;

            if (isAuthPage)
            {
                navBorder.Visibility = Visibility.Collapsed;
                fio.Text = "";
                return;
            }

            navBorder.Visibility = Visibility.Visible;

            if (AppConnect.user == null)
            {
                fio.Text = "Гость";
                menuCatalog.Visibility = Visibility.Visible;
                menuCart.Visibility = Visibility.Visible;
                menuMyOrders.Visibility = Visibility.Collapsed;
                menuOrders.Visibility = Visibility.Collapsed;
                menuTables.Visibility = Visibility.Collapsed;
                btnAuthToggle.Content = "Войти";
                return;
            }

            fio.Text = $"{AppConnect.user.FIO} ({AppConnect.user.RoleName})";
            btnAuthToggle.Content = "Выход";

            int role = AppConnect.user.roleId;

            // 1 - Администратор, 2 - Менеджер, 3 - Клиент
            if (role == 1 || role == 2)
            {
                menuCatalog.Visibility = Visibility.Visible;
                menuCart.Visibility = Visibility.Visible;
                menuMyOrders.Visibility = Visibility.Collapsed;
                menuOrders.Visibility = Visibility.Visible;
                menuTables.Visibility = Visibility.Visible;
            }
            else
            {
                // Клиент
                menuCatalog.Visibility = Visibility.Visible;
                menuCart.Visibility = Visibility.Visible;
                menuMyOrders.Visibility = Visibility.Visible;
                menuOrders.Visibility = Visibility.Collapsed;
                menuTables.Visibility = Visibility.Collapsed;
            }
        }

        private void NavCatalog_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new ProductsPage());
        }

        private void NavCart_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new CartPage());
        }

        private void NavMyOrders_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new UserOrdersPage());
        }

        private void NavOrders_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new OrderPage());
        }

        private void NavTables_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new DatabaseTablesPage("product"));
        }

        private void NavTableItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem mi && mi.Tag != null)
            {
                string tag = mi.Tag.ToString();
                AppFrame.mainFrame.Navigate(new DatabaseTablesPage(tag));
            }
        }

        private void NavAuthToggle_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.user = null;
            UpdateNavigation();
            AppFrame.mainFrame.Navigate(new AuthorizationPage());
        }
    }
}
