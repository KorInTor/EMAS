using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    public partial class DeliveryControlVM : ObservableObject
    {
        public event Action<long> DeliveryConfirmationRequested;

        [ObservableProperty]
        private ObservableCollection<Delivery> _filteredDeliveries;

        [ObservableProperty]
        private Delivery _selectedDelivery;

        [ObservableProperty]
        private List<Delivery> _incomingDeliveries;

        [ObservableProperty]
        private List<Delivery> _outgoingDeliveries;

        [ObservableProperty]
        private bool _isIncomingSelected = true;

        [ObservableProperty]
        private Delivery _desiredDelivery = new(); //Treat DateTime.MinValue as not setted for Filtration.

        [ObservableProperty]
        private List<IStorableObject> _packageList = [];

        [ObservableProperty]
        private IStorableObject _desiredObject;

        private bool _canUserChangeDelivery = false;

        public RelayCommand ConfirmDeliveryCommand { get; set; }
        public RelayCommand ClearFiltersCommand { get; set; }

        private void ConfirmDelivery()
        {
            DeliveryConfirmationRequested?.Invoke(SelectedDelivery.Id);
        }

        public DeliveryControlVM(Type type)
        {
            try
            {
                _desiredObject = (IStorableObject)Activator.CreateInstance(type);
            }
            catch (InvalidCastException)
            {
                // Handle the case where 'type' cannot be cast to IStorableObject
                throw new NotSupportedException("Delivery Control VM supports only IStorableObjects derived classes");
            }

            DesiredDelivery.PropertyChanged += FilterDeliveries;

            ConfirmDeliveryCommand = new RelayCommand(ConfirmDelivery, CanConfirmDelivery);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
        }

        private void ClearFilters()
        {
            //No memory leaks on my duty @Danil.
            DesiredDelivery.PropertyChanged -= FilterDeliveries;

            DesiredDelivery = new();

            DesiredDelivery.PropertyChanged += FilterDeliveries;
        }

        private bool CanConfirmDelivery()
        {
            return IsIncomingSelected && SelectedDelivery is not null && _canUserChangeDelivery;
        }

        private void FilterDeliveries(object? sender, PropertyChangedEventArgs e) //Too bloated. Read SOLID and Refactor.
        {
            List<Delivery> source = IsIncomingSelected ? IncomingDeliveries : OutgoingDeliveries;

            List<Delivery> filteredList = [];

            if (source.Count == 0)
            {
                FilteredDeliveries = new ObservableCollection<Delivery> (filteredList);
                return;
            }
            
            //TODO: Add Actual Filter Logic;

            FilteredDeliveries = new ObservableCollection<Delivery>(filteredList);
        }

        partial void OnSelectedDeliveryChanged(Delivery value)
        {
            ConfirmDeliveryCommand.NotifyCanExecuteChanged();
            PackageList = SelectedDelivery.PackageList;
        }

        partial void OnIsIncomingSelectedChanged(bool value)
        {
            FilterDeliveries(this, new PropertyChangedEventArgs(nameof(IsIncomingSelected)));
            ConfirmDeliveryCommand.NotifyCanExecuteChanged();
        }

        public void ChagneSourceList(List<Delivery> incomingDeliviries, List<Delivery> outgoingDeliviries, bool canChangeDelivery = false)
        {
            IncomingDeliveries = incomingDeliviries;
            OutgoingDeliveries = outgoingDeliviries;

            _canUserChangeDelivery = canChangeDelivery;

            FilterDeliveries(this, new PropertyChangedEventArgs(nameof(IsIncomingSelected)));
        }
    }
}
