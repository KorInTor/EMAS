using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EMAS.Model;
using EMAS.Service.Connection;
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

        public MainEquipmentVM MainEquipmentVM { get; set; } = new();

        public EquipmentController() 
        {
            Locations = DataBaseClient.GetLocationData();
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
                    location.Equipments = DataBaseClient.GetEquipmentOnLocation(location.Id);
                }
            }
        }

        partial void OnCurrentLocationChanged(Location value)
        {
            _monitor.StopActiveListeners();
            _monitor.InitListener(value.Id);

            MainEquipmentVM.LocationInfo = CurrentLocation;
        }

        private void InitLocationsData()
        {
            foreach (var location in Locations)
            {
                location.Equipments = DataBaseClient.GetEquipmentOnLocation(location.Id);
            }
        }
    }
}
