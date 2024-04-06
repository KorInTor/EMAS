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
        private DateTime? _selectedDispatchDate; //Need this because DatePicker returns null if date isnt selected;

        [ObservableProperty]
        private bool _isIncomingSelected = true;

        [ObservableProperty]
        private Delivery _desiredDelivery = new(); //Treat DateTime.MinValue as not setted for Filtration.

        private bool _canUserChangeDelivery = false;

        public RelayCommand ConfirmDeliveryCommand { get; set; }
        public RelayCommand ClearFiltersCommand { get; set; }

        private void ConfirmDelivery()
        {
            DeliveryConfirmationRequested?.Invoke(SelectedDelivery.Id);
        }

        public DeliveryControlVM()
        {
            DesiredDelivery.PropertyChanged += FilterDeliveries;
            DesiredDelivery.Equipment.PropertyChanged += FilterDeliveries;

            ConfirmDeliveryCommand = new RelayCommand(ConfirmDelivery, CanConfirmDelivery);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
        }

        private void ClearFilters()
        {
            //No memory leaks on my duty @Danil.
            DesiredDelivery.PropertyChanged -= FilterDeliveries;
            DesiredDelivery.Equipment.PropertyChanged -= FilterDeliveries;

            DesiredDelivery = new();

            DesiredDelivery.PropertyChanged += FilterDeliveries;
            DesiredDelivery.Equipment.PropertyChanged += FilterDeliveries;
        }

        private bool CanConfirmDelivery()
        {
            return IsIncomingSelected && SelectedDelivery is not null && _canUserChangeDelivery;
        }

        private void FilterDeliveries(object? sender, PropertyChangedEventArgs e) //Too bloated. Read SOLID and Refactor.
        {
            List<Delivery> source = IsIncomingSelected ? IncomingDeliveries : OutgoingDeliveries;

            Debug.WriteLine($"Поменялось свойство фильтрации:{e.PropertyName}");

            List<Delivery> filteredList = [];

            if (source.Count == 0)
            {
                FilteredDeliveries = new ObservableCollection<Delivery> (filteredList);
                return;
            }
            
            if (sender is Equipment)
            {
                var properties = new List<Func<Delivery, bool>>
                {
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Name) || delivery.Equipment.Name.Contains(DesiredDelivery.Equipment.Name),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Description) || delivery.Equipment.Description.Contains(DesiredDelivery.Equipment.Description),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Type) || delivery.Equipment.Type.Contains(DesiredDelivery.Equipment.Type),
                    delivery => DesiredDelivery.Equipment.Id == 0 || delivery.Equipment.Id == DesiredDelivery.Equipment.Id,
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Units) || delivery.Equipment.Units.Contains(DesiredDelivery.Equipment.Units),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Limit) || delivery.Equipment.Limit.Contains(DesiredDelivery.Equipment.Limit),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.AccuracyClass) || delivery.Equipment.AccuracyClass.Contains(DesiredDelivery.Equipment.AccuracyClass),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Manufacturer) || delivery.Equipment.Manufacturer.Contains(DesiredDelivery.Equipment.Manufacturer),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.RegistrationNumber) || delivery.Equipment.RegistrationNumber.Contains(DesiredDelivery.Equipment.RegistrationNumber),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.FactoryNumber) || delivery.Equipment.FactoryNumber.Contains(DesiredDelivery.Equipment.FactoryNumber),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Status) || delivery.Equipment.Status.Contains(DesiredDelivery.Equipment.Status)
                };

                filteredList = source.Where(delivery => properties.All(property => property(delivery))).ToList();
            }
            else if (sender is Delivery)
            {
                var properties = new List<Func<Delivery, bool>>
                {
                    delivery => DesiredDelivery.Id == 0 || delivery.Id == DesiredDelivery.Id,
                    delivery => DesiredDelivery.DispatchDate == DateTime.MinValue || delivery.DispatchDate == DesiredDelivery.DispatchDate,
                    delivery => DesiredDelivery.DepartureId == 0 || delivery.DepartureId == DesiredDelivery.DepartureId,
                    delivery => DesiredDelivery.DestinationId == 0 || delivery.DestinationId == DesiredDelivery.DestinationId
                };

                filteredList = source.Where(delivery => properties.All(property => property(delivery))).ToList();
            }
            else if (sender is DeliveryControlVM && e.PropertyName == nameof(IsIncomingSelected))
            {
                var properties = new List<Func<Delivery, bool>>
                {
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Name) || delivery.Equipment.Name.Contains(DesiredDelivery.Equipment.Name),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Description) || delivery.Equipment.Description.Contains(DesiredDelivery.Equipment.Description),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Type) || delivery.Equipment.Type.Contains(DesiredDelivery.Equipment.Type),
                    delivery => DesiredDelivery.Equipment.Id == 0 || delivery.Equipment.Id == DesiredDelivery.Equipment.Id,
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Units) || delivery.Equipment.Units.Contains(DesiredDelivery.Equipment.Units),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Limit) || delivery.Equipment.Limit.Contains(DesiredDelivery.Equipment.Limit),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.AccuracyClass) || delivery.Equipment.AccuracyClass.Contains(DesiredDelivery.Equipment.AccuracyClass),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Manufacturer) || delivery.Equipment.Manufacturer.Contains(DesiredDelivery.Equipment.Manufacturer),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.RegistrationNumber) || delivery.Equipment.RegistrationNumber.Contains(DesiredDelivery.Equipment.RegistrationNumber),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.FactoryNumber) || delivery.Equipment.FactoryNumber.Contains(DesiredDelivery.Equipment.FactoryNumber),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.Equipment.Status) || delivery.Equipment.Status.Contains(DesiredDelivery.Equipment.Status),

                    delivery => DesiredDelivery.Id == 0 || delivery.Id == DesiredDelivery.Id,
                    delivery => DesiredDelivery.DispatchDate == DateTime.MinValue || delivery.DispatchDate == DesiredDelivery.DispatchDate,
                    delivery => DesiredDelivery.DepartureId == 0 || delivery.DepartureId == DesiredDelivery.DepartureId,
                    delivery => DesiredDelivery.DestinationId == 0 || delivery.DestinationId == DesiredDelivery.DestinationId
                };
                filteredList = source.Where(delivery => properties.All(property => property(delivery))).ToList();
            }
            FilteredDeliveries = new ObservableCollection<Delivery>(filteredList);
        }


        partial void OnSelectedDispatchDateChanged(DateTime? value)
        {
            if (value != null)
            {
                DesiredDelivery.DispatchDate = (DateTime)value;
            }
            else
            {
                DesiredDelivery.DispatchDate = DateTime.MinValue;
            }
        }

        partial void OnSelectedDeliveryChanged(Delivery value)
        {
            ConfirmDeliveryCommand.NotifyCanExecuteChanged();
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
