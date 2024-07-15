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

        public StorableObjectEvent()
        {
        }

        public int EmployeeId { get; set; }
        public long Id { get; set; }
        public EventType EventType { get; set; }
        public DateTime DateTime { get; set; }
        public List<IStorableObject> ObjectsInEvent { get; set; }

    }
}
