using Model.Exceptions;
using Model;
using Model.Event;
using Service.Connection.DataAccess.Event;
using Service.Connection.DataAccess.QueryBuilder;
using Npgsql;
using System.Windows;
using DocumentFormat.OpenXml.Presentation;

namespace Service.Connection.DataAccess
{
    public class EventDataAccess
    {
        private readonly StorableObjectInEventDataAccess objectEventDataAccess = new();
        private readonly DeliveryDataAccess deliveryDataAccess = new();
        private readonly ReservationDataAccess reservationDataAccess = new();
        private readonly AdditionDataAccess additionDataAccess = new();
        private readonly DecomissionDataAccess decomissionDataAccess = new();

        private readonly int _busyAtEventLocationId = -1;
        private readonly int _decomissionedLocationId = -2;

        private Dictionary<EventType, Type> TypeEnumToType = new Dictionary<EventType, Type>();
        public EventDataAccess()
        {
        }

        /// <summary>
        /// Inserts new Event in DataBase.
        /// </summary>
        /// <param name="newEvent"></param>
        public void Add(StorableObjectEvent newEvent)
        {
            Add([newEvent]);
        }

        public void Add(IEnumerable<StorableObjectEvent> newEvents)
        {
            var connection = ConnectionPool.GetConnection();
            Dictionary<long, IEnumerable<IStorableObject>> preparedEquipmentEventObjectRelation = [];

            foreach (var newEvent in newEvents)
            {
                if (IsCompleted(newEvent))
                    throw new EventAlreadyCompletedException();

                if (!IsStorableObjectsNotOccupied(newEvent.ObjectsInEvent, out _))
                    throw new StorableObjectIsAlreadyOccupied();

                using var command = new NpgsqlCommand("INSERT INTO public.event (employee_id, event_type, date) VALUES (@emp_id,@eventTypeId,@date) RETURNING id ", connection);
                command.Parameters.AddWithValue("@emp_id", newEvent.EmployeeId);
                command.Parameters.AddWithValue("@eventTypeId", (int)newEvent.EventType);
                command.Parameters.AddWithValue("@date", newEvent.DateTime);

                newEvent.Id = (long)command.ExecuteScalar();

                if (newEvent is AdditionEvent additionEvent)
                {
                    StorableObjectDataAccess storableObjectDataAccess = new StorableObjectDataAccess();

                    storableObjectDataAccess.Add(additionEvent.ObjectsInEvent,additionEvent.LocationId);

                    additionDataAccess.InsertAdditionEvent(connection, additionEvent);
                }
                if (newEvent is SentEvent sentEvent)
                {
                    deliveryDataAccess.Add(sentEvent);

                    StorableObjectDataAccess storableObjectDataAccess = new StorableObjectDataAccess();

                    storableObjectDataAccess.UpdateLocation(newEvent.ObjectsInEvent, _busyAtEventLocationId);
                }
                if (newEvent is ReservedEvent reservedEvent)
                {
                    reservationDataAccess.Add(reservedEvent);

                    StorableObjectDataAccess storableObjectDataAccess = new StorableObjectDataAccess();

                    storableObjectDataAccess.UpdateLocation(newEvent.ObjectsInEvent, _busyAtEventLocationId);
                }
                if (newEvent is ArrivedEvent arrivedEvent)
                {
                    deliveryDataAccess.Complete(arrivedEvent);

                    //-* Обновление локаций для объектов. *-
                    //На данный момент единственный способ понять куда нам перемещать storableObject.
                    StorableObjectDataAccess storableObjectDataAccess = new StorableObjectDataAccess();
                    var condition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.Id),Comparison.Equal,arrivedEvent.SentEventId);
                    var completedSentEvent = (SentEvent)Select([condition], typeof(SentEvent)).First();

                    storableObjectDataAccess.UpdateLocation(arrivedEvent.ObjectsInEvent, completedSentEvent.DestinationId);
                    //-* ... *-
                }
                if (newEvent is ReserveEndedEvent reserveEndedEvent)
                {
                    reservationDataAccess.Complete(reserveEndedEvent);
                }
                if (newEvent is DecomissionedEvent decomissionedEvent)
                {
                    StorableObjectDataAccess storableObjectDataAccess = new StorableObjectDataAccess();

                    decomissionDataAccess.Insert(connection, decomissionedEvent);

                    storableObjectDataAccess.UpdateLocation(decomissionedEvent.ObjectsInEvent,_decomissionedLocationId);

                    if (decomissionedEvent.StartEventId != null && decomissionedEvent.StartEventType != null)
                    {
                        var condition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.Id), Comparison.Equal, decomissionedEvent.StartEventId);
                        
                        if (decomissionedEvent.StartEventType == EventType.Sent)
                        {
                            var decomissionedArrival = new ArrivedEvent(decomissionedEvent,"списан", (long)decomissionedEvent.StartEventId);
                            deliveryDataAccess.Complete(decomissionedArrival);
                        }
                        if (decomissionedEvent.StartEventType == EventType.Reserved)
                        {
                            var decomissionedEndReserv = new ReserveEndedEvent(decomissionedEvent, "реализован", (long)decomissionedEvent.StartEventId);
                            reservationDataAccess.Complete(decomissionedEndReserv);
                        }

                    }
                }

                preparedEquipmentEventObjectRelation.Add(newEvent.Id, newEvent.ObjectsInEvent.ToArray());
            }
            ConnectionPool.ReleaseConnection(connection);

            objectEventDataAccess.Add(preparedEquipmentEventObjectRelation);

        }

        public bool IsCompleted(StorableObjectEvent storableObjectEvent)
        {
            if (storableObjectEvent is ArrivedEvent arrivedEvent)
            {
                return deliveryDataAccess.IsCompleted([arrivedEvent]);
            }
            if (storableObjectEvent is ReserveEndedEvent reserveEndedEvent)
            {
                return reservationDataAccess.IsCompleted([reserveEndedEvent]);
            }

            return true;
        }

		public bool IsStorableObjectsNotOccupied(IEnumerable<IStorableObject> storableObjects, out List<IStorableObject> occupiedObject)
		{
			occupiedObject = [];

			foreach (var objectLastEventPair in SelectLastEventsForStorableObjects(storableObjects))
			{
				if (objectLastEventPair.Value.EventType == EventType.Sent || objectLastEventPair.Value.EventType == EventType.Reserved || objectLastEventPair.Value.EventType == EventType.Decommissioned)
				{
					occupiedObject.Add(objectLastEventPair.Key);
				}
			}

			if (occupiedObject.Count == 0)
				return true;
			else
				return false;
		}

		public Dictionary<IStorableObject, StorableObjectEvent> SelectLastEventsForStorableObjects(IEnumerable<IStorableObject> storableObjects)
        {
            var storableObjectLastEventIdDictionary = objectEventDataAccess.SelectLastEventsIdsForStorableObjects(storableObjects);
            var storableObjectLastEvent = new Dictionary<IStorableObject, StorableObjectEvent>();

            foreach (var storableObjectLastEventIdPair in storableObjectLastEventIdDictionary)
            {
                var condition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.Id), Comparison.Equal, storableObjectLastEventIdPair.Value);
                storableObjectLastEvent.Add(storableObjectLastEventIdPair.Key, Select([condition]).FirstOrDefault());
            }

            return storableObjectLastEvent;
        }

        public IEnumerable<StorableObjectEvent> SelectEventsForStorableObject(int storableObjectId)
        {
            var condition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.Id), Comparison.Equal, objectEventDataAccess.SelectEventsIdsForStorableObject(storableObjectId));
            return Select([condition]);
        }

        public IEnumerable<StorableObjectEvent> Select(IEnumerable<BaseCondition> conditions, Type? typeOfEvent = null)
        {
            var storableObjectEvents = new List<StorableObjectEvent>();

            typeOfEvent ??= typeof(StorableObjectEvent);

            using var connection = ConnectionPool.GetConnection();

            var queryBuilder = new SelectQueryBuilder(typeOfEvent, conditions);

            using var command = queryBuilder.Build(connection);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var storableObjectEvent = new StorableObjectEvent(
                    reader.GetInt32(0),
                    reader.GetInt64(1),
                    (EventType)reader.GetInt16(2),
                    reader.GetDateTime(3),
                    objectEventDataAccess.SelectObjectsInEvent(reader.GetInt64(1))
                );

                if (typeOfEvent != typeof(StorableObjectEvent))
                {
                    switch (storableObjectEvent.EventType)
                    {
                        case EventType.Addition:
                            storableObjectEvent = new AdditionEvent(storableObjectEvent, reader.GetInt32(4));
                            break;
                        case EventType.Arrived:
                            storableObjectEvent = new ArrivedEvent(storableObjectEvent, reader.GetString(4), reader.GetInt64(5));
                            break;
                        case EventType.Sent:
                            storableObjectEvent = new SentEvent(storableObjectEvent, reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6));
                            break;
                        case EventType.Reserved:
                            storableObjectEvent = new ReservedEvent(storableObjectEvent, reader.GetString(4), reader.GetInt32(5));
                            break;
                        case EventType.ReserveEnded:
                            storableObjectEvent = new ReserveEndedEvent(storableObjectEvent, reader.GetString(4), reader.GetInt64(5));
                            break;
                        case EventType.Decommissioned:
                            storableObjectEvent = new DecomissionedEvent(storableObjectEvent, reader.GetString(4));
                            break;
                        case EventType.DataChanged:
                            throw new NotImplementedException();
                    }
                }
                else
                {
                    List<BaseCondition> conditions1 = [new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.Id), Comparison.Equal, storableObjectEvent.Id)];

                    switch (storableObjectEvent.EventType)
                    {
                        case EventType.Addition:
                            storableObjectEvent = Select(conditions1, typeof(AdditionEvent)).First();
                            break;
                        case EventType.Arrived:
                            storableObjectEvent = Select(conditions1, typeof(ArrivedEvent)).First();
                            break;
                        case EventType.Sent:
                            storableObjectEvent = Select(conditions1, typeof(SentEvent)).First();
                            break;
                        case EventType.Reserved:
                            storableObjectEvent = Select(conditions1, typeof(ReservedEvent)).First();
                            break;
                        case EventType.ReserveEnded:
                            storableObjectEvent = Select(conditions1, typeof(ReserveEndedEvent)).First();
                            break;
                        case EventType.Decommissioned:
                            storableObjectEvent = Select(conditions1, typeof(DecomissionedEvent)).First();
                            break;
                        case EventType.DataChanged:
                            throw new NotImplementedException();
                    }
                }
                storableObjectEvents.Add(storableObjectEvent);
            }

            ConnectionPool.ReleaseConnection(connection);

            return storableObjectEvents;
        }

        public class StorableObjectInEventDataAccess
        {
            private StorableObjectDataAccess _storableObjectDataAccess = new();

            public void Add(Dictionary<long, IEnumerable<IStorableObject>> evenIdsObjectRelation)
            {
                var connection = ConnectionPool.GetConnection();
                foreach (var eventIdObjectRelation in evenIdsObjectRelation)
                {
                    foreach (var storableObject in eventIdObjectRelation.Value)
                    {
                        string query = "INSERT INTO public.storable_object_event (storable_object_id, event_id) VALUES (@object_id ,@event_id) ";
                        using var command = new NpgsqlCommand(query, connection);

                        command.Parameters.AddWithValue("@event_id", eventIdObjectRelation.Key);
                        command.Parameters.AddWithValue("@object_id", storableObject.Id);

                        command.ExecuteNonQuery();
                    }
                }
                ConnectionPool.ReleaseConnection(connection);
            }

            public List<IStorableObject> SelectObjectsInEvent(long id)
            {
                return new List<IStorableObject>(SelectObjectsInEvents([id])[id]);
            }

            public Dictionary<IStorableObject, long> SelectLastEventsIdsForStorableObjects(IEnumerable<IStorableObject> storableObjects)
            {
                return Task.Run(() => SelectLastEventsIdsForStorableObjectsAsync(storableObjects)).Result;
            }

            public async Task<Dictionary<IStorableObject, long>> SelectLastEventsIdsForStorableObjectsAsync(IEnumerable<IStorableObject> storableObjects)
            {
                var storableObjectIdLastEventIdDictionary = new Dictionary<IStorableObject, long>();
                var ids = storableObjects.Select(o => o.Id).ToArray();

                string query = "SELECT object_event.storable_object_id, MAX(event_id) " +
                    "FROM public.storable_object_event AS object_event " +
                    "WHERE object_event.storable_object_id = ANY(@ids) " +
                    "GROUP BY object_event.storable_object_id ";

                using var connection = ConnectionPool.GetConnection();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@ids", ids);

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var id = reader.GetInt64(0);
                    var storableObject = storableObjects.First(o => o.Id == id);
                    storableObjectIdLastEventIdDictionary[storableObject] = reader.GetInt64(1);
                }

                return storableObjectIdLastEventIdDictionary;
            }


            public Dictionary<long, List<IStorableObject>> SelectObjectsInEvents(IEnumerable<long> idsOfEvents)
            {
                var connection = ConnectionPool.GetConnection();

                var EventIdObjectsDictionary = idsOfEvents.ToDictionary(id => id, id => new List<IStorableObject>());

                string query = "SELECT object_event.event_id, object_event.storable_object_id " +
                    "FROM public.storable_object_event AS object_event " +
                    "WHERE object_event.event_id = ANY(@ids) ";

                using var command = new NpgsqlCommand(query, connection);
                {
                    command.Parameters.AddWithValue("@ids", idsOfEvents.ToArray());

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        EventIdObjectsDictionary[reader.GetInt64(0)].Add(_storableObjectDataAccess.SelectById(reader.GetInt32(1)));
                    }
                }

                ConnectionPool.ReleaseConnection(connection);
                return EventIdObjectsDictionary;
            }

            public List<long> SelectEventsIdsForStorableObject(int objectId)
            {
                var connection = ConnectionPool.GetConnection();

                List<long> eventsIds = [];

                string query = "SELECT event_id FROM public.storable_object_event WHERE storable_object_id = @id";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", objectId);

                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    eventsIds.Add(reader.GetInt64(0));
                }

                ConnectionPool.ReleaseConnection(connection);

                return eventsIds;
            }
        }
    }
}
