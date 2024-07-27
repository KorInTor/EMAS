using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Model.Enum;
using EMAS.Service;
using EMAS.Service.Connection;
using EMAS.Windows;
using EMAS.Windows.Dialogue;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace EMAS.ViewModel
{
    public partial class EquipmentManagerVM : ObservableObject
    {
        public event Action<int> HistoryWindowRequested;
        public event Action<int> AdditionWindowRequested;
        public event Action<int,IStorableObject[]> DeliveryCreationRequested;
        public event Action<int, IStorableObject[]> ReservationCreationRequested;

        [ObservableProperty]
        private List<Equipment> _equipmentSourceList = new();

        public List<IStorableObject> SelectedEquipmentList
        {
            get
            {
                List<IStorableObject> selectedObjectsList = [];
                foreach (var selectableEquipment in FilteredEquipmentList)
                {
                    if (selectableEquipment.IsSelected)
                        selectedObjectsList.Add(selectableEquipment.Object);
                }
                return selectedObjectsList;
            }
        }

        private List<SelectableObject<Equipment>> FiltrationSource { get; set; } = new();

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
        [NotifyCanExecuteChangedFor(nameof(CreateDeliveryCommand))]
        private bool _canSend;

        public RelayCommand OpenAdditionWindow { get; set; }
        public RelayCommand OpenHistoryWindowCommand { get; set; }
        public RelayCommand OpenEditWindow { get; set; }
        public RelayCommand CreateDeliveryCommand { get; set; }
        public RelayCommand CreateReservationCommand { get; set; }

        [ObservableProperty]
        public Dictionary<string, RelayCommand> _namedCommands = [];

        [ObservableProperty]
        private SelectableObject<Equipment> _selectedEquipment;

        [ObservableProperty]
        private ObservableCollection<SelectableObject<Equipment>> _filteredEquipmentList;

        [ObservableProperty]
        private Equipment _desiredEquipment = new();

        public static IWindowsDialogueService DialogueService { get; private set; }
        public EquipmentManagerVM()
        {
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            OpenHistoryWindowCommand = new RelayCommand(RequestHistoryWindow);
            OpenAdditionWindow = new RelayCommand(RequestAdditionWindow, () => CanAdd);
            OpenEditWindow = new RelayCommand(RequestEditWindow, () => CanEdit);
            CreateDeliveryCommand = new RelayCommand(RequestDeliveryCreation, () => CanSend);
            CreateReservationCommand = new RelayCommand(RequestReservationCreation, () => CanEdit);

            InitCommandDictionary();
            DesiredEquipment.PropertyChanged += FilterEquipment;

            DialogueService = new WindowsDialogueService();
        }

        private void RequestReservationCreation()
        {
            ReservationCreationRequested?.Invoke(CurrentLocationId, [.. SelectedEquipmentList]);
        }

        private void InitCommandDictionary()
        {
            NamedCommands.Add("Добавить", OpenAdditionWindow);
            NamedCommands.Add("Изменить", OpenEditWindow);
            NamedCommands.Add("Отправить", CreateDeliveryCommand);
        }

        private void RequestDeliveryCreation()
        {
            DeliveryCreationRequested?.Invoke(CurrentLocationId, [.. SelectedEquipmentList]);
        }

        private void RequestEditWindow()
        {
            throw new NotImplementedException();
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
                                newNamedCommandList.Add("Зарезервировать", CreateReservationCommand);
                            break;
                        }
                        case PermissionType.DeliveryAccess:
                        {
                            CanSend = true;
                                newNamedCommandList.Add("Отправить", CreateDeliveryCommand);
                            break;
                        }
                    }
                }
            }
            newNamedCommandList.Add("Просмотреть историю", OpenHistoryWindowCommand);
            NamedCommands = newNamedCommandList;
        }

        public void ChangeSourceList(List<Equipment> newSource)
        {
            EquipmentSourceList = newSource;
            FiltrationSource.Clear();
            foreach (var equipment in EquipmentSourceList)
            {
                FiltrationSource.Add(new(equipment));
            }
            FilterEquipment(this, new PropertyChangedEventArgs(nameof(EquipmentSourceList)));
        }

        private void FilterEquipment(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Поменялось свойство фильтрации:{e.PropertyName}");
            if (FiltrationSource.Count == 0)
            {
                FilteredEquipmentList = [];
                return;
            }

            var filteredList = FiltrationSource.Where(selectableObject =>
            {
                if (selectableObject.Object is not Equipment equipment) return false;

                return (string.IsNullOrEmpty(DesiredEquipment.Name) || equipment.Name.Contains(DesiredEquipment.Name)) &&
                    (string.IsNullOrEmpty(DesiredEquipment.Description) || equipment.Description.Contains(DesiredEquipment.Description)) &&
                    (string.IsNullOrEmpty(DesiredEquipment.Type) || equipment.Type.Contains(DesiredEquipment.Type)) &&
                    (DesiredEquipment.Id == 0 || equipment.Id == DesiredEquipment.Id) &&
                    (string.IsNullOrEmpty(DesiredEquipment.Units) || equipment.Units.Contains(DesiredEquipment.Units)) &&
                    (string.IsNullOrEmpty(DesiredEquipment.Limit) || equipment.Limit.Contains(DesiredEquipment.Limit)) &&
                    (string.IsNullOrEmpty(DesiredEquipment.AccuracyClass) || equipment.AccuracyClass.Contains(DesiredEquipment.AccuracyClass)) &&
                    (string.IsNullOrEmpty(DesiredEquipment.Manufacturer) || equipment.Manufacturer.Contains(DesiredEquipment.Manufacturer)) &&
                    (string.IsNullOrEmpty(DesiredEquipment.RegistrationNumber) || equipment.RegistrationNumber.Contains(DesiredEquipment.RegistrationNumber)) &&
                    (string.IsNullOrEmpty(DesiredEquipment.FactoryNumber) || equipment.FactoryNumber.Contains(DesiredEquipment.FactoryNumber)) &&
                    (string.IsNullOrEmpty(DesiredEquipment.Status) || equipment.Status.Contains(DesiredEquipment.Status));
            }).ToList();

            FilteredEquipmentList = new (filteredList);
        }

        private void RequestAdditionWindow()
        {
            AdditionWindowRequested?.Invoke(CurrentLocationId);
            var dataContext = new EquipmentAdditionVM(CurrentLocationId);
            DialogueService.ShowWindow<EquipmentAdditionWindow>(dataContext);
        }

        private void RequestHistoryWindow()
        {
            HistoryVM historyVM = new HistoryVM();
            historyVM.StorableObjectEvents = LocationController.GetHistoryForStorableObject(SelectedEquipment.Object.Id);
            DialogueService.ShowWindow<HistoryWindow>(historyVM);
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
