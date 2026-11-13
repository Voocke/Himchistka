using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    public partial class UserOrdersPage : Page
    {
        private List<order> _userOrders = new List<order>();

        public UserOrdersPage()
        {
            InitializeComponent();

            if (AppConnect.user == null)
            {
                MessageBox.Show("Для просмотра истории заказов необходимо авторизоваться.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.mainFrame.Navigate(new AuthorizationPage());
                return;
            }

            tbClientInfo.Text = $"Клиент: {AppConnect.user.FIO}";

            // Настройка фильтра по статусу
            cmbStatusFilter.ItemsSource = new List<string>
            {
                "Все статусы",
                "Новый",
                "В обработке",
                "Готов к выдаче",
                "Выдан",
                "Отменен"
            };
            cmbStatusFilter.SelectedIndex = 0;

            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                int currentUserId = AppConnect.user.id;

                _userOrders = AppConnect.model0db.order
                    .Include(o => o.productOrder.Select(po => po.product))
                    .Include(o => o.productOrder.Select(po => po.status))
                    .Include(o => o.productOrder.Select(po => po.unit))
                    .Where(o => o.userid == currentUserId)
                    .OrderByDescending(o => o.id)
                    .ToList();

                // Расчет сводной статистики сверху страницы
                decimal totalOrdersSum = _userOrders.Sum(o => o.TotalSum);
                int totalOrdersCount = _userOrders.Count;
                int totalItemsCount = _userOrders.Sum(o => o.productOrder != null ? o.productOrder.Count : 0);

                tbTotalOrdersSum.Text = $"{totalOrdersSum:N0} ₽";
                tbTotalOrdersCount.Text = $"{totalOrdersCount}";
                tbTotalItemsCount.Text = $"{totalItemsCount} шт.";

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки истории заказов: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilter()
        {
            var list = _userOrders.AsEnumerable();

            // Фильтрация по строке поиска
            string query = txtSearch.Text?.Trim().ToLower() ?? "";
            if (!string.IsNullOrEmpty(query))
            {
                list = list.Where(o =>
                    o.id.ToString().Contains(query) ||
                    (o.productOrder != null && o.productOrder.Any(po =>
                        po.product != null && po.product.name != null && po.product.name.ToLower().Contains(query))));
            }

            // Фильтрация по статусу
            string selectedStatus = cmbStatusFilter.SelectedItem as string;
            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Все статусы")
            {
                list = list.Where(o =>
                    o.OrderStatus.IndexOf(selectedStatus, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (o.productOrder != null && o.productOrder.Any(po =>
                        po.status != null && po.status.status1 != null &&
                        po.status.status1.Equals(selectedStatus, StringComparison.OrdinalIgnoreCase))));
            }

            var result = list.ToList();

            if (result.Count == 0)
            {
                listOrders.Visibility = Visibility.Collapsed;
                panelEmpty.Visibility = Visibility.Visible;
            }
            else
            {
                panelEmpty.Visibility = Visibility.Collapsed;
                listOrders.Visibility = Visibility.Visible;
                listOrders.ItemsSource = result;
            }
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            ApplyFilter();
        }

        private void BackToCart_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new CartPage());
        }

        private void BackToCatalog_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new ProductsPage());
        }
    }
}