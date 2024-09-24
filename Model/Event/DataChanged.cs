using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Event
{
    public class DataChangedEvent : StorableObjectEvent
    {
        public string Comment { get; set; }

        public DataChangedEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, string comment) : base(employeeId, id, eventType, dateTime, storableObjects)
        {
            if (EventType != EventType.DataChanged)
                throw new InvalidOperationException("Событие DataChanged не может иметь тип отличный от DataChanged");
            Comment = comment;
        }

        public DataChangedEvent(StorableObjectEvent storableObjectEvent, string comment) : base(storableObjectEvent)
        {
            if (EventType != EventType.DataChanged)
                throw new InvalidOperationException("Событие DataChanged не может иметь тип отличный от DataChanged");
            Comment = comment;
        }
    }
}
