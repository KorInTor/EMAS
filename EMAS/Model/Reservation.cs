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

        private Equipment _equipment;

        private DateTime _startDate;

        private Employee _reservedBy;

        private string _additionalInfo;

        private int _locationId;

        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public Equipment Equipment
        {
            get => _equipment;
            set => SetProperty(ref _equipment, value);
        }

        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        public Employee ReservedBy
        {
            get => _reservedBy;
            set => SetProperty(ref _reservedBy, value);
        }

        public string AdditionalInfo
        {
            get => _additionalInfo;
            set => SetProperty(ref _additionalInfo, value);
        }

        public int LocationId
        {
            get => _locationId;
            set => SetProperty(ref _locationId, value);
        }

        public Reservation(long id, DateTime startDate, Employee reservedBy, string additionalInfo, Equipment equipment)
        {
            Id = id;
            StartDate = startDate;
            ReservedBy = reservedBy;
            AdditionalInfo = additionalInfo;
            Equipment = equipment;
        }

        public Reservation() 
        {
            StartDate = DateTime.MinValue;

            Equipment = new();
        }
    }
}
