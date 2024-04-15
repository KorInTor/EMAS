using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service.Connection;
using EMAS.Service.Connection.DataAccess;
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
    public partial class EquipmentController : ObservableObject
    {
        private DataChangeChecker _monitor;

        [ObservableProperty]
        private Location _currentLocation;

        [ObservableProperty]
        private List<Location> _locations = [];
        private PermissionInfo _permissions = SessionManager.PermissionInfo;
        public MainEquipmentVM MainEquipmentVM { get; set; } = new();

        public EquipmentController()
        {
            Locations = DataBaseClient.GetInstance().SelectLocations();
            InitLocationsData();

            _monitor = new DataChangeChecker(Locations.Select(location => location.Id).ToList());
            _monitor.DataChanged += async (id) => UpdateEquipmentData(id);
        }

        private async Task UpdateEquipmentData(int ID)
        {
            foreach (var location in Locations)
            {
                if (location.Id == ID)
                {
                    location.Equipments = DataBaseClient.GetInstance().SelectEquipmentOn(location.Id);
                }
            }
        }

        partial void OnCurrentLocationChanged(Location value)
        {
            if (value == null)
            {
                return;
            }
            _monitor.StopActiveListeners();
            _monitor.InitListener(value.Id);
            MainEquipmentVM.Permissions = _permissions.Permissions[CurrentLocation.Id];
            MainEquipmentVM.LocationInfo = CurrentLocation;
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

        public static List<HistoryEntryBase> GetHistoryOfEquipmentPiece()
        {
            throw new NotImplementedException();
        }
    }
}
