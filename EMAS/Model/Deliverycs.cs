using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace EMAS.Model
{
    /// <summary>
    /// Stores info about active delivery,
    /// </summary>
    public class Delivery
    {
        /// <summary>
        /// Stores event from Dispatch event from dataBase.
        /// </summary>
        private int _eventDispatchId;


        /// <summary>
        /// Date when delivery is dispatched.
        /// </summary>
        private DateOnly _dispatchDate;

        /// <summary>
        /// What equipment is in delivery.
        /// </summary>
        private Equipment _equipment;

        /// <summary>
        /// Stores destination <see cref="Location"/> id.
        /// </summary>
        private int? _destinationId;

        /// <summary>
        /// 
        /// </summary>
        public int EventDispatchId
        {
            get
            {
                return _eventDispatchId;
            }
            set
            {
                _eventDispatchId = value;
            }
        }


        /// <summary>
        /// Returns date of dispatch.
        /// </summary>
        public DateOnly DispatchDate
        {
            get
            {
                return _dispatchDate;
            }
            private set
            {
                _dispatchDate = value;
            }
        }

        /// <summary>
        /// Returns equipment that are in current delivery.
        /// </summary>
        public Equipment Equipment
        {
            get
            {
                return _equipment;
            }
            private set
            {
                _equipment = value;
            }
        }

        /// <summary>
        /// Returns destination <see cref="Location"/> id. Or null if delivery is InGoing.
        /// </summary>
        public int? DestinationId
        {
            get
            {
                return _destinationId;
            }
            private set
            {
                _destinationId = value;
            }
        }

        /// <summary>
        /// Creates outgoing Delivery.
        /// </summary>
        /// <param name="EquipmentList">Equipment that will be sended.</param>
        public Delivery(Equipment Equipment, int destinationId)
        {
            DateOnly DispatchDate = DateOnly.FromDateTime(DateTime.Now);

            this.Equipment = Equipment;

        }

        /// <summary>
        /// Creates ingoing Delivery.
        /// </summary>
        /// <param name="EquipmentList">Equipment that will be sended.</param>
        public Delivery(Delivery d)
        {
            DateOnly DispatchDate = d.DispatchDate;

            this.Equipment = d.Equipment;
            this.DestinationId = null;

        }

        public Delivery(DateTime date, int destinationId, Equipment equipment)
        {
            DateTime DispatchDate = date;

            this.Equipment = equipment;

            this.DestinationId = destinationId;

        }
    }
}
