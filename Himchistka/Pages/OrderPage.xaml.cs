using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    /// <summary>
    /// Логика взаимодействия для OrderPage.xaml
    /// </summary>
    public partial class OrderPage : Page
    {
        private List<order> _allOrders = new List<order>();

        public OrderPage()
        {
            InitializeComponent();

            var mw = Application.Current.MainWindow as MainWindow;
            if (mw != null && AppConnect.user != null)
            {
                mw.fio.Text = $"{AppConnect.user.FIO} ({AppConnect.user.RoleName})";
            }

            // Права доступа к кнопкам управления
            bool isAdmin = AppConnect.user != null && AppConnect.user.roleId == 1;
            bool isManager = AppConnect.user != null && AppConnect.user.roleId == 2;

            btnAddOrder.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;
            btnEditOrder.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;
            btnTables.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;

            // Удаление запрещено для Менеджера (только Администратор)
            btnDeleteOrder.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            Load();
        }

        private void Load()
        {
            try
            {
                _allOrders = AppConnect.model0db.order.ToList();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки заказов: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            var list = _allOrders.AsEnumerable();

            string q = txtSearchOrder.Text.Trim().ToLower();
            if (q.Length > 0)
            {
                list = list.Where(o =>
                    o.id.ToString().Contains(q) ||
                    (o.ClientFIO != null && o.ClientFIO.ToLower().Contains(q)) ||
                    (o.user != null && o.user.phoneNumber != null && o.user.phoneNumber.ToLower().Contains(q)));
            }

            listOrders.ItemsSource = list.OrderByDescending(o => o.id).ToList();
        }

        private void Search_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void listOrders_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditOrder_Click(sender, e);
        }

        private void AddOrder_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.roleId != 1 && AppConnect.user?.roleId != 2)
            {
                MessageBox.Show("Создание заказов доступно персоналу химчистки.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AppFrame.mainFrame.Navigate(new AddOrderPage(null));
        }

        private void EditOrder_Click(object sender, RoutedEventArgs e)
        {
            if (!(listOrders.SelectedItem is order o))
            {
                MessageBox.Show("Пожалуйста, выберите заказ из списка для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            AppFrame.mainFrame.Navigate(new AddOrderPage(o));
        }

        private void DeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.roleId != 1)
            {
                MessageBox.Show("Удаление заказов доступно только Администратору.\nДля роли Менеджер удаление запрещено.",
                    "Доступ ограничен (F4)", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(listOrders.SelectedItem is order o))
            {
                MessageBox.Show("Пожалуйста, выберите заказ для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Бизнес-ограничение F4
            bool hasActiveWork = o.productOrder.Any(po =>
                po.status != null &&
                (po.status.status1.IndexOf("обработк", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 po.status.status1.IndexOf("Готов", StringComparison.OrdinalIgnoreCase) >= 0 ||
                 po.status.status1.IndexOf("Выдан", StringComparison.OrdinalIgnoreCase) >= 0));

            if (hasActiveWork)
            {
                MessageBox.Show($"Невозможно удалить заказ №{o.id} (бизнес-ограничение F4):\n" +
                    "Заказ находится в процессе обработки, готов к выдаче или уже выдан клиенту. Для удаления переведите заказ в статус «Отменен».",
                    "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Вы действительно хотите удалить заказ №{o.id} и все входящие в него позиции?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                AppConnect.model0db.productOrder.RemoveRange(o.productOrder);
                AppConnect.model0db.order.Remove(o);
                AppConnect.model0db.SaveChanges();
                MessageBox.Show($"Заказ №{o.id} успешно удален.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка удаления заказа:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Tables_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new DatabaseTablesPage("order"));
        }

        private void Products_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new ProductsPage());
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            AppConnect.user = null;
            var mw = Application.Current.MainWindow as MainWindow;
            if (mw != null) mw.UpdateNavigation();
            AppFrame.mainFrame.Navigate(new AuthorizationPage());
        }
    }
}
