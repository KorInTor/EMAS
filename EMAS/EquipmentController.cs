using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service.Connection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace EMAS
{
    public partial class EquipmentController : ObservableObject
    {
        public event Action<int> HistoryWindowRequested;
        public event Action<int> AdditionWindowRequested;

        [ObservableProperty]
        private Location _currentLocation;

        [ObservableProperty]
        private List<Location> _locations = [];

        [ObservableProperty]
        private RelayCommand _clearFiltersCommand;

        [ObservableProperty]
        private RelayCommand _openHistoryWindowCommand;

        [ObservableProperty]
        private RelayCommand _openAdditionWindow;

        [ObservableProperty]
        private Equipment _selectedEquipment;

        private DataChangeChecker _monitor;

        [ObservableProperty]
        private ObservableCollection<Equipment> _filteredEquipmentList;

        [ObservableProperty]
        private Equipment _desiredEquipment = new();

        public EquipmentController()
        {
            Locations = DataBaseClient.GetLocationData();

            InitLocationsData();

            _monitor = new DataChangeChecker(Locations.Select(location => location.Id).ToList());
            _monitor.DataChanged += async (id) => UpdateEquipmentData(id);

            ClearFiltersCommand = new RelayCommand(ClearFilters);
            OpenHistoryWindowCommand = new RelayCommand(RequestHistoryWindow);
            OpenAdditionWindow = new RelayCommand(RequestAdditionWindow);

            DesiredEquipment.PropertyChanged += FilterEquipment;
        }

        private void FilterEquipment(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Поменялось свойство фильтрации:{e.PropertyName}");
            if (CurrentLocation.Equipments.Count == 0)
                return;

            var filteredList = CurrentLocation.Equipments.Where(equipment =>
                (string.IsNullOrEmpty(DesiredEquipment.Name) || equipment.Name.Contains(DesiredEquipment.Name)) &&
                (string.IsNullOrEmpty(DesiredEquipment.Description) || equipment.Description.Contains(DesiredEquipment.Description)) &&
                (string.IsNullOrEmpty(DesiredEquipment.Type) || equipment.Type.Contains(DesiredEquipment.Type)) &&
                (DesiredEquipment.Id == 0 || equipment.Id == DesiredEquipment.Id) &&
                (string.IsNullOrEmpty(DesiredEquipment.Units) || equipment.Units.Contains(DesiredEquipment.Units)) &&
                (string.IsNullOrEmpty(DesiredEquipment.Limit) || equipment.Limit.Contains(DesiredEquipment.Limit)) &&
                (string.IsNullOrEmpty(DesiredEquipment.AccuracyClass) || equipment.AccuracyClass.Contains(DesiredEquipment.AccuracyClass)) &&
                (string.IsNullOrEmpty(DesiredEquipment.Manufacturer) || equipment.Manufacturer.Contains(DesiredEquipment.Manufacturer)) &&
                (string.IsNullOrEmpty(DesiredEquipment.RegistrationNumber) || equipment.RegistrationNumber.Contains(DesiredEquipment.RegistrationNumber)) &&
                (string.IsNullOrEmpty(DesiredEquipment.FactoryNumber) || equipment.FactoryNumber.Contains(DesiredEquipment.FactoryNumber)) &&
                (string.IsNullOrEmpty(DesiredEquipment.Status) || equipment.Status.Contains(DesiredEquipment.Status))
             ).ToList();

            FilteredEquipmentList = new ObservableCollection<Equipment>(filteredList);
        }

        private async Task UpdateEquipmentData(int ID)
        {
            foreach (var location in Locations)
            {
                if (location.Id == ID)
                {
                    location.Equipments = DataBaseClient.GetEquipmentOnLocation(location.Id);
                }
            }
            FilteredEquipmentList = new ObservableCollection<Equipment>(CurrentLocation.Equipments);
        }

        private void InitLocationsData()
        {
            foreach (var location in Locations)
            {
                location.Equipments = DataBaseClient.GetEquipmentOnLocation(location.Id);
            }
        }

        partial void OnCurrentLocationChanged(Location value)
        {
            FilteredEquipmentList = new ObservableCollection<Equipment>(CurrentLocation.Equipments);
            _monitor.StopActiveListeners();
            _monitor.InitListener(value.Id);
        }

        private void RequestAdditionWindow()
        {
            AdditionWindowRequested?.Invoke(CurrentLocation.Id);
        }

        private void RequestHistoryWindow()
        {
            HistoryWindowRequested?.Invoke(SelectedEquipment.Id);
        }

        private void ClearFilters()
        {
            DesiredEquipment.Name = string.Empty;
            DesiredEquipment.Description = string.Empty;
            DesiredEquipment.Type = string.Empty;
            DesiredEquipment.Id = 0;
            DesiredEquipment.Units = string.Empty;
            DesiredEquipment.Limit = string.Empty;
            DesiredEquipment.AccuracyClass = string.Empty;
            DesiredEquipment.Manufacturer = string.Empty;
            DesiredEquipment.RegistrationNumber = string.Empty;
            DesiredEquipment.FactoryNumber = string.Empty;
            DesiredEquipment.Status = string.Empty;
        }
    }
}
