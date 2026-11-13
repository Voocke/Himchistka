using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    public partial class DatabaseTablesPage : Page
    {
        private string _currentTable = "product";
        private object _rawData;
        private bool _isInitialized = false;

        public DatabaseTablesPage(string preselectedTable = "product")
        {
            InitializeComponent();
            _isInitialized = true;
            _currentTable = preselectedTable ?? "product";

            UpdateRoleUI();
            SelectTableInList(_currentTable);
            LoadCurrentTable();
        }

        private void UpdateRoleUI()
        {
            bool isAdmin = AppConnect.user != null && AppConnect.user.roleId == 1;
            bool isManager = AppConnect.user != null && AppConnect.user.roleId == 2;

            if (isAdmin)
            {
                badgeRole.Background = (System.Windows.Media.Brush)FindResource("AccentColor");
                btnDelete.Visibility = Visibility.Visible;
            }
            else if (isManager)
            {
                badgeRole.Background = (System.Windows.Media.Brush)FindResource("DarkContrast");
                // Менеджеру удаление записей запрещено
                btnDelete.Visibility = Visibility.Collapsed;
            }
            else
            {
                btnDelete.Visibility = Visibility.Collapsed;
                btnAdd.Visibility = Visibility.Collapsed;
                btnEdit.Visibility = Visibility.Collapsed;
            }
        }

        private void SelectTableInList(string tableTag)
        {
            if (listTables == null) return;
            foreach (ListBoxItem item in listTables.Items)
            {
                if ((string)item.Tag == tableTag)
                {
                    listTables.SelectedItem = item;
                    break;
                }
            }
        }

        private void ListTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!_isInitialized || listTables == null) return;

            if (listTables.SelectedItem is ListBoxItem item && item.Tag != null)
            {
                _currentTable = item.Tag.ToString();
                if (txtSearch != null)
                {
                    txtSearch.Text = "";
                }
                LoadCurrentTable();
            }
        }

        public void LoadCurrentTable()
        {
            if (!_isInitialized || dgTableData == null) return;

            try
            {
                dgTableData.Columns.Clear();

                bool isAdmin = AppConnect.user != null && AppConnect.user.roleId == 1;
                bool isUserOrRole = _currentTable == "user" || _currentTable == "role";

                // Для менеджера пользователи и роли — только просмотр (добавление и редактирование запрещены)
                if (isUserOrRole && !isAdmin)
                {
                    btnAdd.IsEnabled = false;
                    btnEdit.IsEnabled = false;
                    btnAdd.ToolTip = "Управление пользователями и ролями доступно только Администратору.";
                    btnEdit.ToolTip = "Управление пользователями и ролями доступно только Администратору.";
                }
                else
                {
                    btnAdd.IsEnabled = true;
                    btnEdit.IsEnabled = true;
                    btnAdd.ToolTip = null;
                    btnEdit.ToolTip = null;
                }

                switch (_currentTable)
                {
                    case "product":
                        SetupProductColumns();
                        var products = AppConnect.model0db.product
                            .Include(p => p.category)
                            .Include(p => p.material)
                            .ToList();
                        _rawData = products;
                        tbTableTitle.Text = "Таблица: Услуги и изделия (product)";
                        break;

                    case "order":
                        SetupOrderColumns();
                        var orders = AppConnect.model0db.order
                            .Include(o => o.user)
                            .Include(o => o.productOrder)
                            .ToList();
                        _rawData = orders;
                        tbTableTitle.Text = "Таблица: Заказы (order)";
                        break;

                    case "productOrder":
                        SetupProductOrderColumns();
                        var pos = AppConnect.model0db.productOrder
                            .Include(p => p.product)
                            .Include(p => p.status)
                            .Include(p => p.unit)
                            .Include(p => p.order)
                            .ToList();
                        _rawData = pos;
                        tbTableTitle.Text = "Таблица: Состав заказов (productOrder)";
                        break;

                    case "category":
                        SetupSimpleColumns("Категория");
                        var categories = AppConnect.model0db.category
                            .Include(c => c.product)
                            .ToList();
                        _rawData = categories;
                        tbTableTitle.Text = "Таблица: Категории (category)";
                        break;

                    case "material":
                        SetupSimpleColumns("Материал");
                        var materials = AppConnect.model0db.material
                            .Include(m => m.product)
                            .ToList();
                        _rawData = materials;
                        tbTableTitle.Text = "Таблица: Материалы (material)";
                        break;

                    case "unit":
                        SetupUnitColumns();
                        var units = AppConnect.model0db.unit
                            .Include(u => u.productOrder)
                            .ToList();
                        _rawData = units;
                        tbTableTitle.Text = "Таблица: Единицы измерения (unit)";
                        break;

                    case "status":
                        SetupStatusColumns();
                        var statuses = AppConnect.model0db.status
                            .Include(s => s.productOrder)
                            .ToList();
                        _rawData = statuses;
                        tbTableTitle.Text = "Таблица: Статусы заказов (status)";
                        break;

                    case "user":
                        SetupUserColumns();
                        var users = AppConnect.model0db.user
                            .Include(u => u.role)
                            .Include(u => u.order)
                            .ToList();
                        _rawData = users;
                        tbTableTitle.Text = "Таблица: Пользователи системы (user)";
                        break;

                    case "role":
                        SetupRoleColumns();
                        var roles = AppConnect.model0db.role
                            .Include(r => r.user)
                            .ToList();
                        _rawData = roles;
                        tbTableTitle.Text = "Таблица: Роли пользователей (role)";
                        break;
                }

                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки таблицы: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SetupProductColumns()
        {
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("id"), Width = 45 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Наименование услуги", Binding = new Binding("name"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new Binding("CategoryName"), Width = 140 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Материал", Binding = new Binding("MaterialName"), Width = 130 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Цена (₽)", Binding = new Binding("price") { StringFormat = "{0:N0} ₽" }, Width = 90 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Скидка", Binding = new Binding("DiscountText"), Width = 70 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Итоговая цена", Binding = new Binding("FinalPrice") { StringFormat = "{0:N0} ₽" }, Width = 110 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Объем / фасовка", Binding = new Binding("amount"), Width = 120 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Файл фото", Binding = new Binding("image"), Width = 130 });
        }

        private void SetupOrderColumns()
        {
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("id"), Width = 50 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Клиент (ФИО)", Binding = new Binding("ClientFIO"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new Binding("user.phoneNumber"), Width = 125 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Дата оформления", Binding = new Binding("dateOfCart") { StringFormat = "dd.MM.yyyy HH:mm" }, Width = 135 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Дата выдачи", Binding = new Binding("dateOfBuy") { StringFormat = "dd.MM.yyyy" }, Width = 100 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new Binding("OrderStatus"), Width = 120 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Позиций", Binding = new Binding("productOrder.Count"), Width = 70 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Сумма (₽)", Binding = new Binding("TotalSum") { StringFormat = "{0:N0} ₽" }, Width = 100 });
        }

        private void SetupProductOrderColumns()
        {
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("id"), Width = 50 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "№ Заказа", Binding = new Binding("orderId"), Width = 75 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Услуга / Изделие", Binding = new Binding("product.name"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Категория", Binding = new Binding("product.CategoryName"), Width = 130 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Материал", Binding = new Binding("product.MaterialName"), Width = 120 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Статус", Binding = new Binding("StatusName"), Width = 130 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Ед. изм.", Binding = new Binding("UnitName"), Width = 75 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Стоимость", Binding = new Binding("Sum") { StringFormat = "{0:N0} ₽" }, Width = 100 });
        }

        private void SetupSimpleColumns(string entityName)
        {
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("id"), Width = 60 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = entityName, Binding = new Binding("Name"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Связанных услуг", Binding = new Binding("product.Count"), Width = 150 });
        }

        private void SetupUnitColumns()
        {
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("id"), Width = 60 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Единица измерения", Binding = new Binding("Name"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Использований в заказах", Binding = new Binding("productOrder.Count"), Width = 180 });
        }

        private void SetupStatusColumns()
        {
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("id"), Width = 60 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Наименование статуса", Binding = new Binding("Name"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Использований в заказах", Binding = new Binding("productOrder.Count"), Width = 180 });
        }

        private void SetupUserColumns()
        {
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("id"), Width = 50 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "ФИО пользователя", Binding = new Binding("FIO"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Роль", Binding = new Binding("RoleName"), Width = 130 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Телефон", Binding = new Binding("phoneNumber"), Width = 130 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Email (Логин)", Binding = new Binding("email"), Width = 180 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Пароль", Binding = new Binding("password"), Width = 110 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Заказов", Binding = new Binding("order.Count"), Width = 80 });
        }

        private void SetupRoleColumns()
        {
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "ID", Binding = new Binding("id"), Width = 60 });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Наименование роли", Binding = new Binding("Name"), Width = new DataGridLength(1, DataGridLengthUnitType.Star) });
            dgTableData.Columns.Add(new DataGridTextColumn { Header = "Пользователей с ролью", Binding = new Binding("user.Count"), Width = 180 });
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_isInitialized) return;
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            if (!_isInitialized || dgTableData == null || _rawData == null) return;
            string q = txtSearch != null ? txtSearch.Text.Trim().ToLower() : "";

            switch (_currentTable)
            {
                case "product":
                    var plist = (_rawData as List<product>) ?? new List<product>();
                    var pfilt = string.IsNullOrEmpty(q) ? plist : plist.Where(p =>
                        (p.name != null && p.name.ToLower().Contains(q)) ||
                        (p.CategoryName != null && p.CategoryName.ToLower().Contains(q)) ||
                        (p.MaterialName != null && p.MaterialName.ToLower().Contains(q)) ||
                        p.price.ToString().Contains(q) ||
                        p.id.ToString().Contains(q)).ToList();
                    dgTableData.ItemsSource = pfilt;
                    tbRecordCount.Text = $"Всего записей: {pfilt.Count} (из {plist.Count})";
                    break;

                case "order":
                    var olist = (_rawData as List<order>) ?? new List<order>();
                    var ofilt = string.IsNullOrEmpty(q) ? olist : olist.Where(o =>
                        o.id.ToString().Contains(q) ||
                        (o.ClientFIO != null && o.ClientFIO.ToLower().Contains(q)) ||
                        (o.user != null && o.user.phoneNumber != null && o.user.phoneNumber.ToLower().Contains(q)) ||
                        (o.OrderStatus != null && o.OrderStatus.ToLower().Contains(q))).ToList();
                    dgTableData.ItemsSource = ofilt;
                    tbRecordCount.Text = $"Всего записей: {ofilt.Count} (из {olist.Count})";
                    break;

                case "productOrder":
                    var polist = (_rawData as List<productOrder>) ?? new List<productOrder>();
                    var pofilt = string.IsNullOrEmpty(q) ? polist : polist.Where(po =>
                        po.id.ToString().Contains(q) ||
                        po.orderId.ToString().Contains(q) ||
                        (po.product != null && po.product.name != null && po.product.name.ToLower().Contains(q)) ||
                        (po.StatusName != null && po.StatusName.ToLower().Contains(q))).ToList();
                    dgTableData.ItemsSource = pofilt;
                    tbRecordCount.Text = $"Всего записей: {pofilt.Count} (из {polist.Count})";
                    break;

                case "category":
                    var clist = (_rawData as List<category>) ?? new List<category>();
                    var cfilt = string.IsNullOrEmpty(q) ? clist : clist.Where(c =>
                        c.id.ToString().Contains(q) ||
                        (c.category1 != null && c.category1.ToLower().Contains(q))).ToList();
                    dgTableData.ItemsSource = cfilt;
                    tbRecordCount.Text = $"Всего записей: {cfilt.Count} (из {clist.Count})";
                    break;

                case "material":
                    var mlist = (_rawData as List<material>) ?? new List<material>();
                    var mfilt = string.IsNullOrEmpty(q) ? mlist : mlist.Where(m =>
                        m.id.ToString().Contains(q) ||
                        (m.material1 != null && m.material1.ToLower().Contains(q))).ToList();
                    dgTableData.ItemsSource = mfilt;
                    tbRecordCount.Text = $"Всего записей: {mfilt.Count} (из {mlist.Count})";
                    break;

                case "unit":
                    var ulist = (_rawData as List<unit>) ?? new List<unit>();
                    var ufilt = string.IsNullOrEmpty(q) ? ulist : ulist.Where(u =>
                        u.id.ToString().Contains(q) ||
                        (u.unit1 != null && u.unit1.ToLower().Contains(q))).ToList();
                    dgTableData.ItemsSource = ufilt;
                    tbRecordCount.Text = $"Всего записей: {ufilt.Count} (из {ulist.Count})";
                    break;

                case "status":
                    var slist = (_rawData as List<status>) ?? new List<status>();
                    var sfilt = string.IsNullOrEmpty(q) ? slist : slist.Where(s =>
                        s.id.ToString().Contains(q) ||
                        (s.status1 != null && s.status1.ToLower().Contains(q))).ToList();
                    dgTableData.ItemsSource = sfilt;
                    tbRecordCount.Text = $"Всего записей: {sfilt.Count} (из {slist.Count})";
                    break;

                case "user":
                    var uslist = (_rawData as List<user>) ?? new List<user>();
                    var usfilt = string.IsNullOrEmpty(q) ? uslist : uslist.Where(u =>
                        u.id.ToString().Contains(q) ||
                        (u.FIO != null && u.FIO.ToLower().Contains(q)) ||
                        (u.email != null && u.email.ToLower().Contains(q)) ||
                        (u.phoneNumber != null && u.phoneNumber.ToLower().Contains(q)) ||
                        (u.RoleName != null && u.RoleName.ToLower().Contains(q))).ToList();
                    dgTableData.ItemsSource = usfilt;
                    tbRecordCount.Text = $"Всего записей: {usfilt.Count} (из {uslist.Count})";
                    break;

                case "role":
                    var rlist = (_rawData as List<role>) ?? new List<role>();
                    var rfilt = string.IsNullOrEmpty(q) ? rlist : rlist.Where(r =>
                        r.id.ToString().Contains(q) ||
                        (r.role1 != null && r.role1.ToLower().Contains(q))).ToList();
                    dgTableData.ItemsSource = rfilt;
                    tbRecordCount.Text = $"Всего записей: {rfilt.Count} (из {rlist.Count})";
                    break;
            }
        }

        private void DgTableData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            Edit_Click(sender, e);
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            bool isAdmin = AppConnect.user != null && AppConnect.user.roleId == 1;

            if ((_currentTable == "user" || _currentTable == "role") && !isAdmin)
            {
                MessageBox.Show("Добавление пользователей и ролей доступно только Администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            switch (_currentTable)
            {
                case "product":
                    var winP = new AddEditProductWindow();
                    winP.Owner = Application.Current.MainWindow;
                    if (winP.ShowDialog() == true) LoadCurrentTable();
                    break;

                case "order":
                    AppFrame.mainFrame.Navigate(new AddOrderPage(null));
                    break;

                case "productOrder":
                    var winPO = new AddEditProductOrderWindow();
                    winPO.Owner = Application.Current.MainWindow;
                    if (winPO.ShowDialog() == true) LoadCurrentTable();
                    break;

                case "category":
                    var winC = new AddEditSimpleWindow("Добавление категории", "Наименование категории:");
                    winC.Owner = Application.Current.MainWindow;
                    if (winC.ShowDialog() == true)
                    {
                        var c = new category { category1 = winC.ResultValue };
                        AppConnect.model0db.category.Add(c);
                        AppConnect.model0db.SaveChanges();
                        LoadCurrentTable();
                    }
                    break;

                case "material":
                    var winM = new AddEditSimpleWindow("Добавление материала", "Наименование материала:");
                    winM.Owner = Application.Current.MainWindow;
                    if (winM.ShowDialog() == true)
                    {
                        var m = new material { material1 = winM.ResultValue };
                        AppConnect.model0db.material.Add(m);
                        AppConnect.model0db.SaveChanges();
                        LoadCurrentTable();
                    }
                    break;

                case "unit":
                    var winU = new AddEditSimpleWindow("Добавление единицы измерения", "Наименование единицы (напр. шт., пара, комплект):");
                    winU.Owner = Application.Current.MainWindow;
                    if (winU.ShowDialog() == true)
                    {
                        var un = new unit { unit1 = winU.ResultValue };
                        AppConnect.model0db.unit.Add(un);
                        AppConnect.model0db.SaveChanges();
                        LoadCurrentTable();
                    }
                    break;

                case "status":
                    var winS = new AddEditSimpleWindow("Добавление статуса", "Наименование статуса заказа:");
                    winS.Owner = Application.Current.MainWindow;
                    if (winS.ShowDialog() == true)
                    {
                        var s = new status { status1 = winS.ResultValue };
                        AppConnect.model0db.status.Add(s);
                        AppConnect.model0db.SaveChanges();
                        LoadCurrentTable();
                    }
                    break;

                case "user":
                    var winUs = new AddEditUserWindow();
                    winUs.Owner = Application.Current.MainWindow;
                    if (winUs.ShowDialog() == true) LoadCurrentTable();
                    break;

                case "role":
                    var winR = new AddEditSimpleWindow("Добавление роли", "Наименование роли:");
                    winR.Owner = Application.Current.MainWindow;
                    if (winR.ShowDialog() == true)
                    {
                        var r = new role { role1 = winR.ResultValue };
                        AppConnect.model0db.role.Add(r);
                        AppConnect.model0db.SaveChanges();
                        LoadCurrentTable();
                    }
                    break;
            }
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (dgTableData.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись из таблицы для редактирования.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            bool isAdmin = AppConnect.user != null && AppConnect.user.roleId == 1;
            if ((_currentTable == "user" || _currentTable == "role") && !isAdmin)
            {
                MessageBox.Show("Редактирование пользователей и их ролей доступно только Администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            switch (_currentTable)
            {
                case "product":
                    {
                        var p = dgTableData.SelectedItem as product;
                        if (p != null)
                        {
                            var win = new AddEditProductWindow(p);
                            win.Owner = Application.Current.MainWindow;
                            if (win.ShowDialog() == true) LoadCurrentTable();
                        }
                        break;
                    }

                case "order":
                    {
                        var o = dgTableData.SelectedItem as order;
                        if (o != null)
                        {
                            AppFrame.mainFrame.Navigate(new AddOrderPage(o));
                        }
                        break;
                    }

                case "productOrder":
                    {
                        var po = dgTableData.SelectedItem as productOrder;
                        if (po != null)
                        {
                            var win = new AddEditProductOrderWindow(po, po.orderId);
                            win.Owner = Application.Current.MainWindow;
                            if (win.ShowDialog() == true) LoadCurrentTable();
                        }
                        break;
                    }

                case "category":
                    {
                        var c = dgTableData.SelectedItem as category;
                        if (c != null)
                        {
                            var win = new AddEditSimpleWindow("Редактирование категории", "Наименование категории:", c.category1);
                            win.Owner = Application.Current.MainWindow;
                            if (win.ShowDialog() == true)
                            {
                                c.category1 = win.ResultValue;
                                AppConnect.model0db.SaveChanges();
                                LoadCurrentTable();
                            }
                        }
                        break;
                    }

                case "material":
                    {
                        var m = dgTableData.SelectedItem as material;
                        if (m != null)
                        {
                            var win = new AddEditSimpleWindow("Редактирование материала", "Наименование материала:", m.material1);
                            win.Owner = Application.Current.MainWindow;
                            if (win.ShowDialog() == true)
                            {
                                m.material1 = win.ResultValue;
                                AppConnect.model0db.SaveChanges();
                                LoadCurrentTable();
                            }
                        }
                        break;
                    }

                case "unit":
                    {
                        var un = dgTableData.SelectedItem as unit;
                        if (un != null)
                        {
                            var win = new AddEditSimpleWindow("Редактирование единицы измерения", "Наименование единицы:", un.unit1);
                            win.Owner = Application.Current.MainWindow;
                            if (win.ShowDialog() == true)
                            {
                                un.unit1 = win.ResultValue;
                                AppConnect.model0db.SaveChanges();
                                LoadCurrentTable();
                            }
                        }
                        break;
                    }

                case "status":
                    {
                        var s = dgTableData.SelectedItem as status;
                        if (s != null)
                        {
                            var win = new AddEditSimpleWindow("Редактирование статуса", "Наименование статуса:", s.status1);
                            win.Owner = Application.Current.MainWindow;
                            if (win.ShowDialog() == true)
                            {
                                s.status1 = win.ResultValue;
                                AppConnect.model0db.SaveChanges();
                                LoadCurrentTable();
                            }
                        }
                        break;
                    }

                case "user":
                    {
                        var u = dgTableData.SelectedItem as user;
                        if (u != null)
                        {
                            var win = new AddEditUserWindow(u);
                            win.Owner = Application.Current.MainWindow;
                            if (win.ShowDialog() == true) LoadCurrentTable();
                        }
                        break;
                    }

                case "role":
                    {
                        var r = dgTableData.SelectedItem as role;
                        if (r != null)
                        {
                            var win = new AddEditSimpleWindow("Редактирование роли", "Наименование роли:", r.role1);
                            win.Owner = Application.Current.MainWindow;
                            if (win.ShowDialog() == true)
                            {
                                r.role1 = win.ResultValue;
                                AppConnect.model0db.SaveChanges();
                                LoadCurrentTable();
                            }
                        }
                        break;
                    }
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.roleId != 1)
            {
                MessageBox.Show("Удаление записей запрещено для роли Менеджер.\nДанное действие доступно только Администратору.",
                    "Запрещено (F4)", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dgTableData.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Бизнес-ограничения F4 при удалении
            switch (_currentTable)
            {
                case "product":
                    {
                        var p = dgTableData.SelectedItem as product;
                        if (p == null) return;
                        if (AppConnect.model0db.productOrder.Any(x => x.productId == p.id))
                        {
                            MessageBox.Show($"Невозможно удалить услугу «{p.name}» (бизнес-ограничение F4):\nДанная услуга уже оформлена в существующих клиентских заказах.",
                                "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (ConfirmDelete($"Вы действительно хотите удалить услугу «{p.name}»?"))
                        {
                            AppConnect.model0db.product.Remove(p);
                            CommitDelete();
                        }
                        break;
                    }

                case "order":
                    {
                        var o = dgTableData.SelectedItem as order;
                        if (o == null) return;
                        bool hasActiveWork = o.productOrder.Any(x =>
                            x.status != null &&
                            (x.status.status1.IndexOf("обработк", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             x.status.status1.IndexOf("Готов", StringComparison.OrdinalIgnoreCase) >= 0 ||
                             x.status.status1.IndexOf("Выдан", StringComparison.OrdinalIgnoreCase) >= 0));

                        if (hasActiveWork)
                        {
                            MessageBox.Show($"Невозможно удалить заказ №{o.id} (бизнес-ограничение F4):\n" +
                                "Заказ находится в процессе обработки, готов к выдаче или уже выдан клиенту. Для удаления отмените заказ.",
                                "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }

                        if (ConfirmDelete($"Вы действительно хотите удалить заказ №{o.id} и все входящие в него позиции?"))
                        {
                            AppConnect.model0db.productOrder.RemoveRange(o.productOrder);
                            AppConnect.model0db.order.Remove(o);
                            CommitDelete();
                        }
                        break;
                    }

                case "productOrder":
                    {
                        var po = dgTableData.SelectedItem as productOrder;
                        if (po == null) return;
                        if (ConfirmDelete($"Удалить позицию #{po.id} (услуга «{po.product?.name}») из заказа №{po.orderId}?"))
                        {
                            AppConnect.model0db.productOrder.Remove(po);
                            CommitDelete();
                        }
                        break;
                    }

                case "category":
                    {
                        var c = dgTableData.SelectedItem as category;
                        if (c == null) return;
                        if (AppConnect.model0db.product.Any(x => x.categoryId == c.id))
                        {
                            MessageBox.Show($"Невозможно удалить категорию «{c.category1}» (бизнес-ограничение F4):\n" +
                                "В этой категории зарегистрированы услуги в каталоге. Сначала удалите или перенесите услуги.",
                                "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (ConfirmDelete($"Вы действительно хотите удалить категорию «{c.category1}»?"))
                        {
                            AppConnect.model0db.category.Remove(c);
                            CommitDelete();
                        }
                        break;
                    }

                case "material":
                    {
                        var m = dgTableData.SelectedItem as material;
                        if (m == null) return;
                        if (AppConnect.model0db.product.Any(x => x.materialId == m.id))
                        {
                            MessageBox.Show($"Невозможно удалить материал «{m.material1}» (бизнес-ограничение F4):\n" +
                                "Данный материал назначен услугам в каталоге. Сначала измените материал у этих услуг.",
                                "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (ConfirmDelete($"Вы действительно хотите удалить материал «{m.material1}»?"))
                        {
                            AppConnect.model0db.material.Remove(m);
                            CommitDelete();
                        }
                        break;
                    }

                case "unit":
                    {
                        var un = dgTableData.SelectedItem as unit;
                        if (un == null) return;
                        if (AppConnect.model0db.productOrder.Any(x => x.unitId == un.id))
                        {
                            MessageBox.Show($"Невозможно удалить единицу измерения «{un.unit1}» (бизнес-ограничение F4):\n" +
                                "Данная единица измерения используется в оформленных позициях заказов.",
                                "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (ConfirmDelete($"Вы действительно хотите удалить единицу измерения «{un.unit1}»?"))
                        {
                            AppConnect.model0db.unit.Remove(un);
                            CommitDelete();
                        }
                        break;
                    }

                case "status":
                    {
                        var s = dgTableData.SelectedItem as status;
                        if (s == null) return;
                        if (AppConnect.model0db.productOrder.Any(x => x.statusId == s.id))
                        {
                            MessageBox.Show($"Невозможно удалить статус «{s.status1}» (бизнес-ограничение F4):\n" +
                                "Данный технологический статус назначен позициям клиентских заказов.",
                                "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (ConfirmDelete($"Вы действительно хотите удалить статус «{s.status1}»?"))
                        {
                            AppConnect.model0db.status.Remove(s);
                            CommitDelete();
                        }
                        break;
                    }

                case "user":
                    {
                        var u = dgTableData.SelectedItem as user;
                        if (u == null) return;
                        if (u.id == AppConnect.user.id)
                        {
                            MessageBox.Show("Невозможно удалить текущего авторизованного пользователя (бизнес-ограничение F4).",
                                "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (AppConnect.model0db.order.Any(x => x.userid == u.id))
                        {
                            MessageBox.Show($"Невозможно удалить пользователя «{u.FIO}» (бизнес-ограничение F4):\n" +
                                "У данного клиента имеются оформленные заказы в системе. История заказов должна сохраняться.",
                                "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (ConfirmDelete($"Вы действительно хотите удалить пользователя «{u.FIO}» ({u.email})?"))
                        {
                            AppConnect.model0db.user.Remove(u);
                            CommitDelete();
                        }
                        break;
                    }

                case "role":
                    {
                        var r = dgTableData.SelectedItem as role;
                        if (r == null) return;
                        if (AppConnect.model0db.user.Any(x => x.roleId == r.id))
                        {
                            MessageBox.Show($"Невозможно удалить роль «{r.role1}» (бизнес-ограничение F4):\n" +
                                "Данная роль назначена пользователям системы. Сначала переназначьте их роли.",
                                "Бизнес-ограничение F4", MessageBoxButton.OK, MessageBoxImage.Warning);
                            return;
                        }
                        if (ConfirmDelete($"Вы действительно хотите удалить роль «{r.role1}»?"))
                        {
                            AppConnect.model0db.role.Remove(r);
                            CommitDelete();
                        }
                        break;
                    }
            }
        }

        private bool ConfirmDelete(string message)
        {
            return MessageBox.Show(message, "Подтверждение удаления",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        private void CommitDelete()
        {
            try
            {
                AppConnect.model0db.SaveChanges();
                MessageBox.Show("Запись успешно удалена.", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCurrentTable();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при удалении записи:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            LoadCurrentTable();
        }

        private void BackToCatalog_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new ProductsPage());
        }
    }
}
