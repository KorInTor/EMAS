using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Model.Enum;
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
        [NotifyCanExecuteChangedFor(nameof(OpenAdditionWindow))]
        private bool _canAdd;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenEditWindow))]
        private bool _canEdit;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(OpenDeliveryWindow))]
        private bool _canSend;

        public RelayCommand OpenAdditionWindow { get; set; }
        public RelayCommand OpenHistoryWindowCommand { get; set; }
        public RelayCommand OpenEditWindow { get; set; }
        public RelayCommand OpenDeliveryWindow { get; set; }

        [ObservableProperty]
        public Dictionary<string, RelayCommand> _namedCommands = [];

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
            OpenAdditionWindow = new RelayCommand(RequestAdditionWindow, () => CanAdd);
            OpenEditWindow = new RelayCommand(RequestEditWindow, () => CanEdit);
            OpenDeliveryWindow = new RelayCommand(RequestDeliveryCreationWindow, () => CanSend);

            InitCommandDictionary();
            DesiredEquipment.PropertyChanged += FilterEquipment;
        }

        private void InitCommandDictionary()
        {
            NamedCommands.Add("Добавить", OpenAdditionWindow);
            NamedCommands.Add("Изменить", OpenEditWindow);
            NamedCommands.Add("Отправить", OpenDeliveryWindow);
        }

        private void RequestDeliveryCreationWindow()
        {
            throw new NotImplementedException();
        }

        private void RequestEditWindow()
        {
            throw new NotImplementedException();
        }

        partial void OnEquipmentSourceListChanged(List<Equipment> value)
        {
            FilterEquipment(this, new PropertyChangedEventArgs(nameof(EquipmentSourceList)));
        }

        public void ChangeCommandAccess(List<string> permissions)
        {
            CanAdd = false;
            CanEdit = false;
            CanSend = false;
            Dictionary<string,RelayCommand> newNamedCommandList = [];

            foreach(string permission in permissions)
            {
                if (Enum.TryParse(permission, out PermissionType permissionType))
                {
                    switch (permissionType)
                    {
                        case PermissionType.EquipmentAdd:
                        {
                            CanAdd = true;
                                newNamedCommandList.Add("Добавить", OpenAdditionWindow);
                            break;
                        }
                        case PermissionType.EquipmentEdit:
                        {
                            CanEdit = true;
                                newNamedCommandList.Add("Изменить", OpenEditWindow);
                            break;
                        }
                        case PermissionType.DeliveryAccess:
                        {
                            CanSend = true;
                                newNamedCommandList.Add("Отправить", OpenDeliveryWindow);
                            break;
                        }
                    }
                }
            }
            newNamedCommandList.Add("Просмотреть историю", OpenHistoryWindowCommand);
            NamedCommands = newNamedCommandList;
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
