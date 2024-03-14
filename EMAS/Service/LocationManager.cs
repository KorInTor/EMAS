using EMAS.Events;
using EMAS.Model;
using EMAS.Service.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service
{
    public class LocationManager
    {
        private static  Location _currentLocation;
        private static  List<Location> _locations;

        public LocationManager()
        {
            FillLocationsList();
        }

        public static  Location CurrentLocation
        {
            get
            {
                return _currentLocation;
            }
            private set
            {
                _currentLocation = value;
                GeneralEvents.InvokeLocationChanged();
            }
        }

        public static  List<Location> Locations
        {
            get
            {
                return _locations;
            }
            private set
            {
                _locations = value;
            }
        }

        private void SwitchCurrentLocation(int Id)
        {
            if(Locations == null)
            {
                return;
            }

            var locations = from locs in Locations where locs.Id == Id select locs;
            CurrentLocation = locations.First();
        }

        private void FillLocationsList()
        {
            Locations = DataBaseClient.GetLocationData();
        }
    }
}
