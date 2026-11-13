using System;
using System.Linq;
using System.Windows;
using Himchistka.AppData;

namespace Himchistka.Pages
{
    public partial class AddEditUserWindow : Window
    {
        private user _user;
        private bool _isNew;

        public AddEditUserWindow(user u = null)
        {
            InitializeComponent();
            _isNew = u == null;
            _user = u ?? new user();

            LoadRoles();

            if (_isNew)
            {
                Title = "Добавление пользователя";
                tbTitle.Text = "Новый пользователь";
            }
            else
            {
                Title = "Редактирование пользователя";
                tbTitle.Text = $"Редактирование: {_user.FIO}";
                tbSurname.Text = _user.surname;
                tbName.Text = _user.name;
                tbPatronymic.Text = _user.patronymic;
                tbPhone.Text = _user.phoneNumber;
                tbEmail.Text = _user.email;
                tbPassword.Text = _user.password;
                cbRole.SelectedValue = _user.roleId;
            }
        }

        private void LoadRoles()
        {
            try
            {
                cbRole.ItemsSource = AppConnect.model0db.role.ToList();
                if (cbRole.Items.Count > 0 && cbRole.SelectedIndex < 0)
                    cbRole.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки ролей: " + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string surname = tbSurname.Text.Trim();
            string name = tbName.Text.Trim();
            string patronymic = tbPatronymic.Text.Trim();
            string phone = tbPhone.Text.Trim();
            string email = tbEmail.Text.Trim();
            string password = tbPassword.Text.Trim();

            if (string.IsNullOrWhiteSpace(surname) || string.IsNullOrWhiteSpace(name))
            {
                MessageBox.Show("Пожалуйста, укажите фамилию и имя.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                MessageBox.Show("Пожалуйста, укажите номер телефона.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbPhone.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                MessageBox.Show("Пожалуйста, укажите адрес электронной почты.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbEmail.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                MessageBox.Show("Пожалуйста, укажите пароль.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                tbPassword.Focus();
                return;
            }

            if (cbRole.SelectedValue == null)
            {
                MessageBox.Show("Пожалуйста, выберите роль пользователя.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                cbRole.Focus();
                return;
            }

            int roleId = (int)cbRole.SelectedValue;

            // Проверка уникальности email / телефона
            bool duplicate = AppConnect.model0db.user.Any(u =>
                u.id != _user.id && (u.email == email || u.phoneNumber == phone));

            if (duplicate)
            {
                MessageBox.Show("Пользователь с таким email или телефоном уже существует в системе.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                _user.surname = surname;
                _user.name = name;
                _user.patronymic = patronymic;
                _user.phoneNumber = phone;
                _user.email = email;
                _user.password = password;
                _user.roleId = roleId;

                if (_isNew)
                {
                    AppConnect.model0db.user.Add(_user);
                }

                AppConnect.model0db.SaveChanges();
                MessageBox.Show("Данные пользователя успешно сохранены!", "Успех",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения пользователя:\n" + ex.Message, "Ошибка",
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
