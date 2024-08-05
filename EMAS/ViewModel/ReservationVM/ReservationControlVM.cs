using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model;
using Model.Event;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace ViewModel.ReservationVM
{
    public partial class ReservationControlVM : ObservableObject
    {
        public event Action<ReservedEvent> ReservationCompletionRequested;

        private bool _canUserChangeReservation = false;

        [ObservableProperty]
        private ReservedEvent _desiredReservation = new();

        [ObservableProperty]
        private ObservableCollection<ReservedEvent> _filteredReservations;

        [ObservableProperty]
        private List<ReservedEvent> _reservationSourceList;

        [ObservableProperty]
        private ReservedEvent _selectedReservation;

        [ObservableProperty]
        private DateTime? _selectedStartDate;

        [ObservableProperty]
        private List<IStorableObject> _reservList;

        public RelayCommand ClearFilterCommand { get; set; }

        public RelayCommand EndReservationCommand { get; set; }

        //Treat DateTime.MinValue as not setted for Filtration.
        public ReservationControlVM()
        {
            //DesiredReservation.PropertyChanged += FilterReservations;

            ClearFilterCommand = new RelayCommand(ClearFilters);
            EndReservationCommand = new RelayCommand(RequestReservationCompletion, CanEndReservation);
        }

        private bool CanEndReservation()
        {
            return SelectedReservation is not null && _canUserChangeReservation;
        }

        private void ClearFilters()
        {
            //DesiredReservation.PropertyChanged -= FilterReservations;

            DesiredReservation = new();

            //DesiredReservation.PropertyChanged += FilterReservations;
        }

        private void FilterReservations(object? sender, PropertyChangedEventArgs e)
        {
            Debug.WriteLine($"Поменялось свойство фильтрации:{e.PropertyName}");

            List<ReservedEvent> filteredList = [];

            if (ReservationSourceList.Count == 0)
            {
                FilteredReservations = new ObservableCollection<ReservedEvent>(filteredList);
                return;
            }

            //TODO: Add Actual Reservation;

            FilteredReservations = new ObservableCollection<ReservedEvent>(ReservationSourceList);
        }

        partial void OnSelectedReservationChanged(ReservedEvent value)
        {
            if (value != null)
            {
                EndReservationCommand.NotifyCanExecuteChanged();
                ReservList = value.ObjectsInEvent;
            }
        }

        partial void OnSelectedStartDateChanged(DateTime? value)
        {
            if (value != null)
            {
                DesiredReservation.DateTime = (DateTime)value;
            }
            else
            {
                DesiredReservation.DateTime = DateTime.MinValue;
            }
        }

        //Need this because DatePicker returns null if date isnt selected;
        private void RequestReservationCompletion()
        {
            ReservationCompletionRequested?.Invoke(SelectedReservation);
        }

        partial void OnReservationSourceListChanged(List<ReservedEvent> value)
        {
            FilterReservations(this, new PropertyChangedEventArgs(nameof(ReservationSourceList)));
        }

        public void ChangeSourceList(List<ReservedEvent> source, bool canChangeReserv = false)
        {
            ReservationSourceList = new(source);
            _canUserChangeReservation = canChangeReserv;
        }
    }
}
