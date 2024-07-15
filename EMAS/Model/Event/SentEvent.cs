namespace EMAS.Model.Event
{
    public class SentEvent : StorableObjectEvent
    {
        public int DepartureId;

        public int DestinationId;

        public string Comment { get; set; }

        public SentEvent()
        {
        }

        public SentEvent(StorableObjectEvent storableObjectEvent, string comment, int departureId, int destinationId) : base(storableObjectEvent)
        {
            Comment = comment;
            DepartureId = departureId;
            DestinationId = destinationId;
        }

        public SentEvent(int employee, long id, EventType eventType, DateTime dateTime, List<IStorableObject> storableObjects, string comment, int departureId, int destinationId) : base(employee, id, eventType, dateTime, storableObjects)
        {
            Comment = comment;
            DepartureId = departureId;
            DestinationId = destinationId;
        }
    }
}
