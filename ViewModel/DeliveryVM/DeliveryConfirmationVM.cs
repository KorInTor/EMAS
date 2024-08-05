using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model.Event;
using Service;
using Service.Connection;

namespace ViewModel.DeliveryVM
{
    public partial class DeliveryConfirmationVM : ObservableObject
    {
        public event Action<ArrivedEvent> DeliveryCompleted;

        private SentEvent _deliveryToComplete;

        [ObservableProperty]
        private DateTime _arriveDate;

        [ObservableProperty]
        private string _arriveComment;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MakeDeliveryCompletedCommand))]
        private bool _canCompleteDelivery;

        IWindowsDialogueService DialogueService { get; set; }

        public DeliveryConfirmationVM(SentEvent deliveryToComplete, IWindowsDialogueService dialogueService)
        {
            _deliveryToComplete = deliveryToComplete;
            DialogueService = dialogueService;
            ArriveDate = DateTime.Now;
            ArriveComment = string.Empty;
        }

        partial void OnArriveDateChanged(DateTime value)
        {
            CanCompleteDelivery = CanCompelte();
        }

        partial void OnArriveCommentChanged(string value)
        {
            CanCompleteDelivery = CanCompelte();
        }
        private bool CanCompelte()
        {
            if (ArriveDate == null || ArriveComment == null)
                return false;
            if (ArriveComment.Trim([' ', '.', ',', '\r', '\n']) == string.Empty || ArriveDate > _deliveryToComplete.DateTime)
                return false;
            return true;
        }

        [RelayCommand(CanExecute = nameof(CanCompleteDelivery))]
        private void MakeDeliveryCompleted()
        {
            ArrivedEvent arrivedEvent = new(new(SessionManager.UserId, 0, EventType.Arrived, _arriveDate, _deliveryToComplete.ObjectsInEvent), ArriveComment, _deliveryToComplete.Id);

            DeliveryCompleted?.Invoke(arrivedEvent);
        }
    }
}
