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
    public class Delivery : ObservableObject , IObjectState, ILocationBounded
    {
        /// <summary>
        /// Stores event from Dispatch event from dataBase.
        /// </summary>
        private long _id;

        /// <summary>
        /// Date when delivery is dispatched.
        /// </summary>
        private DateTime _dispatchDate;

        private List<IStorableObject> _packageList;

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
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
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
        public List<IStorableObject> PackageList
        {
            get => _packageList;
            set => SetProperty(ref _packageList, value);
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

        public Delivery(long dispatchEventId,int departureId ,int destinationId, DateTime date, List<IStorableObject> storableObjects)
        {
            Id = dispatchEventId;

            DispatchDate = date;

            PackageList = storableObjects;

            DestinationId = destinationId;
            DepartureId = departureId;
        }

        public Delivery()
        {
            DispatchDate = DateTime.MinValue;

            PackageList = new();
        }
    }
}
