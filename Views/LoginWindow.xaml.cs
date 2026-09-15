using System.Text.RegularExpressions;
using System.Windows;
using LostAndFoundAgency.ViewModels;
using LostAndFoundAgency.Data;

namespace LostAndFoundAgency.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DatabaseSeeder.Seed();
            var viewModel = new LoginViewModel();

            viewModel.OnLoginSuccess = (employee, client) =>
            {
                if (employee != null)
                {
                    var empWindow = new EmployeeMainWindow(employee);
                    empWindow.Show();
                }
                else if (client != null)
                {
                    var clientWindow = new ClientMainWindow(client);
                    clientWindow.Show();
                }
                this.Close();
            };

            DataContext = viewModel;
        }

        private void lnkRegister_Click(object sender, RoutedEventArgs e)
        {
            var registerWindow = new RegisterWindow();
            registerWindow.Show();
            this.Close();
        }

        private void btnTogglePassword_Click(object sender, RoutedEventArgs e)
        {
            if (btnTogglePassword.IsChecked == true)
            {
                txtPasswordVisible.Text = txtPasswordHidden.Password;
                txtPasswordVisible.Visibility = Visibility.Visible;
                txtPasswordHidden.Visibility = Visibility.Collapsed;
            }
            else
            {
                txtPasswordHidden.Password = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPasswordHidden.Visibility = Visibility.Visible;
            }
        }

        private void txtPasswordVisible_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (txtPasswordVisible.Visibility == Visibility.Visible)
            {
                txtPasswordHidden.Password = txtPasswordVisible.Text;
            }
        }

        private void LoginPass_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9]+$");
        }

        private void BlockSpace_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Space)
            {
                e.Handled = true;
            }
        }
    }
}