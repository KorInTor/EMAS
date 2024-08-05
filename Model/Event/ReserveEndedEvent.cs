using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Event
{
    public class ReserveEndedEvent : StorableObjectEvent
    {

        public string Comment { get; set; }

        public long ReserveEventId;

        public ReserveEndedEvent(StorableObjectEvent storableObjectEvent, string comment, long reserveEventId) : base(storableObjectEvent)
        {
            Comment = comment;
            ReserveEventId = reserveEventId;
        }

        public ReserveEndedEvent(int employee, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, string comment, long reserveEventId) : base(employee, id, eventType, dateTime, storableObjects)
        {
            Comment = comment;
            ReserveEventId = reserveEventId;
        }
    }
}
