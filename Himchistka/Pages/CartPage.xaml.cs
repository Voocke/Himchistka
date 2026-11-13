using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    /// <summary>
    /// Логика взаимодействия для CartPage.xaml
    /// </summary>
    public partial class CartPage : Page
    {
        public CartPage()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            listCart.ItemsSource = null;
            listCart.ItemsSource = Cart.Items;
            tbTotal.Text = $"{Cart.TotalSum():N0} ₽";
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем ввод только цифр
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Qty_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Qty_LostFocus(sender, e);
            }
        }

        private void Qty_LostFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is CartItem item)
            {
                string text = tb.Text?.Trim();
                if (string.IsNullOrEmpty(text))
                {
                    item.Quantity = 1;
                    tb.Text = "1";
                    Refresh();
                    return;
                }

                if (int.TryParse(text, out int val))
                {
                    if (val <= 0)
                    {
                        MessageBox.Show("Количество должно быть не менее 1 шт.",
                            "Некорректное количество", MessageBoxButton.OK, MessageBoxImage.Warning);
                        item.Quantity = 1;
                        tb.Text = "1";
                    }
                    else if (val > 300)
                    {
                        MessageBox.Show("Нельзя заказать больше 300 единиц одной услуги за раз.",
                            "Ограничение количества", MessageBoxButton.OK, MessageBoxImage.Warning);
                        item.Quantity = 300;
                        tb.Text = "300";
                    }
                    else
                    {
                        item.Quantity = val;
                    }
                }
                else
                {
                    MessageBox.Show("Введите корректное целое число от 1 до 300.",
                        "Некорректное значение", MessageBoxButton.OK, MessageBoxImage.Warning);
                    tb.Text = item.Quantity.ToString();
                }
                Refresh();
            }
        }

        private void Remove_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is CartItem item)
            {
                Cart.Items.Remove(item);
                Refresh();
            }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Items.Count == 0) return;

            if (MessageBox.Show("Очистить все позиции из корзины?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Cart.Clear();
                Refresh();
            }
        }

        private void CreateOrder_Click(object sender, RoutedEventArgs e)
        {
            if (Cart.Items.Count == 0)
            {
                MessageBox.Show("Корзина пуста. Добавьте услуги из каталога.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Валидация: не более 300 на одну позицию
            var invalidItem = Cart.Items.FirstOrDefault(i => i.Quantity <= 0 || i.Quantity > 300);
            if (invalidItem != null)
            {
                MessageBox.Show($"В позиции «{invalidItem.ProductName}» указано некорректное количество ({invalidItem.Quantity}).\n" +
                    "Количество для каждой услуги должно быть от 1 до 300 шт.",
                    "Ограничение количества", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Валидация: не более 300 за раз на весь заказ
            int totalCount = Cart.Items.Sum(i => i.Quantity);
            if (totalCount > 300)
            {
                MessageBox.Show($"Общее количество услуг в одном заказе ({totalCount} шт.) превышает лимит (максимум 300 шт. за раз).\n" +
                    "Пожалуйста, уменьшите количество позиций перед оформлением заказа.",
                    "Ограничение на заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (AppConnect.user == null)
            {
                MessageBox.Show("Для оформления заказа необходимо войти в систему под своей учетной записью.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.mainFrame.Navigate(new AuthorizationPage());
                return;
            }

            try
            {
                var newOrder = new order
                {
                    userid = AppConnect.user.id,
                    dateOfCart = DateTime.Now,
                    dateOfBuy = DateTime.Now
                };
                AppConnect.model0db.order.Add(newOrder);
                AppConnect.model0db.SaveChanges();

                foreach (var item in Cart.Items)
                {
                    for (int i = 0; i < item.Quantity; i++)
                    {
                        var po = new productOrder
                        {
                            productId = item.ProductId,
                            orderId = newOrder.id,
                            statusId = 1, // 1 = Новый
                            unitId = 1    // 1 = шт.
                        };
                        AppConnect.model0db.productOrder.Add(po);
                    }
                }

                AppConnect.model0db.SaveChanges();
                Cart.Clear();

                MessageBox.Show($"Заказ №{newOrder.id} успешно сформирован и передан в работу!",
                    "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);

                AppFrame.mainFrame.Navigate(new ProductsPage());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения заказа в базе данных: " + ex.Message,
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OrderHistory_Click(object sender, RoutedEventArgs e)
        {
            if (AppConnect.user == null)
            {
                MessageBox.Show("Для просмотра истории заказов необходимо войти в систему под своей учетной записью.",
                    "Требуется авторизация", MessageBoxButton.OK, MessageBoxImage.Information);
                AppFrame.mainFrame.Navigate(new AuthorizationPage());
                return;
            }

            AppFrame.mainFrame.Navigate(new UserOrdersPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new ProductsPage());
        }
    }
}
