﻿using CommunityToolkit.Mvvm.ComponentModel;
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

            DesiredReservation = new();

            DesiredReservation.PropertyChanged += FilterReservations;
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

            //TODO: Add Actual Reservation;

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
