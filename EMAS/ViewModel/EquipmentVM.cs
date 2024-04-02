using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service.Connection;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace EMAS.ViewModel
{
    public partial class EquipmentVM : ObservableObject
    {
        public event Action<int> HistoryWindowRequested;
        public event Action<int> AdditionWindowRequested;

        [ObservableProperty]
        private List<Equipment> _equipmentSourceList = new();

        public int CurrentLocationId;

        [ObservableProperty]
        private RelayCommand _clearFiltersCommand;

        [ObservableProperty]
        private RelayCommand _openHistoryWindowCommand;

        [ObservableProperty]
        private RelayCommand _openAdditionWindow;

        [ObservableProperty]
        private Equipment _selectedEquipment;

        [ObservableProperty]
        private ObservableCollection<Equipment> _filteredEquipmentList;

        [ObservableProperty]
        private Equipment _desiredEquipment = new();

        public EquipmentVM()
        {
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            OpenHistoryWindowCommand = new RelayCommand(RequestHistoryWindow);
            OpenAdditionWindow = new RelayCommand(RequestAdditionWindow);

            DesiredEquipment.PropertyChanged += FilterEquipment;
        }

        partial void OnEquipmentSourceListChanged(List<Equipment> value)
        {
            FilterEquipment(this, new PropertyChangedEventArgs(nameof(EquipmentSourceList)));
        }

        private void FilterEquipment(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Поменялось свойство фильтрации:{e.PropertyName}");
            if (EquipmentSourceList.Count == 0)
            {
                FilteredEquipmentList = [];
                return;
            }

            var filteredList = EquipmentSourceList.Where(equipment =>
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

        private void RequestAdditionWindow()
        {
            AdditionWindowRequested?.Invoke(CurrentLocationId);
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
