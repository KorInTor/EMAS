using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model;
using Model.Event;
using Service;
using Service.Connection;
using System.Collections.ObjectModel;
using System.Diagnostics;
using EMAS_WPF;

namespace ViewModel.ReservationVM
{
    public partial class ReservationCreationVM : ObservableObject
    {
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ConfrimReservationCreationCommand))]
        private bool _canCreate;

        [ObservableProperty]
        private string _reservStartComment;

        [ObservableProperty]
        private DateTime _reservStartDate = DateTime.Now;

        [ObservableProperty]
        private IStorableObject _selectedObject;

        public IWindowsDialogueService DialogueService { get; set; }

        public ObservableCollection<IStorableObject> StorableObjectsInReservation { get; set; }

        public ValueTuple<int, string> LocationIdlocationName { get; set; }

        public ReservationCreationVM(IEnumerable<IStorableObject> storableObjectsInDelivery, IWindowsDialogueService windowsDialogueService, ValueTuple<int,string> locationIdlocationName)
        {
            StorableObjectsInReservation = new (storableObjectsInDelivery);
            DialogueService = windowsDialogueService;
            LocationIdlocationName = locationIdlocationName;
            CanCreate = false;
        }
        
        [RelayCommand(CanExecute = nameof(CanCreate))]
        private void ConfrimReservationCreation()
        {
            ReservationCreated?.Invoke(new ReservedEvent(SessionManager.UserId,0,EventType.Reserved,DateTime.Now, [.. StorableObjectsInReservation], ReservStartComment,LocationIdlocationName.Item1));
        }

        [RelayCommand]
        private void RemoveSelectedFromReservation()
        {
            if (StorableObjectsInReservation.Count == 1)
                DialogueService.Close();
            if (StorableObjectsInReservation.Last().Id == SelectedObject.Id)
            {
                StorableObjectsInReservation.RemoveAt(StorableObjectsInReservation.Count - 1);
                SelectedObject = StorableObjectsInReservation.Last();
            }
            else
            {
                StorableObjectsInReservation.Remove(SelectedObject);
            }
        }

        partial void OnReservStartCommentChanged(string value)
        {
            if (ReservStartComment.Trim(new char[] { ' ', '.', ',', '\r', '\n' }) != string.Empty && ReservStartDate <= DateTime.Now)
                CanCreate = true;
            else
                CanCreate = false;
        }

        partial void OnReservStartDateChanged(DateTime value)
        {
            if (ReservStartComment.Trim(new char[] { ' ', '.', ',', '\r', '\n' }) != string.Empty && ReservStartDate <= DateTime.Now)
                CanCreate = true;
            else
                CanCreate = false;
        }

        public event Action<ReservedEvent> ReservationCreated;
    }
}
