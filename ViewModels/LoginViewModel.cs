using System;
using System.Windows.Input;
using System.Text.RegularExpressions;
using LostAndFoundAgency.Commands;
using LostAndFoundAgency.Services;
using LostAndFoundAgency.Models;

namespace LostAndFoundAgency.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;

        private string _login = string.Empty;
        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        private string _errorMessage = string.Empty;
        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }

        public Action<Employee?, Person?>? OnLoginSuccess { get; set; }

        public LoginViewModel()
        {
            _authService = new AuthService();
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object? parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            var password = passwordBox?.Password;

            if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Поля не можуть бути порожніми.";
                return;
            }
            string pattern = @"^[a-zA-Zа-яА-ЯіІїЇєЄ0-9_]+$";
            if (!Regex.IsMatch(Login, pattern) || !Regex.IsMatch(password, pattern))
            {
                ErrorMessage = "Логін та пароль можуть містити лише літери та цифри!";
                return;
            }
            var (employee, client) = _authService.Authenticate(Login, password);
            if (employee != null || client != null)
            {
                ErrorMessage = string.Empty;
                OnLoginSuccess?.Invoke(employee, client);
            }
            else
            {
                ErrorMessage = "Невірний логін або пароль!";
            }
        }
    }
}
