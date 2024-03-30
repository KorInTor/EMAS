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

        public RelayCommand CompleteDeliveryCommand { get; set; }

        private static void ConfirmDelivery()
        {
            throw new NotImplementedException();
        }

        public DeliveryControlVM()
        {
            DesiredDelivery.PropertyChanged += FilterDeliveries;
            DesiredDelivery.Equipment.PropertyChanged += FilterDeliveries;

            CompleteDeliveryCommand = new RelayCommand(ConfirmDelivery, CanConfirmDelivery);
        }

        private bool CanConfirmDelivery()
        {
            return IsIncomingSelected && SelectedDelivery != null;
        }

        private void FilterDeliveries(object? sender, PropertyChangedEventArgs e)
        {
            List<Delivery> source;

            if (IsIncomingSelected)
            {
                source = IncomingDeliveries;
            }
            else
            {
                source = OutgoingDeliveries;
            }

            Debug.WriteLine($"Поменялось свойство фильтрации:{e.PropertyName}");

            if (source.Count == 0)
                return;

            List<Delivery> filteredList = [];

            if (sender is Equipment)
            {
                filteredList = source.Where(delivery =>
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.Name) || delivery.Equipment.Name.Contains(DesiredDelivery.Equipment.Name)) &&
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.Description) || delivery.Equipment.Description.Contains(DesiredDelivery.Equipment.Description)) &&
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.Type) || delivery.Equipment.Type.Contains(DesiredDelivery.Equipment.Type)) &&
                (DesiredDelivery.Equipment.Id == 0 || delivery.Equipment.Id == DesiredDelivery.Equipment.Id) &&
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.Units) || delivery.Equipment.Units.Contains(DesiredDelivery.Equipment.Units)) &&
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.Limit) || delivery.Equipment.Limit.Contains(DesiredDelivery.Equipment.Limit)) &&
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.AccuracyClass) || delivery.Equipment.AccuracyClass.Contains(DesiredDelivery.Equipment.AccuracyClass)) &&
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.Manufacturer) || delivery.Equipment.Manufacturer.Contains(DesiredDelivery.Equipment.Manufacturer)) &&
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.RegistrationNumber) || delivery.Equipment.RegistrationNumber.Contains(DesiredDelivery.Equipment.RegistrationNumber)) &&
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.FactoryNumber) || delivery.Equipment.FactoryNumber.Contains(DesiredDelivery.Equipment.FactoryNumber)) &&
                (string.IsNullOrEmpty(DesiredDelivery.Equipment.Status) || delivery.Equipment.Status.Contains(DesiredDelivery.Equipment.Status))
                ).ToList();
            }
            else if (sender is Delivery)
            {
                filteredList = source.Where(delivery =>
                (DesiredDelivery.EventDispatchId != 0 || delivery.EventDispatchId == DesiredDelivery.EventDispatchId) &&
                (DesiredDelivery.DispatchDate != DateTime.MinValue || delivery.DispatchDate == DesiredDelivery.DispatchDate))
                .ToList();
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
            CompleteDeliveryCommand.NotifyCanExecuteChanged();
        }

        partial void OnIsIncomingSelectedChanged(bool value)
        {
            CompleteDeliveryCommand.NotifyCanExecuteChanged();
        }
    }
}
