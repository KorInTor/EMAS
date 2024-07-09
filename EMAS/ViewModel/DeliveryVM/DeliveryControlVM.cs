using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace EMAS.ViewModel.DeliveryVM
{
    public partial class DeliveryControlVM : ObservableObject
    {
        public event Action<SentEvent> DeliveryConfirmationRequested;

        [ObservableProperty]
        private ObservableCollection<SentEvent> _filteredDeliveries;

        [ObservableProperty]
        private SentEvent _selectedDelivery;

        [ObservableProperty]
        private List<SentEvent> _incomingDeliveries;

        [ObservableProperty]
        private List<SentEvent> _outgoingDeliveries;

        [ObservableProperty]
        private bool _isIncomingSelected = true;

        [ObservableProperty]
        private SentEvent _desiredDelivery = new(); //Treat DateTime.MinValue as not setted for Filtration.

        [ObservableProperty]
        private List<IStorableObject> _packageList = [];

        [ObservableProperty]
        private IStorableObject _desiredObject;

        private bool _canUserChangeDelivery = false;

        public IWindowsDialogueService DialogueService { get; set; } = new WindowsDialogueService();

        public RelayCommand ConfirmDeliveryCommand { get; set; }
        public RelayCommand ClearFiltersCommand { get; set; }

        private void ConfirmDelivery()
        {
            DeliveryConfirmationRequested?.Invoke(SelectedDelivery);
        }

        public DeliveryControlVM()
        {
            //DesiredDelivery.PropertyChanged += FilterDeliveries;

            ConfirmDeliveryCommand = new RelayCommand(ConfirmDelivery, CanConfirmDelivery);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
        }

        private void ClearFilters()
        {
            //DesiredDelivery.PropertyChanged -= FilterDeliveries;

            DesiredDelivery = new();

            //DesiredDelivery.PropertyChanged += FilterDeliveries;
        }

        private bool CanConfirmDelivery()
        {
            return IsIncomingSelected && SelectedDelivery is not null && _canUserChangeDelivery;
        }

        private void FilterDeliveries(object? sender, PropertyChangedEventArgs e) //Too bloated. Read SOLID and Refactor.
        {
            List<SentEvent> source = IsIncomingSelected ? IncomingDeliveries : OutgoingDeliveries;

            List<SentEvent> filteredList = [];

            if (source.Count == 0)
            {
                FilteredDeliveries = new ObservableCollection<SentEvent>(filteredList);
                return;
            }

            //TODO: Add Actual Filter Logic;

            FilteredDeliveries = new ObservableCollection<SentEvent>(source);
        }

        partial void OnSelectedDeliveryChanged(SentEvent value)
        {
            ConfirmDeliveryCommand.NotifyCanExecuteChanged();
            if (SelectedDelivery != null)
                PackageList = SelectedDelivery.ObjectsInEvent;
        }

        partial void OnIsIncomingSelectedChanged(bool value)
        {
            FilterDeliveries(this, new PropertyChangedEventArgs(nameof(IsIncomingSelected)));
            ConfirmDeliveryCommand.NotifyCanExecuteChanged();
        }

        public void ChagneSourceList(List<SentEvent> incomingDeliviries, List<SentEvent> outgoingDeliviries, bool canChangeDelivery = false)
        {
            IncomingDeliveries = incomingDeliviries;
            OutgoingDeliveries = outgoingDeliviries;

            _canUserChangeDelivery = canChangeDelivery;

            FilterDeliveries(this, new PropertyChangedEventArgs(nameof(IsIncomingSelected)));
        }
    }
}
