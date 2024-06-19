using CommunityToolkit.Mvvm.ComponentModel;
using EMAS.Model.Event;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    public class Reservation : ObservableObject
    {
        private long _id;

        private List<IStorableObject> _reservedObjectsList;

        private DateTime _startDate;

        private DateTime _endDate = DateTime.MinValue;

        private string _reserveStartInfo;

        private string? _reserveEndInfo = null;

        private int _locationId;

        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public List<IStorableObject> ReservedObjectsList
        {
            get => _reservedObjectsList;
            set => SetProperty(ref _reservedObjectsList, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public DateTime EndDate
        {
            get => _endDate;
        }

        public string ReserveStartInfo
        {
            get => _reserveStartInfo;
            set => SetProperty(ref _reserveStartInfo, value);
        }

        public string ReserveEndInfo
        {
            get => _reserveEndInfo;
        }

        public int LocationId
        {
            get => _locationId;
            set => SetProperty(ref _locationId, value);
        }

        public bool IsCompleted
        {
            get
            {
                return _reserveStartInfo != null && _endDate != DateTime.MinValue && _reserveStartInfo != string.Empty;
            }
        }

        public Reservation(long id, DateTime startDate, string additionalInfo, List<IStorableObject> objectsToReserv,int locationId)
        {
            Id = id;
            StartDate = startDate;
            ReserveStartInfo = additionalInfo;
            ReservedObjectsList = objectsToReserv;
            LocationId = locationId;
        }

        public Reservation(StorableObjectEvent storableObjectEvent, string additionalInfo, int locationId)
        {
            Id = storableObjectEvent.Id;
            ReserveStartInfo = additionalInfo;
            ReservedObjectsList = storableObjectEvent.ObjectsInEvent;
            StartDate = storableObjectEvent.DateTime;
            LocationId = locationId;
        }

        public Reservation() 
        {
            StartDate = DateTime.MinValue;

            ReservedObjectsList = new();
        }

        public void Complete(DateTime endDate,string reserveEndComment)
        {
            _endDate = endDate;
            _reserveEndInfo = reserveEndComment;
        }
    }
}
