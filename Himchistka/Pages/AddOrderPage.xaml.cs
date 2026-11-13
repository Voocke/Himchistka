using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    public partial class AddOrderPage : Page
    {
        private order _order;
        private bool _isNew;
        private List<productOrder> _tempItems = new List<productOrder>();

        public AddOrderPage(order order)
        {
            InitializeComponent();

            _isNew = order == null;
            _order = order ?? new order();

            LoadLookups();

            if (_isNew)
            {
                tbHeader.Text = "Новый заказ";
                dpDateOfCart.SelectedDate = DateTime.Now;
                dpDateOfBuy.SelectedDate = null;
            }
            else
            {
                tbHeader.Text = $"Редактирование заказа №{_order.id}";
                dpDateOfCart.SelectedDate = _order.dateOfCart ?? DateTime.Now;
                dpDateOfBuy.SelectedDate = _order.dateOfBuy;
                cbClient.SelectedValue = _order.userid;
            }

            LoadItems();
        }

        private void LoadLookups()
        {
            try
            {
                cbClient.ItemsSource = AppConnect.model0db.user.ToList();
                cbMassStatus.ItemsSource = AppConnect.model0db.status.ToList();
                if (cbMassStatus.Items.Count > 0)
                    cbMassStatus.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки справочников: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadItems()
        {
            if (_isNew)
            {
                dgItems.ItemsSource = null;
                dgItems.ItemsSource = _tempItems;
                decimal sum = _tempItems.Sum(i => i.product != null ? i.product.FinalPrice : 0m);
                tbTotal.Text = $"Итого: {sum:N0} ₽";
            }
            else
            {
                var items = AppConnect.model0db.productOrder
                    .Where(po => po.orderId == _order.id)
                    .ToList();
                dgItems.ItemsSource = items;
                decimal sum = items.Sum(i => i.product != null ? i.product.FinalPrice : 0m);
                tbTotal.Text = $"Итого: {sum:N0} ₽";
            }
        }

        private void ApplyMassStatus_Click(object sender, RoutedEventArgs e)
        {
            if (cbMassStatus.SelectedValue == null) return;
            int newStatusId = (int)cbMassStatus.SelectedValue;

            if (_isNew)
            {
                var st = AppConnect.model0db.status.Find(newStatusId);
                foreach (var item in _tempItems)
                {
                    item.statusId = newStatusId;
                    item.status = st;
                }
                dgItems.Items.Refresh();
            }
            else
            {
                var items = AppConnect.model0db.productOrder
                    .Where(po => po.orderId == _order.id)
                    .ToList();

                foreach (var item in items)
                {
                    item.statusId = newStatusId;
                }

                try
                {
                    AppConnect.model0db.SaveChanges();
                    LoadItems();
                    MessageBox.Show("Статус всех позиций успешно обновлен.", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка обновления статусов: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddEditProductOrderWindow(null, _isNew ? null : (int?)_order.id);
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true && win.CreatedItem != null)
            {
                if (_isNew)
                {
                    _tempItems.Add(win.CreatedItem);
                    LoadItems();
                }
                else
                {
                    LoadItems();
                }
            }
        }

        private void ChangeItemStatus_Click(object sender, RoutedEventArgs e)
        {
            if (!(dgItems.SelectedItem is productOrder po))
            {
                MessageBox.Show("Пожалуйста, выберите позицию из списка.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var win = new AddEditProductOrderWindow(po, _order.id);
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true)
            {
                LoadItems();
            }
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user?.roleId != 1)
            {
                MessageBox.Show("Удаление позиций из заказа доступно только Администратору.",
                    "Доступ ограничен", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!(dgItems.SelectedItem is productOrder po))
            {
                MessageBox.Show("Пожалуйста, выберите позицию для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (MessageBox.Show("Вы уверены, что хотите удалить данную позицию из заказа?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            if (_isNew)
            {
                _tempItems.Remove(po);
                LoadItems();
            }
            else
            {
                try
                {
                    AppConnect.model0db.productOrder.Remove(po);
                    AppConnect.model0db.SaveChanges();
                    LoadItems();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления позиции: " + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (cbClient.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите клиента.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int selectedUserId = (int)cbClient.SelectedValue;

            try
            {
                if (_isNew)
                {
                    if (_tempItems.Count == 0)
                    {
                        MessageBox.Show("Добавьте хотя бы одну позицию (услугу) в заказ.", "Внимание",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    _order.userid = selectedUserId;
                    _order.dateOfCart = dpDateOfCart.SelectedDate ?? DateTime.Now;
                    _order.dateOfBuy = dpDateOfBuy.SelectedDate;

                    AppConnect.model0db.order.Add(_order);
                    AppConnect.model0db.SaveChanges();

                    foreach (var item in _tempItems)
                    {
                        item.orderId = _order.id;
                        item.product = null;
                        item.status = null;
                        item.unit = null;
                        AppConnect.model0db.productOrder.Add(item);
                    }
                    AppConnect.model0db.SaveChanges();

                    MessageBox.Show($"Заказ №{_order.id} успешно создан!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    _order.userid = selectedUserId;
                    _order.dateOfCart = dpDateOfCart.SelectedDate ?? DateTime.Now;
                    _order.dateOfBuy = dpDateOfBuy.SelectedDate;

                    AppConnect.model0db.SaveChanges();
                    MessageBox.Show($"Заказ №{_order.id} успешно сохранен!", "Успех",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }

                AppFrame.mainFrame.Navigate(new OrderPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения заказа: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new OrderPage());
        }
    }
}
