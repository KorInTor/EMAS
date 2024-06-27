using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public class StorableObjectEvent
    {
        public StorableObjectEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects)
        {
            EmployeeId = employeeId;
            Id = id;
            EventType = eventType;
            ObjectsInEvent = storableObjects;
            DateTime = dateTime;
        }

        public StorableObjectEvent(StorableObjectEvent storableObjectEvent)
        {
            EmployeeId = storableObjectEvent.EmployeeId;
            Id = storableObjectEvent.Id;
            EventType = storableObjectEvent.EventType;
            ObjectsInEvent = storableObjectEvent.ObjectsInEvent;
            DateTime = storableObjectEvent.DateTime;
        }

        public int EmployeeId { get; set; } = employeeId;
        public long Id { get; set; } = id;
        public EventType EventType { get; set; } = eventType;
        public List<IStorableObject> ObjectsInEvent{get;set;} = storableObjects;
        public DateTime DateTime { get; set; } = dateTime;
    }
}
