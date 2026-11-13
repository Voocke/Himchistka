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
    /// Логика взаимодействия для ProductsPage.xaml
    /// </summary>
    public partial class ProductsPage : Page
    {
        private List<product> _all = new List<product>();

        public ProductsPage()
        {
            InitializeComponent();

            var mw = Application.Current.MainWindow as MainWindow;
            if (mw != null)
            {
                mw.fio.Text = AppConnect.user != null
                    ? $"{AppConnect.user.FIO} ({AppConnect.user.RoleName})"
                    : "Гость";
            }

            // Управление правами доступа
            bool isAdmin = AppConnect.user != null && AppConnect.user.roleId == 1;
            bool isManager = AppConnect.user != null && AppConnect.user.roleId == 2;

            btnAddProduct.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;
            btnEditProduct.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;
            btnTables.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;
            btnOrders.Visibility = (isAdmin || isManager) ? Visibility.Visible : Visibility.Collapsed;

            // Удаление доступно только Администратору (RoleId == 1). Менеджеру удаление запрещено.
            btnDelete.Visibility = isAdmin ? Visibility.Visible : Visibility.Collapsed;

            Load();
        }

        private void Load()
        {
            try
            {
                _all = AppConnect.model0db.product.ToList();

                cmbSort.Items.Clear();
                cmbSort.Items.Add("Без сортировки");
                cmbSort.Items.Add("Цена ↑");
                cmbSort.Items.Add("Цена ↓");
                cmbSort.SelectedIndex = 0;

                cmbFilter.Items.Clear();
                cmbFilter.Items.Add("Все категории");
                foreach (var c in AppConnect.model0db.category.ToList())
                {
                    cmbFilter.Items.Add(c.category1);
                }
                cmbFilter.SelectedIndex = 0;

                Apply();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Apply()
        {
            var list = _all.AsEnumerable();

            string q = txtSearch.Text.Trim().ToLower();
            if (q.Length > 0)
            {
                list = list.Where(p =>
                    (p.name != null && p.name.ToLower().Contains(q)) ||
                    (p.category != null && p.category.category1 != null && p.category.category1.ToLower().Contains(q)) ||
                    (p.material != null && p.material.material1 != null && p.material.material1.ToLower().Contains(q)));
            }

            if (cmbFilter.SelectedIndex > 0)
            {
                string selectedCat = cmbFilter.SelectedItem.ToString();
                list = list.Where(p => p.category != null && p.category.category1 == selectedCat);
            }

            if (cmbSort.SelectedIndex == 1)
                list = list.OrderBy(p => p.FinalPrice);
            else if (cmbSort.SelectedIndex == 2)
                list = list.OrderByDescending(p => p.FinalPrice);

            var resultList = list.ToList();
            listProduct.ItemsSource = resultList;

            if (tbRecordCount != null)
            {
                tbRecordCount.Text = $"Найдено: {resultList.Count} из {_all.Count}";
            }
        }

        private void ResetFilters_Click(object sender, RoutedEventArgs e)
        {
            txtSearch.Text = "";
            if (cmbFilter.Items.Count > 0) cmbFilter.SelectedIndex = 0;
            if (cmbSort.Items.Count > 0) cmbSort.SelectedIndex = 0;
            Apply();
        }

        private void Filters_Changed(object sender, EventArgs e) => Apply();

        private void listProduct_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!(listProduct.SelectedItem is product p)) return;

            AppFrame.mainFrame.Navigate(new ProductDetailsPage(p));
        }

        private void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddEditProductWindow();
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true) Load();
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (!(listProduct.SelectedItem is product p))
            {
                MessageBox.Show("Пожалуйста, выберите услугу из каталога для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new AddEditProductWindow(p);
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true) Load();
        }

        private void Tables_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new DatabaseTablesPage("product"));
        }

        private void Del_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.roleId != 1)
            {
                MessageBox.Show("Удаление доступно только Администратору.\nДля роли Менеджер удаление запрещено.",
                    "Доступ ограничен (F4)", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(listProduct.SelectedItem is product p))
            {
                MessageBox.Show("Пожалуйста, выберите услугу из списка для удаления.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Бизнес-ограничение F4
            if (AppConnect.model0db.productOrder.Any(po => po.productId == p.id))
            {
                MessageBox.Show($"Невозможно удалить услугу «{p.name}» (бизнес-ограничение F4):\n" +
                    "Данная услуга уже присутствует в оформленных заказах клиентов.",
                    "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show($"Вы действительно хотите удалить услугу «{p.name}»?", "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            try
            {
                AppConnect.model0db.product.Remove(p);
                AppConnect.model0db.SaveChanges();
                MessageBox.Show("Услуга успешно удалена.", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                Load();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Не удалось удалить услугу:\n" + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cart_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new CartPage());
        }

        private void Orders_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Для просмотра истории заказов необходимо авторизоваться.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.mainFrame.Navigate(new AuthorizationPage());
                return;
            }

            if (AppConnect.user.roleId == 1 || AppConnect.user.roleId == 2)
            {
                AppFrame.mainFrame.Navigate(new OrderPage());
            }
            else
            {
                AppFrame.mainFrame.Navigate(new UserOrdersPage());
            }
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
