using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    public partial class DeliveryConfirmationVM : ObservableObject
    {
        public event Action<Delivery> DeliveryCompleted;

        private Delivery _deliveryToComplete;

        [ObservableProperty]
        private DateTime _arriveDate;

        [ObservableProperty]
        private string _arriveComment;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(MakeDeliveryCompletedCommand))]
        private bool _canCompleteDelivery;

        IWindowsDialogueService DialogueService { get; set; }

        public DeliveryConfirmationVM(Delivery deliveryToComplete,IWindowsDialogueService dialogueService)
        {
            _deliveryToComplete = deliveryToComplete;
            DialogueService = dialogueService;
            ArriveDate = DateTime.Now;
            ArriveComment = string.Empty;
        }

        partial void OnArriveDateChanged(DateTime value)
        {
            if (ArriveDate >= _deliveryToComplete.DispatchDate && ArriveComment != string.Empty)
                CanCompleteDelivery = true;
            else
                CanCompleteDelivery = false;
        }

        partial void OnArriveCommentChanged(string value)
        {
            if (ArriveDate >= _deliveryToComplete.DispatchDate && ArriveComment != string.Empty)
                CanCompleteDelivery = true;
            else
                CanCompleteDelivery = false;
        }

        [RelayCommand(CanExecute = nameof(CanCompleteDelivery))]
        private void MakeDeliveryCompleted()
        {
            _deliveryToComplete.Complete(ArriveDate,ArriveComment);
            DeliveryCompleted?.Invoke(_deliveryToComplete);
        }
    }
}
