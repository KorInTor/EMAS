using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public class ReserveEndedEvent : StorableObjectEvent
    {
        public long ReserveEventId;
        public string Comment;

        public ReserveEndedEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, long reserveEventId, string comment) : base(employeeId, id, eventType, dateTime, storableObjects)
        {
            Comment = comment;
            ReserveEventId = reserveEventId;
        }
    }
}
