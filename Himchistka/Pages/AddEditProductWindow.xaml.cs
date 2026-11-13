using System;
using System.IO;
using System.Linq;
using System.Windows;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    public partial class AddEditProductWindow : Window
    {
        private product _product;
        private bool _isNew;

        public AddEditProductWindow(product p = null)
        {
            InitializeComponent();
            _isNew = p == null;
            _product = p ?? new product();

            LoadLookups();

            if (_isNew)
            {
                Title = "Добавление услуги";
                tbTitle.Text = "Новая услуга";
                cbImage.Text = "picture.png";
            }
            else
            {
                Title = "Редактирование услуги";
                tbTitle.Text = $"Редактирование: {_product.name}";
                tbName.Text = _product.name;
                cbCategory.SelectedValue = _product.categoryId;
                cbMaterial.SelectedValue = _product.materialId;
                tbPrice.Text = _product.price.ToString();
                tbDiscount.Text = (_product.discount ?? 0).ToString();
                tbAmount.Text = _product.amount;
                cbImage.Text = _product.image ?? "picture.png";
            }
        }

        private void LoadLookups()
        {
            try
            {
                cbCategory.ItemsSource = AppConnect.model0db.category.ToList();
                cbMaterial.ItemsSource = AppConnect.model0db.material.ToList();

                // Список изображений
                string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Images");
                if (Directory.Exists(imagesDir))
                {
                    var files = Directory.GetFiles(imagesDir, "*.png")
                        .Select(Path.GetFileName)
                        .ToList();
                    cbImage.ItemsSource = files;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string name = tbName.Text.Trim();
            if (string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Пожалуйста, укажите наименование услуги.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbName.Focus();
                return;
            }

            if (name.Length < 3 || name.Length > 150)
            {
                MessageBox.Show("Наименование услуги должно содержать от 3 до 150 символов.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbName.Focus();
                return;
            }

            if (cbCategory.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите категорию изделия.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbCategory.Focus();
                return;
            }

            if (cbMaterial.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите тип материала.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbMaterial.Focus();
                return;
            }

            if (!int.TryParse(tbPrice.Text.Trim(), out int price) || price <= 0 || price > 1000000)
            {
                MessageBox.Show("Пожалуйста, введите корректную цену (целое число от 1 до 1 000 000 руб.).", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbPrice.Focus();
                return;
            }

            int discount = 0;
            if (!string.IsNullOrWhiteSpace(tbDiscount.Text))
            {
                if (!int.TryParse(tbDiscount.Text.Trim(), out discount) || discount < 0 || discount > 100)
                {
                    MessageBox.Show("Скидка должна быть целым числом от 0 до 100.", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    tbDiscount.Focus();
                    return;
                }
            }

            try
            {
                _product.name = name;
                _product.categoryId = (int)cbCategory.SelectedValue;
                _product.materialId = (int)cbMaterial.SelectedValue;
                _product.price = price;
                _product.discount = discount > 0 ? (int?)discount : null;
                _product.amount = string.IsNullOrWhiteSpace(tbAmount.Text) ? "1 шт." : tbAmount.Text.Trim();
                _product.image = string.IsNullOrWhiteSpace(cbImage.Text) ? "picture.png" : cbImage.Text.Trim();

                if (_isNew)
                {
                    AppConnect.model0db.product.Add(_product);
                }

                AppConnect.model0db.SaveChanges();
                MessageBox.Show("Услуга успешно сохранена!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения в базу данных:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
