using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model
{
    public class Reservation : ObservableObject , IObjectState, ILocationBounded
    {
        private long _id;

        private List<IStorableObject> _reservedObjectsList;

        private DateTime _startDate;

        private DateTime _endDate = DateTime.MinValue;

        private Employee _reservedBy;

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

        public Employee ReservedBy
        {
            get => _reservedBy;
            set => SetProperty(ref _reservedBy, value);
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

        public Reservation(long id, DateTime startDate, Employee reservedBy, string additionalInfo, List<IStorableObject> objectsToReserv)
        {
            Id = id;
            StartDate = startDate;
            ReservedBy = reservedBy;
            ReserveStartInfo = additionalInfo;
            ReservedObjectsList = objectsToReserv;
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
