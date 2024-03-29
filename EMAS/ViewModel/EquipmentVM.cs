using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service.Connection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    public partial class EquipmentVM : ObservableObject
    {
        public event Action<int> HistoryWindowRequested;
        public event Action<int> AdditionWindowRequested;

        public int CurrentLocationId { get; private set; }

        [ObservableProperty]
        private string _accuracyClassFilter;

        [ObservableProperty]
        private RelayCommand _clearFiltersCommand;

        [ObservableProperty]
        private RelayCommand _openHistoryWindowCommand;

        [ObservableProperty]
        private RelayCommand _openAdditionWindow;

        [ObservableProperty]
        private string _descriptionFilter;

        [ObservableProperty]
        private ObservableCollection<Equipment> _filteredEquipmnetList;

        [ObservableProperty]
        private string _idFilter;

        [ObservableProperty]
        private string _inventoryNumberFilter;

        [ObservableProperty]
        private string _limitFilter;

        [ObservableProperty]
        private string _manufacturerFilter;

        [ObservableProperty]
        private string _nameFilter;

        [ObservableProperty]
        private string _serialNumberFilter;

        [ObservableProperty]
        private List<Equipment> _sourceEquipmentList;

        [ObservableProperty]
        private string _statusFilter;

        [ObservableProperty]
        private string _typeFilter;

        [ObservableProperty]
        private string _unitsFilter;

        [ObservableProperty]
        private Equipment _selectedEquipment;

        public EquipmentVM()
        {
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            OpenHistoryWindowCommand = new RelayCommand(RequestHistoryWindow);
            OpenAdditionWindow = new RelayCommand(RequestAdditionWindow);
        }

        public EquipmentVM(int locationId)
        {
            CurrentLocationId = locationId;
            SourceEquipmentList = DataBaseClient.GetEquipmentOnLocation(locationId);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            OpenHistoryWindowCommand = new RelayCommand(RequestHistoryWindow);
            OpenAdditionWindow = new RelayCommand(RequestAdditionWindow);
        }

        private void RequestAdditionWindow()
        {
            AdditionWindowRequested?.Invoke(CurrentLocationId);
        }

        public void FilterEquipment()
        {
            if (SourceEquipmentList == null)
                return;

            var filteredList = SourceEquipmentList.Where(equipment =>
                (string.IsNullOrEmpty(NameFilter) || equipment.Name.Contains(NameFilter)) &&
                (string.IsNullOrEmpty(DescriptionFilter) || equipment.Description.Contains(DescriptionFilter)) &&
                (string.IsNullOrEmpty(TypeFilter) || equipment.Type.Contains(TypeFilter)) &&
                (string.IsNullOrEmpty(IdFilter) || equipment.Id == int.Parse(IdFilter)) &&
                (string.IsNullOrEmpty(UnitsFilter) || equipment.Units.Contains(UnitsFilter)) &&
                (string.IsNullOrEmpty(LimitFilter) || equipment.Limit.Contains(LimitFilter)) &&
                (string.IsNullOrEmpty(AccuracyClassFilter) || equipment.AccuracyClass.Contains(AccuracyClassFilter)) &&
                (string.IsNullOrEmpty(ManufacturerFilter) || equipment.Manufacturer.Contains(ManufacturerFilter)) &&
                (string.IsNullOrEmpty(InventoryNumberFilter) || equipment.RegistrationNumber.Contains(InventoryNumberFilter)) &&
                (string.IsNullOrEmpty(SerialNumberFilter) || equipment.FactoryNumber.Contains(SerialNumberFilter))
            ).ToList();

            FilteredEquipmnetList = new ObservableCollection<Equipment>(filteredList);
        }

        private void ClearFilters()
        {
            NameFilter = string.Empty;
            DescriptionFilter = string.Empty;
            TypeFilter = string.Empty;
            IdFilter = string.Empty;
            UnitsFilter = string.Empty;
            LimitFilter = string.Empty;
            AccuracyClassFilter = string.Empty;
            ManufacturerFilter = string.Empty;
            InventoryNumberFilter = string.Empty;
            SerialNumberFilter = string.Empty;
        }

        partial void OnSourceEquipmentListChanged(List<Equipment> value)
        {
            FilteredEquipmnetList = new ObservableCollection<Equipment>(value);
        }

        private void RequestHistoryWindow()
        {
            HistoryWindowRequested?.Invoke(SelectedEquipment.Id);
        }
    }
}
