using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Model.Event
{
    public class StorableObjectEvent(int employeeId, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects)
    {
        public int EmployeeId { get; set; } = employeeId;
        public long Id { get; set; } = id;
        public EventType EventType { get; set; } = eventType;
        public List<IStorableObject> ObjectsInEvent{get;set;} = storableObjects;
        public DateTime DateTime { get; set; } = dateTime;

        public Delivery ToDelivery()
        {
            if (EventType != EventType.Sent)
            {
                throw new NotSupportedException("Event can be only EventType.Sent");
            }
            Delivery delivery = new();
            delivery.DispatchDate = DateTime;
            delivery.PackageList = ObjectsInEvent;
            delivery.Id = Id;
            return delivery;
        }
    }
}
