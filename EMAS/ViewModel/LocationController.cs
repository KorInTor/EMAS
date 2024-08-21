using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Model.Exceptions;
using Model;
using Model.Event;
using Service;
using Service.Connection;
using ViewModel.DeliveryVM;
using ViewModel.ReservationVM;
using EMAS_WPF.Windows.Dialogue.Delivery;
using EMAS_WPF.Windows.Dialogue.Reservation;
using EMAS_WPF;

namespace ViewModel
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

        private PermissionInfo _permissions = LocalSessionManager.PermissionInfo;
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

        public SingleLocationVM SingleLocationVM { get; set; } = new();

        public TopMenuVM TopMenuVM { get; set; } = new();

        public LocationController()
        {
            Locations = DataBaseClient.GetInstance().SelectLocations();
            DataBaseClient.GetInstance().SyncData(Locations);
            DataBaseClient.GetInstance().NewEventsOccured += ShowNewEventsInfo;
            DialogueService = new WindowsDialogueService();

            SingleLocationVM.EquipmentVM.DeliveryCreationRequested += ShowDeliveryCreationWindow;
            SingleLocationVM.EquipmentVM.ReservationCreationRequested += ShowReservationCreationWindow;
            SingleLocationVM.DeliveryControlVM.DeliveryConfirmationRequested += ShowDeliveryConfiramtionWindow;
            SingleLocationVM.ReservationControlVM.ReservationCompletionRequested += ShowReservationCompleteionWindow;

            SingleLocationVM.MaterialsVM.DeliveryCreationRequested += ShowDeliveryCreationWindow;
            SingleLocationVM.MaterialsVM.ReservationCreationRequested += ShowReservationCreationWindow;
            Task.Run(SyncWithDataBase);

            TopMenuVM.DataSyncRequested += SynchronizeData;
        }

        public static List<StorableObjectEvent> GetHistoryForStorableObject(int Id)
        {
            return DataBaseClient.GetInstance().SelectForStorableObjectId(Id);
        }

        partial void OnCurrentLocationChanged(Location value)
        {
            if (value == null)
            {
                return;
            }
            SingleLocationVM.Permissions = _permissions.Permissions[CurrentLocation.Id];
            SingleLocationVM.LocationInfo = value;
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
            SingleLocationVM.UpdateLocationData(CurrentLocation);
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