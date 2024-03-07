using EMAS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.EventArgs
{
    public class LocationListEventArgs
    {
        public LocationListEventArgs(List<Location> locations)
        {
            Locations = locations;
        }

        public List<Location> Locations { get; set; }
    }
}
