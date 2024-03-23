using EMAS.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.ViewModel
{
    internal class LocationController
    {

        private LocationManager _locationManager;

        public LocationController()
        {

        }

        public LocationManager LocationManager
        {
            get;
            private set;
        }

        private void AssertLocationByRequest()
        {

        }
    }
}
