using EMAS.Model;
using EMAS.Service.Command;
using EMAS.Service.Connection;
using EMAS.Service.Events;
using System.Collections.ObjectModel;
using System.ComponentModel;
using static EMAS.Service.Events.CommandEvents;

namespace EMAS.ViewModel
{
    public class EquipmentVM : INotifyPropertyChanged
    {
        private List<Equipment> _sourceEquipmentList;

        private ObservableCollection<Equipment> _filteredEquipmnetList;

        private string _nameFilter;

        private string _descriptionFilter;

        private string _typeFilter;

        private string _idFilter;

        private string _unitsFilter;

        private string _limitFilter;

        private string _accuracyClassFilter;

        private string _manufacturerFilter;

        private string _inventoryNumberFilter;

        private string _serialNumberFilter;

        private string _statusFilter;

        private RelayCommand _clearFiltersCommand;
        private RelayCommand _openHistoryWindowCommand;

        public event PropertyChangedEventHandler? PropertyChanged;

        public RelayCommand ClearFiltersCommand => _clearFiltersCommand ??= new RelayCommand(param => ClearFilters());

        public RelayCommand OpenHistoryWindowCommand => _openHistoryWindowCommand ??= new RelayCommand(OpenHistoryWindow);

        public List<string> GetAllTypes
        {
            get
            {
                return SourceEquipmentList.Select(eq => eq.Type).Distinct().ToList();
            }
        }

        public List<string> GetAllUnits
        {
            get
            {
                return SourceEquipmentList.Select(eq => eq.Units).Distinct().ToList();
            }
        }

        public List<string> GetAllLimits
        {
            get
            {
                return SourceEquipmentList.Select(eq => eq.Limit).Distinct().ToList();
            }
        }

        public List<string> GetAllAccuracyClasses
        {
            get
            {
                return SourceEquipmentList.Select(eq => eq.AccuracyClass).Distinct().ToList();
            }
        }

        public List<string> GetAllStatuses
        {
            get
            {
                return SourceEquipmentList.Select(eq => eq.Status).Distinct().ToList();
            }
        }

        public string NameFilter
        {
            get
            {
                return _nameFilter;
            }
            set
            {
                if (_nameFilter == value) 
                    return;
                _nameFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NameFilter)));
                FilterEquipment();
            }
        }

        public string DescriptionFilter
        {
            get
            {
                return _descriptionFilter;
            }
            set
            {
                if (_descriptionFilter == value) 
                    return;
                _descriptionFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DescriptionFilter)));
                FilterEquipment();
            }
        }

        public string TypeFilter
        {
            get
            {
                return _typeFilter;
            }
            set
            {
                if (_typeFilter == value) 
                    return;
                _typeFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TypeFilter)));
                FilterEquipment();
            }
        }

        public string IdFilter
        {
            get
            {
                return _idFilter;
            }
            set
            {
                if (_idFilter == value) 
                    return;
                _idFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IdFilter)));
                FilterEquipment();
            }
        }

        public string UnitsFilter
        {
            get
            {
                return _unitsFilter;
            }
            set
            {
                if (_unitsFilter == value) 
                    return;
                _unitsFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(UnitsFilter)));
                FilterEquipment();
            }
        }

        public string LimitFilter
        {
            get
            {
                return _limitFilter;
            }
            set
            {
                if (_limitFilter == value) 
                    return;
                _limitFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LimitFilter)));
                FilterEquipment();
            }
        }

        public string AccuracyClassFilter
        {
            get
            {
                return _accuracyClassFilter;
            }
            set
            {
                if (_accuracyClassFilter == value) 
                    return;
                _accuracyClassFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(AccuracyClassFilter)));
                FilterEquipment();
            }
        }

        public string ManufacturerFilter
        {
            get
            {
                return _manufacturerFilter;
            }
            set
            {
                if (_manufacturerFilter == value) 
                    return;
                _manufacturerFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ManufacturerFilter)));
                FilterEquipment();
            }
        }

        public string InventoryNumberFilter
        {
            get
            {
                return _inventoryNumberFilter;
            }
            set
            {
                if (_inventoryNumberFilter == value) 
                    return;
                _inventoryNumberFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InventoryNumberFilter)));
                FilterEquipment();
            }
        }

        public string SerialNumberFilter
        {
            get
            {
                return _serialNumberFilter;
            }
            set
            {
                if (_serialNumberFilter == value) 
                    return;
                _serialNumberFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(SerialNumberFilter)));
                FilterEquipment();
            }
        }

        public string StatusFilter
        {
            get
            {
                return _statusFilter;
            }
            set
            {
                if (_statusFilter == value)
                    return;
                _statusFilter = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusFilter)));
                FilterEquipment();
            }
        }

        public List<Equipment> SourceEquipmentList
        {
            get
            {
                return _sourceEquipmentList;
            }
            set
            {
                _sourceEquipmentList = value;
                FilteredEquipmnetList = new ObservableCollection<Equipment>(value);
            }
        }

        public ObservableCollection<Equipment> FilteredEquipmnetList
        {
            get
            {
                return _filteredEquipmnetList;
            }
            set
            {
                _filteredEquipmnetList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(FilteredEquipmnetList)));
            }
        }

        public EquipmentVM()
        {
            SourceEquipmentList = [];
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

        private void OpenHistoryWindow(object data)
        {
            throw new NotImplementedException();
        }
    }
}
