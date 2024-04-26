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

        /// <summary>
        /// Objects in delivery.
        /// </summary>
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
        /// Дополнительная текстовая информация про отправление.
        /// </summary>
        private string _dispatchComment;

        /// <summary>
        /// Id of event, unique through all <see cref="IObjectState"/>.
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
        /// Returns objects that are in current delivery.
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

        public string DispatchComment
        {
            get
            {
                return _dispatchComment;
            }
            set
            {
                _dispatchComment = value;
            }
        }

        public Delivery(long dispatchEventId,int departureId ,int destinationId,string departureInfo ,DateTime date, List<IStorableObject> storableObjects)
        {
            Id = dispatchEventId;

            DispatchDate = date;

            DispatchComment = departureInfo;

            PackageList = storableObjects;

            DestinationId = destinationId;
            DepartureId = departureId;
        }

        /// <summary>
        /// Default constructor. Inits <see cref="DispatchDate"/> with <see cref="DateTime.MinValue"/> and <see cref="PackageList"/>.
        /// </summary>
        public Delivery()
        {
            DispatchDate = DateTime.MinValue;

            PackageList = [];
        }
    }
}
