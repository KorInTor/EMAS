using CommunityToolkit.Mvvm.ComponentModel;
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
    public class Delivery : ObservableObject
    {
        /// <summary>
        /// Stores event from Dispatch event from dataBase.
        /// </summary>
        private long _eventDispatchId;

        /// <summary>
        /// Date when delivery is dispatched.
        /// </summary>
        private DateTime _dispatchDate;

        /// <summary>
        /// What equipment is in delivery.
        /// </summary>
        private Equipment _equipment;

        /// <summary>
        /// Stores destination <see cref="Location"/> id.
        /// </summary>
        private int _destinationId;

        /// <summary>
        /// Stores departure <see cref="Location"/> id.
        /// </summary>
        private int _departureId;

        /// <summary>
        /// 
        /// </summary>
        public long EventDispatchId
        {
            get => _eventDispatchId;
            set => SetProperty(ref _eventDispatchId, value);
        }


        /// <summary>
        /// Returns date of dispatch.
        /// </summary>
        public DateTime DispatchDate
        {
            get => _dispatchDate;
            set => SetProperty(ref _dispatchDate, value);
        }

        /// <summary>
        /// Returns equipment that are in current delivery.
        /// </summary>
        public Equipment Equipment
        {
            get => _equipment;
            set => SetProperty(ref _equipment, value);
        }

        /// <summary>
        /// Returns destination <see cref="Location"/> id. Or null if delivery is InGoing.
        /// </summary>
        public int DestinationId
        {
            get => _destinationId;
            set => SetProperty(ref _destinationId, value);
        }

        /// <summary>
        /// Returns destination <see cref="Location"/> id. Or null if delivery is InGoing.
        /// </summary>
        public int DepartureId
        {
            get => _departureId;
            set => SetProperty(ref _departureId, value);
        }

        /// <summary>
        /// Creates outgoing Delivery.
        /// </summary>
        /// <param name="EquipmentList">Equipment that will be sended.</param>
        public Delivery(Equipment Equipment, int destinationId)
        {
            DispatchDate = DateTime.Now;

            this.Equipment = Equipment;

        }

        public Delivery(long dispatchEventId,int departureId ,int destinationId, DateTime date, Equipment equipment)
        {
            EventDispatchId = dispatchEventId;

            DispatchDate = date;

            Equipment = equipment;

            DestinationId = destinationId;
            DepartureId = departureId;
        }

        public Delivery()
        {
            DispatchDate = DateTime.MinValue;

            Equipment = new();
        }

        public Delivery(int departureId, int destinationId, DateTime date, Equipment equipment)
        {
            EventDispatchId = 0;

            DispatchDate = date;

            Equipment = equipment;

            DestinationId = destinationId;
            DepartureId = departureId;
        }
    }
}
