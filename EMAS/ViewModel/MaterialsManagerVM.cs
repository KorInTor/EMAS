using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service;
using Emas.View.Windows.Dialogue;
using Emas.View.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Diagnostics;
using EMAS.Model.Enum;
using DocumentFormat.OpenXml.Packaging;
using EMAS.View.Windows;

namespace EMAS.ViewModel
{
    public partial class MaterialsManagerVM : ObservableObject
    {
        public event Action<int> HistoryWindowRequested;
        public event Action<int> AdditionWindowRequested;
        public event Action<int, IStorableObject[]> DeliveryCreationRequested;
        public event Action<int, IStorableObject[]> ReservationCreationRequested;

        [ObservableProperty]
        private List<MaterialPiece> _materialsSourceList = new();

        public List<IStorableObject> SelectedMaterialsList
        {
            get
            {
                List<IStorableObject> selectedObjectsList = [];
                foreach (var selectableMaterials in FilteredMaterialsList)
                {
                    if (selectableMaterials.IsSelected)
                        selectedObjectsList.Add(selectableMaterials.Object);
                }
                return selectedObjectsList;
            }
        }

        private List<SelectableObject<MaterialPiece>> FiltrationSource { get; set; } = new();

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
        public RelayCommand OpenCommentsWindow { get; set; }

        [ObservableProperty]
        public Dictionary<string, RelayCommand> _namedCommands = [];

        [ObservableProperty]
        private SelectableObject<MaterialPiece> _selectedMaterials;

        [ObservableProperty]
        private ObservableCollection<SelectableObject<MaterialPiece>> _filteredMaterialsList;

        [ObservableProperty]
        private MaterialPiece _desiredMaterialPiece = new();

        public static IWindowsDialogueService DialogueService { get; private set; }


        public MaterialsManagerVM()
        {
            ClearFiltersCommand = new RelayCommand(ClearFilters);
            OpenHistoryWindowCommand = new RelayCommand(RequestHistoryWindow);
            OpenAdditionWindow = new RelayCommand(RequestAdditionWindow, () => CanAdd);
            OpenEditWindow = new RelayCommand(RequestEditWindow, () => CanEdit);
            CreateDeliveryCommand = new RelayCommand(RequestDeliveryCreation, () => CanSend);
            CreateReservationCommand = new RelayCommand(RequestReservationCreation, () => CanEdit);
            OpenCommentsWindow = new RelayCommand(RequestCommentsWindow, () => true);
            InitCommandDictionary();
            DesiredMaterialPiece.PropertyChanged += FilterMaterials;

            DialogueService = new WindowsDialogueService();
            //Debug.WriteLine($"HERE ARE ALL ANSWERS ----->>> {FilteredMaterialsList.Count}");
        }

        private void RequestReservationCreation()
        {
            ReservationCreationRequested?.Invoke(CurrentLocationId, [.. SelectedMaterialsList]);
        }

        private void InitCommandDictionary()
        {
            NamedCommands.Add("Добавить", OpenAdditionWindow);
            NamedCommands.Add("Изменить", OpenEditWindow);
            NamedCommands.Add("Отправить", CreateDeliveryCommand);
        }

        private void RequestDeliveryCreation()
        {
            DeliveryCreationRequested?.Invoke(CurrentLocationId, [.. SelectedMaterialsList]);
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
            Dictionary<string, RelayCommand> newNamedCommandList = [];

            foreach (string permission in permissions)
            {
                if (Enum.TryParse(permission, out PermissionType permissionType))
                {
                    switch (permissionType)
                    {
                        case PermissionType.MaterialsAdd:
                            {
                                CanAdd = true;
                                newNamedCommandList.Add("Добавить", OpenAdditionWindow);
                                break;
                            }
                        case PermissionType.MaterialsEdit:
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

        public void ChangeSourceList(List<MaterialPiece> newSource)
        {
            MaterialsSourceList = newSource;
            FiltrationSource.Clear();
            foreach (var equipment in MaterialsSourceList)
            {
                FiltrationSource.Add(new(equipment));
            }
            FilterMaterials(this, new PropertyChangedEventArgs(nameof(MaterialsSourceList)));
        }

        private void FilterMaterials(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Поменялось свойство фильтрации:{e.PropertyName}");
            if (FiltrationSource.Count == 0)
            {
                FilteredMaterialsList = [];
                return;
            }

            var filteredList = FiltrationSource.Where(selectableObject =>
            {
                if (selectableObject.Object is not MaterialPiece materialPiece) return false;

                return
                (string.IsNullOrEmpty(DesiredMaterialPiece.Name) || materialPiece.Name.Contains(DesiredMaterialPiece.Name)) &&
                (string.IsNullOrEmpty(DesiredMaterialPiece.Description) || materialPiece.Description.Contains(DesiredMaterialPiece.Description)) &&
                (string.IsNullOrEmpty(DesiredMaterialPiece.Type) || materialPiece.Type.Contains(DesiredMaterialPiece.Type)) &&
                (DesiredMaterialPiece.Id == 0 || materialPiece.Id == DesiredMaterialPiece.Id) &&
                (string.IsNullOrEmpty(DesiredMaterialPiece.Units) || materialPiece.Units.Contains(DesiredMaterialPiece.Units)) &&
                (DesiredMaterialPiece.Amount == 0 || materialPiece.Amount == DesiredMaterialPiece.Amount) &&
                (string.IsNullOrEmpty(DesiredMaterialPiece.StorageType) || materialPiece.Units.Contains(DesiredMaterialPiece.StorageType));
            }).ToList();

            FilteredMaterialsList = new(filteredList);
        }


        private void RequestAdditionWindow()
        {
            AdditionWindowRequested?.Invoke(CurrentLocationId);
            var dataContext = new MaterialsAdditionVM(CurrentLocationId);
            DialogueService.ShowWindow<MaterialsAdditionWindow>(dataContext);
        }

        private void RequestHistoryWindow()
        {
            HistoryVM historyVM = new HistoryVM();
            historyVM.StorableObjectEvents = LocationController.GetHistoryForStorableObject(SelectedMaterials.Object.Id);
            DialogueService.ShowWindow<HistoryWindow>(historyVM);
        }

        private void RequestCommentsWindow()
        {
            throw new NotImplementedException();
        }

        private void ClearFilters()
        {
            DesiredMaterialPiece.Name = string.Empty;
            DesiredMaterialPiece.Type = string.Empty;
            DesiredMaterialPiece.Description = string.Empty;
            DesiredMaterialPiece.StorageType = string.Empty;
            DesiredMaterialPiece.Units = string.Empty;
        }
    }
}
