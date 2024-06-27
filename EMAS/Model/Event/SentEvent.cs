using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public class SentEvent : StorableObjectEvent
    {
        public string Comment;

        public int DepartureId;

        public int DestinationId;

        public SentEvent(StorableObjectEvent storableObjectEvent, string comment, int departureId, int destinationId) : base(storableObjectEvent)
        {
            Comment = comment;
            DepartureId = departureId;
            DestinationId = destinationId;
        }

        public SentEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, string comment, int departureId, int destinationId) : base(employeeId, id, eventType, dateTime, storableObjects)
        {
            Comment = comment;
            DepartureId = departureId;
            DestinationId = destinationId;
        }
    }
}
