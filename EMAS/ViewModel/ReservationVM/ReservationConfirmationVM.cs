﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service;

namespace EMAS.ViewModel.ReservationVM
{
    public partial class ReservationConfirmationVM : ObservableObject
    {
        public event Action<Reservation> ReservationCompleted;

        private Reservation _ReservationToComplete;

        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private string _endComment;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MakeReservationCompletedCommand))]
        private bool _canCompleteReservation;

        IWindowsDialogueService DialogueService { get; set; }

        public ReservationConfirmationVM(Reservation ReservationToComplete, IWindowsDialogueService dialogueService)
        {
            _ReservationToComplete = ReservationToComplete;
            DialogueService = dialogueService;
            EndDate = DateTime.Now;
            EndComment = string.Empty;

            DialogueService = dialogueService;
        }

        partial void OnEndDateChanged(DateTime value)
        {
            CanCompleteReservation = CanCompelte();
        }

        partial void OnEndCommentChanged(string value)
        {
            CanCompleteReservation = CanCompelte();
        }
        private bool CanCompelte()
        {
            if (EndDate == null || EndComment == null)
                return false;
            if (EndComment.Trim([' ', '.', ',', '\r', '\n']) == string.Empty || EndDate < _ReservationToComplete.StartDate)
                return false;
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanCompleteReservation))]
        private void MakeReservationCompleted()
        {
            _ReservationToComplete.Complete(EndDate, EndComment);
            ReservationCompleted?.Invoke(_ReservationToComplete);
        }
    }
}
