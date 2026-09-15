using System.Windows;
using LostAndFoundAgency.Models;
using LostAndFoundAgency.ViewModels;

namespace LostAndFoundAgency.Views
{
    /// <summary>
    /// Interaction logic for ClientMainWindow.xaml
    /// </summary>
    public partial class ClientMainWindow : Window
    {
        public ClientMainWindow(Person currentClient)
        {
            InitializeComponent();
            DataContext = new ClientMainViewModel(currentClient);
        }
        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }
    }
}
