using System;
using System.Linq;
using System.Windows;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditProductOrderWindow.xaml
    /// </summary>
    public partial class AddEditProductOrderWindow : Window
    {
        private productOrder _item;
        private bool _isNew;
        private int? _presetOrderId;

        public productOrder CreatedItem => _item;

        public AddEditProductOrderWindow(productOrder po = null, int? presetOrderId = null)
        {
            InitializeComponent();
            _isNew = po == null;
            _item = po ?? new productOrder();
            _presetOrderId = presetOrderId;

            LoadLookups();

            if (_isNew)
            {
                Title = "Добавление позиции в заказ";
                tbTitle.Text = "Новая позиция в заказе";
                if (_presetOrderId.HasValue)
                {
                    cbOrder.SelectedValue = _presetOrderId.Value;
                    cbOrder.IsEnabled = false;
                }
            }
            else
            {
                Title = "Редактирование позиции заказа";
                tbTitle.Text = $"Позиция №{_item.id}";
                cbOrder.SelectedValue = _item.orderId;
                cbProduct.SelectedValue = _item.productId;
                cbStatus.SelectedValue = _item.statusId;
                cbUnit.SelectedValue = _item.unitId;
            }
        }

        private void LoadLookups()
        {
            try
            {
                var orders = AppConnect.model0db.order.ToList().Select(o => new
                {
                    id = o.id,
                    DisplayText = $"Заказ №{o.id} ({o.ClientFIO})"
                }).ToList();

                cbOrder.ItemsSource = orders;
                cbOrder.DisplayMemberPath = "DisplayText";
                cbOrder.SelectedValuePath = "id";

                cbProduct.ItemsSource = AppConnect.model0db.product.ToList();
                cbStatus.ItemsSource = AppConnect.model0db.status.ToList();
                cbUnit.ItemsSource = AppConnect.model0db.unit.ToList();

                if (_isNew)
                {
                    if (cbStatus.Items.Count > 0) cbStatus.SelectedIndex = 0;
                    if (cbUnit.Items.Count > 0) cbUnit.SelectedIndex = 0;
                    if (cbProduct.Items.Count > 0) cbProduct.SelectedIndex = 0;
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
            if (cbOrder.SelectedValue == null && !_presetOrderId.HasValue)
            {
                MessageBox.Show("Пожалуйста, выберите заказ.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbOrder.Focus();
                return;
            }

            if (cbProduct.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите услугу.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbProduct.Focus();
                return;
            }

            if (cbStatus.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите статус выполнения.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbStatus.Focus();
                return;
            }

            int orderId = _presetOrderId.HasValue ? _presetOrderId.Value : (int)cbOrder.SelectedValue;
            int productId = (int)cbProduct.SelectedValue;
            int statusId = (int)cbStatus.SelectedValue;
            int? unitId = cbUnit.SelectedValue != null ? (int?)cbUnit.SelectedValue : null;

            try
            {
                _item.orderId = orderId;
                _item.productId = productId;
                _item.statusId = statusId;
                _item.unitId = unitId;

                // Если привязано к существующему заказу в БД
                if (_isNew && orderId > 0)
                {
                    AppConnect.model0db.productOrder.Add(_item);
                    AppConnect.model0db.SaveChanges();
                }
                else if (!_isNew)
                {
                    AppConnect.model0db.SaveChanges();
                }
                else
                {
                    // Для нового несохраненного заказа заполняем навигационные свойства
                    _item.product = AppConnect.model0db.product.Find(productId);
                    _item.status = AppConnect.model0db.status.Find(statusId);
                    if (unitId.HasValue)
                        _item.unit = AppConnect.model0db.unit.Find(unitId.Value);
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения позиции:\n" + ex.Message, "Ошибка",
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
