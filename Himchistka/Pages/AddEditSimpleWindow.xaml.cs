using System;
using System.Windows;

namespace Himchistka.Pages
{
    /// <summary>
    /// Логика взаимодействия для AddEditSimpleWindow.xaml
    /// </summary>
    public partial class AddEditSimpleWindow : Window
    {
        public string ResultValue { get; private set; }

        public AddEditSimpleWindow(string title, string fieldLabel, string initialValue = "")
        {
            InitializeComponent();
            Title = title;
            tbTitle.Text = fieldLabel;
            tbValue.Text = initialValue ?? "";
            tbValue.Focus();
            tbValue.SelectAll();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string val = tbValue.Text.Trim();
            if (string.IsNullOrWhiteSpace(val))
            {
                MessageBox.Show("Пожалуйста, заполните поле.", "Предупреждение",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ResultValue = val;
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
