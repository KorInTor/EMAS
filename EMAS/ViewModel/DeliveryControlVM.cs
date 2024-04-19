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
            DesiredDelivery.PackageList.PropertyChanged += FilterDeliveries;

            ConfirmDeliveryCommand = new RelayCommand(ConfirmDelivery, CanConfirmDelivery);
            ClearFiltersCommand = new RelayCommand(ClearFilters);
        }

        private void ClearFilters()
        {
            //No memory leaks on my duty @Danil.
            DesiredDelivery.PropertyChanged -= FilterDeliveries;
            DesiredDelivery.PackageList.PropertyChanged -= FilterDeliveries;

            DesiredDelivery = new();

            DesiredDelivery.PropertyChanged += FilterDeliveries;
            DesiredDelivery.PackageList.PropertyChanged += FilterDeliveries;
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
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Name) || delivery.PackageList.Name.Contains(DesiredDelivery.PackageList.Name),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Description) || delivery.PackageList.Description.Contains(DesiredDelivery.PackageList.Description),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Type) || delivery.PackageList.Type.Contains(DesiredDelivery.PackageList.Type),
                    delivery => DesiredDelivery.PackageList.Id == 0 || delivery.PackageList.Id == DesiredDelivery.PackageList.Id,
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Units) || delivery.PackageList.Units.Contains(DesiredDelivery.PackageList.Units),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Limit) || delivery.PackageList.Limit.Contains(DesiredDelivery.PackageList.Limit),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.AccuracyClass) || delivery.PackageList.AccuracyClass.Contains(DesiredDelivery.PackageList.AccuracyClass),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Manufacturer) || delivery.PackageList.Manufacturer.Contains(DesiredDelivery.PackageList.Manufacturer),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.RegistrationNumber) || delivery.PackageList.RegistrationNumber.Contains(DesiredDelivery.PackageList.RegistrationNumber),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.FactoryNumber) || delivery.PackageList.FactoryNumber.Contains(DesiredDelivery.PackageList.FactoryNumber),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Status) || delivery.PackageList.Status.Contains(DesiredDelivery.PackageList.Status)
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
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Name) || delivery.PackageList.Name.Contains(DesiredDelivery.PackageList.Name),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Description) || delivery.PackageList.Description.Contains(DesiredDelivery.PackageList.Description),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Type) || delivery.PackageList.Type.Contains(DesiredDelivery.PackageList.Type),
                    delivery => DesiredDelivery.PackageList.Id == 0 || delivery.PackageList.Id == DesiredDelivery.PackageList.Id,
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Units) || delivery.PackageList.Units.Contains(DesiredDelivery.PackageList.Units),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Limit) || delivery.PackageList.Limit.Contains(DesiredDelivery.PackageList.Limit),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.AccuracyClass) || delivery.PackageList.AccuracyClass.Contains(DesiredDelivery.PackageList.AccuracyClass),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Manufacturer) || delivery.PackageList.Manufacturer.Contains(DesiredDelivery.PackageList.Manufacturer),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.RegistrationNumber) || delivery.PackageList.RegistrationNumber.Contains(DesiredDelivery.PackageList.RegistrationNumber),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.FactoryNumber) || delivery.PackageList.FactoryNumber.Contains(DesiredDelivery.PackageList.FactoryNumber),
                    delivery => string.IsNullOrEmpty(DesiredDelivery.PackageList.Status) || delivery.PackageList.Status.Contains(DesiredDelivery.PackageList.Status),

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
