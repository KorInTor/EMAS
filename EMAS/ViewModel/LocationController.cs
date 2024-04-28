using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service;
using EMAS.Service.Connection;
using EMAS.Service.Connection.DataAccess;
using EMAS.Windows.Dialogue;
using EMAS.Windows.Dialogue.Delivery;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

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
        public SingleLocationVM MainEquipmentVM { get; set; } = new();
        public IWindowsDialogueService DialogueService { get; set; }
        public LocationController()
        {
            Locations = DataBaseClient.GetInstance().SelectLocations();
            DataBaseClient.GetInstance().SyncData(Locations);
            DataBaseClient.GetInstance().NewEventsOccured += ShowNewEventsInfo;
            DialogueService = new WindowsDialogueService();

            MainEquipmentVM.EquipmentVM.DeliveryCreationRequested += ShowDeliveryCreationWindow;
            MainEquipmentVM.DeliveryControlVM.DeliveryConfirmationRequested += ShowDeliveryConfiramtionWindow;

            Task.Run(SyncWithDataBase);
        }

        private void ShowDeliveryConfiramtionWindow(Delivery delivery)
        {
            DeliveryConfirmationVM deliveryConfirmationVM = new(delivery,DialogueService);
            deliveryConfirmationVM.DeliveryCompleted += TryCompleteDelivery;
            DialogueService.ShowWindow<DeliveryConfirmationWindow>(deliveryConfirmationVM);
            deliveryConfirmationVM.DeliveryCompleted -= TryCompleteDelivery;
        }

        private void TryCompleteDelivery(Delivery delivery)
        {
            try
            {
                DataBaseClient.GetInstance().Complete(delivery);
                DialogueService.Close();
                DialogueService.ShowSuccesfullMessage("Подтверждение успешно");
            }
            catch (Exception exception)
            {
                DialogueService.ShowFailMessage(exception.Message);
            }
        }

        private void ShowDeliveryCreationWindow(int arg1, IStorableObject[] arg2)
        {
            DeliveryCreationVM deliveryCreationVM = new([..arg2], LocationIdNameDictionary,arg1,DialogueService);
            deliveryCreationVM.DeliveryCreated += TryAddDelivery;
            DialogueService.ShowWindow<DeliveryCreationWindow>(deliveryCreationVM);
            deliveryCreationVM.DeliveryCreated -= TryAddDelivery;
        }

        private void TryAddDelivery(Delivery delivery)
        {
            try
            {
                DataBaseClient.GetInstance().Add(delivery);
                DialogueService.Close();
                DialogueService.ShowSuccesfullMessage("Доставка добавлена успешно");
            }
            catch(Exception exception)
            {
                DialogueService.ShowFailMessage(exception.Message);
            }
        }

        private void TestFoo(int obj)
        {
            DataBaseClient.GetInstance().SyncData(Locations);
            MainEquipmentVM.UpdateLocationData(CurrentLocation);
        }

        private void ShowNewEventsInfo(List<StorableObjectEvent> list)
        {
            MainEquipmentVM.UpdateLocationData(CurrentLocation);
        }

        private async Task SyncWithDataBase()
        {
            do
            {
                DataBaseClient.GetInstance().SyncData(Locations);

                await Task.Delay(10000);
            }
            while (true);
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

        private void InitLocationsData()
        {
            foreach (var location in Locations)
            {
                location.Equipments = DataBaseClient.GetInstance().SelectEquipmentOn(location.Id);

                location.OutgoingDeliveries = DataBaseClient.GetInstance().GetDeliverysOutOf(location.Id);
            }

            InitIncomingDeliveriesFromOutgoing();
        }

        /// <summary>
        /// Initializes Locations incoming deliveries when Location outgoing deliveries is initialized, reduces the load on the DBMS.
        /// </summary>
        private void InitIncomingDeliveriesFromOutgoing()
        {
            Dictionary<int, List<Delivery>> incomingDeliveries = [];

            foreach (var location in Locations)
            {
                incomingDeliveries.Add(location.Id, []);
            }

            foreach (var location in Locations)
            {
                foreach (var delivery in location.OutgoingDeliveries)
                {
                    incomingDeliveries[delivery.DestinationId].Add(delivery);
                }
            }

            foreach (var location in Locations)
            {
                location.IncomingDeliveries = incomingDeliveries[location.Id];
            }
        }

        public static List<HistoryEntryBase> GetHistoryOfEquipmentPiece(int Id)
        {
            return DataBaseClient.GetInstance().SelectHistoryEntryByEquipmentId(Id);
        }

        public Dictionary<int,string> LocationIdNameDictionary
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

        [RelayCommand]
        private void SynchronizeData()
        {
            DataBaseClient.GetInstance().SyncData(Locations);
        }
    }
}
