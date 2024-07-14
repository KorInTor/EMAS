using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public class AdditionEvent : StorableObjectEvent
    {
        public int LocationId { get; set; }

        public AdditionEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects,int locationId) : base(employeeId, id, eventType, dateTime, storableObjects)
        {
            if (EventType != EventType.Addition)
                throw new InvalidOperationException("Событие добавление не может иметь тип отличный от Addition");
            LocationId = locationId;
        }

        public AdditionEvent(StorableObjectEvent storableObjectEvent, int locationId) : base(storableObjectEvent)
        {
            if (EventType != EventType.Addition)
                throw new InvalidOperationException("Событие добавление не может иметь тип отличный от Addition");
            LocationId = locationId;
        }
    }
}
