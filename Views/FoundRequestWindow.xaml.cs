using System.Windows;
using LostAndFoundAgency.Models;
using LostAndFoundAgency.ViewModels;

namespace LostAndFoundAgency.Views
{
    public partial class FoundRequestWindow : Window
    {
        public FoundRequestWindow(Person currentClient)
        {
            InitializeComponent();
            var viewModel = new FoundRequestViewModel(currentClient);

            viewModel.OnSuccess = () =>
            {
                MessageBox.Show("Знахідку успішно зареєстровано! Дякуємо.", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            };
            DataContext = viewModel;
        }
    }
}