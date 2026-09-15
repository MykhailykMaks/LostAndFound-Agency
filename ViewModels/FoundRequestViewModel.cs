using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using LostAndFoundAgency.Commands;
using LostAndFoundAgency.Models;
using LostAndFoundAgency.Services;

namespace LostAndFoundAgency.ViewModels
{
    public class FoundRequestViewModel : ViewModelBase
    {
        private readonly IClientService _clientService;
        private readonly Person _currentClient;

        public string ItemName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime SelectedDate { get; set; } = DateTime.Now;
        public string StorageLocation { get; set; } = string.Empty;
        public int SelectedCategoryId { get; set; }
        public int SelectedLocationId { get; set; }

        public ObservableCollection<Category> Categories { get; } = new ObservableCollection<Category>();
        public ObservableCollection<Location> Locations { get; } = new ObservableCollection<Location>();

        public ICommand SaveCommand { get; }
        public Action? OnSuccess { get; set; }

        public FoundRequestViewModel(Person currentClient)
        {
            _currentClient = currentClient;
            _clientService = new ClientService();
            SaveCommand = new RelayCommand(ExecuteSave);

            LoadDictionaries();
        }

        private void LoadDictionaries()
        {
            foreach (var category in _clientService.GetCategories())
            {
                Categories.Add(category);
            }

            foreach (var location in _clientService.GetLocations())
            {
                Locations.Add(location);
            }

            SelectedCategoryId = Categories.FirstOrDefault()?.CategoryId ?? 0;
            SelectedLocationId = Locations.FirstOrDefault()?.LocationId ?? 0;
        }

        private void ExecuteSave(object? obj)
        {
            if (string.IsNullOrWhiteSpace(ItemName)) return;

            var item = new FoundItem
            {
                FinderId = _currentClient.PersonId,
                CategoryId = SelectedCategoryId,
                LocationId = SelectedLocationId,
                ItemName = ItemName,
                Description = Description,
                DateFound = SelectedDate,
                StorageLocation = string.IsNullOrWhiteSpace(StorageLocation) ? "У клієнта" : StorageLocation,
                Status = ItemStatuses.Pending
            };

            _clientService.AddFoundItem(item);
            OnSuccess?.Invoke();
        }
    }
}
