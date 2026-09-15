using System.Windows;
using LostAndFoundAgency.Models;
using LostAndFoundAgency.ViewModels;

namespace LostAndFoundAgency.Views
{
    public partial class LostRequestWindow : Window
    {
        public LostRequestWindow(Person currentClient)
        {
            InitializeComponent();
            var viewModel = new LostRequestViewModel(currentClient);

            viewModel.OnSuccess = () =>
            {
                MessageBox.Show("Вашу заяву про втрату успішно зареєстровано!", "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                this.Close();
            };
            DataContext = viewModel;
        }
    }
}