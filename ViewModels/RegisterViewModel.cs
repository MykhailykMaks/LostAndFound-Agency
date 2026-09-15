using System;
using System.Windows.Input;
using System.Text.RegularExpressions;
using LostAndFoundAgency.Commands;
using LostAndFoundAgency.Services;

namespace LostAndFoundAgency.ViewModels
{
    public class RegisterViewModel : ViewModelBase
    {
        private readonly IAuthService _authService;

        private string _fullName = string.Empty;
        private string _phone = string.Empty;
        private string _login = string.Empty;
        private string _errorMessage = string.Empty;

        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }

        public string Phone
        {
            get => _phone;
            set { _phone = value; OnPropertyChanged(); }
        }

        public string Login
        {
            get => _login;
            set { _login = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public Action? OnRegisterSuccess { get; set; }

        public RegisterViewModel()
        {
            _authService = new AuthService();
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }

        private void ExecuteRegister(object? parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            var password = passwordBox?.Password;

            // 1. Порожні поля
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Phone) || 
                string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(password))
            {
                ErrorMessage = "Помилка: Заповніть усі поля!";
                return;
            }

            // 2. ПІБ: Тільки літери (українські/англійські), пробіли та дефіс
            if (!Regex.IsMatch(FullName, @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ\s\-]+$"))
            {
                ErrorMessage = "Помилка ПІБ: Дозволені лише літери!";
                return;
            }

            // 3. Телефон: Тільки цифри і довжина від 10 до 12 символів (напр. 0991234567)
            if (!Regex.IsMatch(Phone, @"^\d{10,12}$"))
            {
                ErrorMessage = "Помилка телефону: Введіть від 10 до 12 цифр (без +, - або пробілів)!";
                return;
            }

            // 4. Логін: Тільки літери та цифри, без спецсимволів (!@#$ тощо)
            if (!Regex.IsMatch(Login, @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9]+$"))
            {
                ErrorMessage = "Помилка логіну: Дозволені лише літери та цифри!";
                return;
            }

            // 5. Пароль: Тільки літери та цифри
            if (!Regex.IsMatch(password, @"^[a-zA-Zа-яА-ЯіІїЇєЄґҐ0-9]+$"))
            {
                ErrorMessage = "Помилка пароля: Дозволені лише літери та цифри!";
                return;
            }

            // 6. Спроба зберегти в базу
            bool isSuccess = _authService.RegisterClient(FullName, Login, password, Phone);

            if (isSuccess)
            {
                ErrorMessage = string.Empty;
                OnRegisterSuccess?.Invoke();
            }
            else
            {
                ErrorMessage = "Користувач з таким логіном вже існує в системі!";
            }
        }
    }
}
