using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace EMAS.ViewModel
{
    public partial class ReservationControlVM : ObservableObject
    {
        public event Action<long> ReservationCompletionRequested;

        private bool _canUserChangeReservation = false;

        [ObservableProperty]
        private Reservation _desiredReservation = new();

        [ObservableProperty]
        private ObservableCollection<Reservation> _filteredReservations;

        [ObservableProperty]
        private List<Reservation> _reservationSourceList;

        [ObservableProperty]
        private Reservation _selectedReservation;

        [ObservableProperty]
        private DateTime? _selectedStartDate;

        public RelayCommand ClearFilterCommand { get; set; }

        public RelayCommand EndReservationCommand { get; set; }

        //Treat DateTime.MinValue as not setted for Filtration.
        public ReservationControlVM()
        {
            DesiredReservation.PropertyChanged += FilterReservations;
            DesiredReservation.Equipment.PropertyChanged += FilterReservations;

            ClearFilterCommand = new RelayCommand(ClearFilters);
            EndReservationCommand = new RelayCommand(RequestReservationCompletion, CanEndReservation);
        }

        private bool CanEndReservation()
        {
            return SelectedReservation is not null && _canUserChangeReservation;
        }

        private void ClearFilters()
        {
            DesiredReservation.PropertyChanged -= FilterReservations;
            DesiredReservation.Equipment.PropertyChanged -= FilterReservations;

            DesiredReservation = new();

            DesiredReservation.PropertyChanged += FilterReservations;
            DesiredReservation.Equipment.PropertyChanged += FilterReservations;
        }

        private void FilterReservations(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Поменялось свойство фильтрации:{e.PropertyName}");

            List<Reservation> filteredList = [];

            if (ReservationSourceList.Count == 0)
            {
                FilteredReservations = new ObservableCollection<Reservation>(filteredList);
                return;
            }

            if (sender is Equipment)
            {
                var properties = new List<Func<Reservation, bool>>
                {
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Name) || reservation.Equipment.Name.Contains(DesiredReservation.Equipment.Name),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Description) || reservation.Equipment.Description.Contains(DesiredReservation.Equipment.Description),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Type) || reservation.Equipment.Type.Contains(DesiredReservation.Equipment.Type),
                    reservation => DesiredReservation.Equipment.Id == 0 || reservation.Equipment.Id == DesiredReservation.Equipment.Id,
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Units) || reservation.Equipment.Units.Contains(DesiredReservation.Equipment.Units),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Limit) || reservation.Equipment.Limit.Contains(DesiredReservation.Equipment.Limit),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.AccuracyClass) || reservation.Equipment.AccuracyClass.Contains(DesiredReservation.Equipment.AccuracyClass),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Manufacturer) || reservation.Equipment.Manufacturer.Contains(DesiredReservation.Equipment.Manufacturer),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.RegistrationNumber) || reservation.Equipment.RegistrationNumber.Contains(DesiredReservation.Equipment.RegistrationNumber),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.FactoryNumber) || reservation.Equipment.FactoryNumber.Contains(DesiredReservation.Equipment.FactoryNumber),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Status) || reservation.Equipment.Status.Contains(DesiredReservation.Equipment.Status)
                };

                filteredList = ReservationSourceList.Where(reservation => properties.All(property => property(reservation))).ToList();
            }
            else if (sender is Reservation)
            {
                var properties = new List<Func<Reservation, bool>>
                {
                    reservation => DesiredReservation.Id == 0 || reservation.Id == DesiredReservation.Id,
                    reservation => DesiredReservation.StartDate == DateTime.MinValue || reservation.StartDate == DesiredReservation.StartDate,
                    reservation => DesiredReservation.ReservedBy.Id == 0 || reservation.ReservedBy.Id == DesiredReservation.ReservedBy.Id
                };

                filteredList = ReservationSourceList.Where(reservation => properties.All(property => property(reservation))).ToList();
            }
            else if (sender is ReservationControlVM && e.PropertyName == nameof(ReservationSourceList))
            {
                var properties = new List<Func<Reservation, bool>>
                {
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Name) || reservation.Equipment.Name.Contains(DesiredReservation.Equipment.Name),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Description) || reservation.Equipment.Description.Contains(DesiredReservation.Equipment.Description),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Type) || reservation.Equipment.Type.Contains(DesiredReservation.Equipment.Type),
                    reservation => DesiredReservation.Equipment.Id == 0 || reservation.Equipment.Id == DesiredReservation.Equipment.Id,
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Units) || reservation.Equipment.Units.Contains(DesiredReservation.Equipment.Units),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Limit) || reservation.Equipment.Limit.Contains(DesiredReservation.Equipment.Limit),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.AccuracyClass) || reservation.Equipment.AccuracyClass.Contains(DesiredReservation.Equipment.AccuracyClass),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Manufacturer) || reservation.Equipment.Manufacturer.Contains(DesiredReservation.Equipment.Manufacturer),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.RegistrationNumber) || reservation.Equipment.RegistrationNumber.Contains(DesiredReservation.Equipment.RegistrationNumber),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.FactoryNumber) || reservation.Equipment.FactoryNumber.Contains(DesiredReservation.Equipment.FactoryNumber),
                    reservation => string.IsNullOrEmpty(DesiredReservation.Equipment.Status) || reservation.Equipment.Status.Contains(DesiredReservation.Equipment.Status),

                    reservation => DesiredReservation.Id == 0 || reservation.Id == DesiredReservation.Id,
                    reservation => DesiredReservation.StartDate == DateTime.MinValue || reservation.StartDate == DesiredReservation.StartDate,
                    reservation => DesiredReservation.ReservedBy.Id == 0 || reservation.ReservedBy.Id == DesiredReservation.ReservedBy.Id
                };
                filteredList = ReservationSourceList.Where(reservation => properties.All(property => property(reservation))).ToList();
            }
            FilteredReservations = new ObservableCollection<Reservation>(filteredList);
        }

        partial void OnSelectedReservationChanged(Reservation value)
        {
            if (value != null)
            {
                EndReservationCommand.NotifyCanExecuteChanged();
            }
        }

        partial void OnSelectedStartDateChanged(DateTime? value)
        {
            if (value != null)
            {
                DesiredReservation.StartDate = (DateTime)value;
            }
            else
            {
                DesiredReservation.StartDate = DateTime.MinValue;
            }
        }

        //Need this because DatePicker returns null if date isnt selected;
        private void RequestReservationCompletion()
        {
            ReservationCompletionRequested?.Invoke(SelectedReservation.Id);
        }

        partial void OnReservationSourceListChanged(List<Reservation> value)
        {
            FilterReservations(this,new PropertyChangedEventArgs(nameof(ReservationSourceList)));
        }

        public void ChagneSourceList(List<Reservation> source, bool canChangeReservation = false)
        {
            ReservationSourceList = source;
            _canUserChangeReservation = canChangeReservation;
        }
    }
}
