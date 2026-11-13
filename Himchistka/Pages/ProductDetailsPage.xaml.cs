using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    public partial class ProductDetailsPage : Page
    {
        private product _product;

        public ProductDetailsPage(product p)
        {
            InitializeComponent();
            _product = p;
            DataContext = _product;

            bool canEdit = AppConnect.user != null && (AppConnect.user.roleId == 1 || AppConnect.user.roleId == 2);
            btnEditStaff.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;
        }

        private void EditStaff_Click(object sender, RoutedEventArgs e)
        {
            var win = new AddEditProductWindow(_product);
            win.Owner = Application.Current.MainWindow;
            if (win.ShowDialog() == true)
            {
                DataContext = null;
                DataContext = _product;
            }
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем ввод только цифр
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void AddToCart_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(tbQty.Text.Trim(), out int qty) || qty <= 0)
            {
                MessageBox.Show("Пожалуйста, укажите корректное положительное количество от 1 до 300.",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (qty > 300)
            {
                MessageBox.Show("Нельзя заказать больше 300 единиц услуги за один раз.",
                    "Ограничение количества", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var existing = Cart.Items.FirstOrDefault(i => i.ProductId == _product.id);
            int currentQty = existing != null ? existing.Quantity : 0;
            if (currentQty + qty > 300)
            {
                MessageBox.Show($"Нельзя добавить {qty} шт. В корзине уже находится {currentQty} шт. данной услуги.\n" +
                    "Максимально допустимое количество одной услуги — 300 шт.",
                    "Ограничение количества", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int currentTotal = Cart.Items.Sum(i => i.Quantity);
            if (currentTotal + qty > 300)
            {
                MessageBox.Show($"Общее количество услуг в одном заказе не может превышать 300 шт.\n" +
                    $"В корзине уже: {currentTotal} шт., попытка добавить: {qty} шт.",
                    "Ограничение на заказ", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (existing != null)
            {
                existing.Quantity += qty;
            }
            else
            {
                Cart.Items.Add(new CartItem
                {
                    ProductId = _product.id,
                    ProductName = _product.name,
                    CategoryName = _product.CategoryName,
                    MaterialName = _product.MaterialName,
                    Quantity = qty,
                    PricePerUnit = _product.price,
                    Discount = _product.discount,
                    FinalPrice = _product.FinalPrice
                });
            }

            MessageBox.Show($"Услуга «{_product.name}» (x{qty}) добавлена в корзину!",
                "Корзина", MessageBoxButton.OK, MessageBoxImage.Information);

            AppFrame.mainFrame.Navigate(new ProductsPage());
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AppFrame.mainFrame.Navigate(new ProductsPage());
        }
    }
}
