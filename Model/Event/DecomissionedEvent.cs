using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Event
{
    public class DecomissionedEvent : StorableObjectEvent
    {
        public long? StartEventId;
        public EventType? StartEventType;

        public int LocationId;
        public string Comment;

        public DecomissionedEvent(StorableObjectEvent storableObjectEvent, string comment, int locationId) : base(storableObjectEvent)
        {
            Comment = comment;
            LocationId = locationId;
        }

        public DecomissionedEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, string comment, int locationId) : base(employeeId, id, eventType, dateTime, storableObjects)
        {
            Comment = comment;
			LocationId = locationId;
		}

        public DecomissionedEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, string comment, EventType startEventType, long startEventId, int locationId) : base(employeeId, id, eventType, dateTime, storableObjects)
        {
            StartEventId = startEventId;
            StartEventType = startEventType;
            Comment = comment;
			LocationId = locationId;
		}
    }
}
