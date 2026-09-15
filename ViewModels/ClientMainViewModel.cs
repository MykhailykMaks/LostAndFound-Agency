using System.Collections.ObjectModel;
using System.Windows.Input;
using LostAndFoundAgency.Commands;
using LostAndFoundAgency.Models;
using LostAndFoundAgency.Services;

namespace LostAndFoundAgency.ViewModels
{
    public class ClientMainViewModel : ViewModelBase
    {
        private readonly IClientService _clientService;
        public Person CurrentClient { get; }

        private ObservableCollection<LostRequest> _myLostRequests = new ObservableCollection<LostRequest>();
        public ObservableCollection<LostRequest> MyLostRequests
        {
            get => _myLostRequests;
            set { _myLostRequests = value; OnPropertyChanged(); }
        }

        private ObservableCollection<FoundItem> _myFoundItems = new ObservableCollection<FoundItem>();
        public ObservableCollection<FoundItem> MyFoundItems
        {
            get => _myFoundItems;
            set { _myFoundItems = value; OnPropertyChanged(); }
        }
        public ICommand ReportLostCommand { get; }
        public ICommand ReportFoundCommand { get; }

        public ClientMainViewModel(Person client)
        {
            CurrentClient = client;
            _clientService = new ClientService();

            ReportLostCommand = new RelayCommand(OpenReportLost);
            ReportFoundCommand = new RelayCommand(OpenReportFound);

            LoadData();
        }

        public void LoadData()
        {
            var lost = _clientService.GetMyLostRequests(CurrentClient.PersonId);
            MyLostRequests = new ObservableCollection<LostRequest>(lost);

            var found = _clientService.GetMyFoundItems(CurrentClient.PersonId);
            MyFoundItems = new ObservableCollection<FoundItem>(found);
        }

        private void OpenReportLost(object? obj)
        {
            // Відкриваємо нове вікно втрати
            var window = new Views.LostRequestWindow(CurrentClient);
            window.ShowDialog();
            LoadData(); // Оновлюємо таблицю після закриття
        }

        private void OpenReportFound(object? obj)
        {
            // Відкриваємо нове вікно знахідки
            var window = new Views.FoundRequestWindow(CurrentClient);
            window.ShowDialog();
            LoadData(); // Оновлюємо таблицю
        }
    }
}
