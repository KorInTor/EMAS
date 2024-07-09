using EMAS.Model.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    public class Location
    {
        /// <summary>
        /// Unique id of this site.
        /// </summary>
        private int _id;

        /// <summary>
        /// Name of site.
        /// </summary>
        private string _name;

        /// <summary>
        /// Objects that are currently in this site.
        /// </summary>
        private List<IStorableObject> _storableObjectsList;

        /// <summary>
        /// List of Outgoing deliviries.
        /// </summary>
        private List<SentEvent> _outgoingDeliviries;

        /// <summary>
        /// List of Incoming deliviries.
        /// </summary>
        private List<SentEvent> _incomingDeliviries;

        private List<ReservedEvent> _reservations;
        
        /// <summary>
        /// Return unique id of this site.
        /// </summary>
        public int Id
        {
            get
            {
                return _id;
            }
            private set
            {
                _id = value;
            }
        }

        /// <summary>
        /// Return name of site.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            private set
            {
                _name = value;
            }
        }

        /// <summary>
        /// Return and sets equipment list that are currently in this site.
        /// </summary>
        public List<IStorableObject> StorableObjectsList
        {
            get
            {
                return _storableObjectsList;
            }
            set
            {
                _storableObjectsList = value;
            }
        }

        /// <summary>
        /// Return and set list of Delivery going from this site.
        /// </summary>
        public List<SentEvent> OutgoingDeliveries
        {
            get
            {
                return _outgoingDeliviries;
            }
            set
            {
                _outgoingDeliviries = value;
            }
        }

        /// <summary>
        /// Return and set list of Incoming Delivery to this site.
        /// </summary>
        public List<SentEvent> IncomingDeliveries
        {
            get
            {
                return _incomingDeliviries;
            }
            set
            {
                _incomingDeliviries = value;
            }
        }

        public List<ReservedEvent> Reservations
        {
            get
            {
                return _reservations;
            }
            set
            {
                _reservations = value;
            }
        }

        /// <summary>
        /// Creates instance of site with Lists for equipment, Delivery, Reservation.
        /// </summary>
        /// <param name="Id"></param>
        /// <param name="Name"></param>
        public Location(int Id, string Name)
        {
            this.Id = Id;
            this.Name = Name;
            StorableObjectsList = [];
            IncomingDeliveries = [];
            OutgoingDeliveries = [];
            Reservations = [];
        }

        public Location()
        {
        }

        public Location(Location newLocation)
        {
            this.Id = newLocation.Id;
            this.Name = newLocation.Name;
            StorableObjectsList = newLocation.StorableObjectsList;
            IncomingDeliveries = newLocation.IncomingDeliveries;
            OutgoingDeliveries = newLocation.OutgoingDeliveries;
            Reservations = newLocation.Reservations;
        }
    }
}
