using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model.Event;
using Service;
using Service.Connection;
using EMAS_WPF;

namespace ViewModel.ReservationVM
{
    public partial class ReservationConfirmationVM : ObservableObject
    {
        public event Action<ReserveEndedEvent> ReservationCompleted;

        private ReservedEvent _ReservationToComplete;

        [ObservableProperty]
        private DateTime _endDate;

        [ObservableProperty]
        private string _endComment;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MakeReservationCompletedCommand))]
        private bool _canCompleteReservation;

        IWindowsDialogueService DialogueService { get; set; }

        public ReservationConfirmationVM(ReservedEvent ReservationToComplete, IWindowsDialogueService dialogueService)
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
            if (EndComment.Trim([' ', '.', ',', '\r', '\n']) == string.Empty || EndDate > _ReservationToComplete.DateTime)
                return false;
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanCompleteReservation))]
        private void MakeReservationCompleted()
        {
            ReserveEndedEvent reserveEndedEvent = new(SessionManager.UserId, 0, EventType.ReserveEnded, EndDate, _ReservationToComplete.ObjectsInEvent, EndComment, _ReservationToComplete.Id);

            ReservationCompleted?.Invoke(reserveEndedEvent);
        }
    }
}
