using System.Text.RegularExpressions;
using System.Windows;
using LostAndFoundAgency.ViewModels;

namespace LostAndFoundAgency.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
            var viewModel = new RegisterViewModel();

            viewModel.OnRegisterSuccess = () =>
            {
                MessageBox.Show("Реєстрація успішна! Тепер ви можете увійти.", "Вітаємо", MessageBoxButton.OK, MessageBoxImage.Information);
                var loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            };

            DataContext = viewModel;
        }

        private void lnkBack_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
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

        private void Name_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ\s\-]+$");
        }

        private void Phone_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, @"^[0-9]+$");
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