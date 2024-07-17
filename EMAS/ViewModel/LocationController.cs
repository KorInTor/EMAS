using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Exceptions;
using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service;
using EMAS.Service.Connection;
using EMAS.ViewModel.DeliveryVM;
using EMAS.ViewModel.ReservationVM;
using Emas.View.Windows.Dialogue.Delivery;
using Emas.View.Windows.Dialogue.Reservation;

namespace EMAS.ViewModel
{
    /// <summary>
    /// So called Content Dispencer ;P
    /// </summary>
    public partial class LocationController : ObservableObject
    {
        [ObservableProperty]
        private Location _currentLocation;

        [ObservableProperty]
        private List<Location> _locations = [];

        private PermissionInfo _permissions = SessionManager.PermissionInfo;
        public IWindowsDialogueService DialogueService { get; set; }
        public Dictionary<int, string> LocationIdNameDictionary
        {
            get
            {
                Dictionary<int, string> loationIdNameDictionary = [];
                foreach (Location location in Locations)
                {
                    loationIdNameDictionary.Add(location.Id, location.Name);
                }
                return loationIdNameDictionary;
            }
        }

        public SingleLocationVM MainEquipmentVM { get; set; } = new();
        public LocationController()
        {
            Locations = DataBaseClient.GetInstance().SelectLocations();
            DataBaseClient.GetInstance().SyncData(Locations);
            DataBaseClient.GetInstance().NewEventsOccured += ShowNewEventsInfo;
            DialogueService = new WindowsDialogueService();

            MainEquipmentVM.EquipmentVM.DeliveryCreationRequested += ShowDeliveryCreationWindow;
            MainEquipmentVM.EquipmentVM.ReservationCreationRequested += ShowReservationCreationWindow;
            MainEquipmentVM.DeliveryControlVM.DeliveryConfirmationRequested += ShowDeliveryConfiramtionWindow;
            MainEquipmentVM.ReservationControlVM.ReservationCompletionRequested += ShowReservationCompleteionWindow;
            Task.Run(SyncWithDataBase);
        }

        public static List<StorableObjectEvent> GetHistoryOfEquipmentPiece(int Id)
        {
            return DataBaseClient.GetInstance().SelectForStorableObjectId(Id);
        }

        partial void OnCurrentLocationChanged(Location value)
        {
            if (value == null)
            {
                return;
            }
            MainEquipmentVM.Permissions = _permissions.Permissions[CurrentLocation.Id];
            MainEquipmentVM.LocationInfo = value;
        }

        private void ShowDeliveryConfiramtionWindow(SentEvent delivery)
        {
            DeliveryConfirmationVM deliveryConfirmationVM = new(delivery, DialogueService);
            deliveryConfirmationVM.DeliveryCompleted += TryCompleteDelivery;
            DialogueService.ShowWindow<DeliveryConfirmationWindow>(deliveryConfirmationVM);
            deliveryConfirmationVM.DeliveryCompleted -= TryCompleteDelivery;
        }

        private void ShowDeliveryCreationWindow(int arg1, IStorableObject[] arg2)
        {
            DeliveryCreationVM deliveryCreationVM = new([.. arg2], LocationIdNameDictionary, arg1, DialogueService);
            deliveryCreationVM.DeliveryCreated += TryAddDelivery;
            DialogueService.ShowWindow<DeliveryCreationWindow>(deliveryCreationVM);
            deliveryCreationVM.DeliveryCreated -= TryAddDelivery;
        }

        private void ShowNewEventsInfo(List<StorableObjectEvent> list)
        {
            //TODO Add windwos that shows what changed;
            MainEquipmentVM.UpdateLocationData(CurrentLocation);
        }

        private void ShowReservationCompleteionWindow(ReservedEvent reservationtoComplete)
        {
            ReservationConfirmationVM reservationConfirmationVM = new(reservationtoComplete, DialogueService);
            reservationConfirmationVM.ReservationCompleted += TryCompleteReservation;
            DialogueService.ShowWindow<ReservationConfirmationWindow>(reservationConfirmationVM);
            reservationConfirmationVM.ReservationCompleted -= TryCompleteReservation;
        }

        private void ShowReservationCreationWindow(int arg1, IStorableObject[] arg2)
        {
            ReservationCreationVM reservationCreationVM = new(arg2, DialogueService, (CurrentLocation.Id, CurrentLocation.Name));
            reservationCreationVM.ReservationCreated += TryAddReservation;
            DialogueService.ShowWindow<ReservationCreationWindow>(reservationCreationVM);
            reservationCreationVM.ReservationCreated -= TryAddReservation;
        }

        [RelayCommand]
        private void SynchronizeData()
        {
            DataBaseClient.GetInstance().SyncData(Locations);
        }

        private async Task SyncWithDataBase()
        {
            do
            {
                DataBaseClient.GetInstance().SyncData(Locations);

                await Task.Delay(900000);
            }
            while (true);
        }

        private void TryAddDelivery(SentEvent delivery)
        {
            TryAddEvent(delivery, [.. delivery.ObjectsInEvent]);
        }

        private void TryAddEvent(object newEvent, IStorableObject[] storableObjectsInEvent, string succesfullMessage = "Успех")
        {
            try
            {
                if (!DataBaseClient.GetInstance().IsStorableObjectsNotOccupied(storableObjectsInEvent, out _))
                    throw new StorableObjectIsAlreadyOccupied();

                DataBaseClient.GetInstance().Add(newEvent);
                DialogueService.Close();
                DialogueService.ShowSuccesfullMessage(succesfullMessage);
            }
            catch (StorableObjectIsAlreadyOccupied)
            {
                DialogueService.ShowFailMessage("Выбранные объекты уже заняты");
            }
            catch (Exception exception)
            {
                DialogueService.ShowFailMessage(exception.Message);
            }
        }

        private void TryAddReservation(ReservedEvent reservation)
        {
            TryAddEvent(reservation, [.. reservation.ObjectsInEvent]);
        }

        private void TryCompleteDelivery(ArrivedEvent deliveryToComplete)
        {
            TryCompleteEvent(deliveryToComplete);
        }

        private void TryCompleteEvent(object completedEvent, string succesfullMessage = "Успех")
        {
            try
            {
                DataBaseClient.GetInstance().Add(completedEvent);
                DialogueService.Close();
                DialogueService.ShowSuccesfullMessage(succesfullMessage);
            }
            catch (EventAlreadyCompletedException)
            {
                DataBaseClient.GetInstance().SyncData(Locations);
                DialogueService.Close();
                DialogueService.ShowFailMessage("Данное событие уже завершенно.");
            }
            catch (Exception exception)
            {
                DialogueService.ShowFailMessage(exception.Message);
            }
        }

        private void TryCompleteReservation(ReserveEndedEvent reservation)
        {
            TryCompleteEvent(reservation);
        }
    }
}