using Model.Exceptions;
using Model;
using Model.Event;
using Service.Connection.DataAccess.Event;
using Service.Connection.DataAccess.Query;
using Npgsql;
using System.Windows;
using DocumentFormat.OpenXml.Presentation;
using Microsoft.Extensions.Logging;

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

        public EventDataAccess()
        {
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

					Query.QueryBuilder queryBuilder = new();
					queryBuilder.LazyInit<SentEvent>().Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}", "=", arrivedEvent.SentEventId);
					var completedSentEvent = Select<SentEvent>(queryBuilder).First();

                    storableObjectDataAccess.UpdateLocation(arrivedEvent.ObjectsInEvent, completedSentEvent.DestinationId);
                    //-* ... *-
                }
                if (newEvent is ReserveEndedEvent reserveEndedEvent)
                {
                    reservationDataAccess.Complete(reserveEndedEvent);

					StorableObjectDataAccess storableObjectDataAccess = new StorableObjectDataAccess();

					Query.QueryBuilder queryBuilder = new();
					queryBuilder.LazyInit<ReservedEvent>().Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}", "=", reserveEndedEvent.Id);
					var completedReserveEvent = Select<ReservedEvent>(queryBuilder).First();

					storableObjectDataAccess.UpdateLocation(reserveEndedEvent.ObjectsInEvent, completedReserveEvent.LocationId);
				}
                if (newEvent is DecomissionedEvent decomissionedEvent)
                {
                    StorableObjectDataAccess storableObjectDataAccess = new StorableObjectDataAccess();

                    decomissionDataAccess.Insert(connection, decomissionedEvent);

                    storableObjectDataAccess.UpdateLocation(decomissionedEvent.ObjectsInEvent,_decomissionedLocationId);

                    if (decomissionedEvent.StartEventId != null && decomissionedEvent.StartEventType != null)
                    {
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

            return false;
        }

		public bool IsStorableObjectsNotOccupied(IEnumerable<IStorableObject> storableObjects, out List<IStorableObject> occupiedObject)
		{
			occupiedObject = [];

			foreach (var objectLastEventPair in SelectLastEventsForStorableObjects(storableObjects.Select(x => x.Id)))
			{
				if (objectLastEventPair.Value.EventType == EventType.Sent || objectLastEventPair.Value.EventType == EventType.Reserved || objectLastEventPair.Value.EventType == EventType.Decommissioned)
				{
					occupiedObject.AddRange(objectLastEventPair.Value.ObjectsInEvent
	                .Where(x => x.Id == objectLastEventPair.Key)
	                .Select(x => x));

				}
			}

			if (occupiedObject.Count == 0)
				return true;
			else
				return false;
		}

		public Dictionary<int, StorableObjectEvent> SelectLastEventsForStorableObjects(IEnumerable<int> storableObjectsIds)
        {
            var storableObjectLastEventIdDictionary = objectEventDataAccess.SelectLastEventsIdsForStorableObjects(storableObjectsIds);
            var storableObjectLastEvent = new Dictionary<int, StorableObjectEvent>();

            var queryBuilder = new Query.QueryBuilder();
            

            foreach (var storableObjectLastEventIdPair in storableObjectLastEventIdDictionary)
            {
				queryBuilder = new Query.QueryBuilder();
				queryBuilder.LazyInit<StorableObjectEvent>().Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}", "=", storableObjectLastEventIdPair.Value);
                storableObjectLastEvent.Add(storableObjectLastEventIdPair.Key, Select<StorableObjectEvent>(queryBuilder, false).FirstOrDefault());
            }

            return storableObjectLastEvent;
        }

		public IEnumerable<TEvent> Select<TEvent>(Query.QueryBuilder queryBuilder, bool fillObjects = true) where TEvent : StorableObjectEvent
		{
			Dictionary<EventType, List<StorableObjectEvent>> preparedEventsToFetch = new();

			var storableObjectEvents = new List<TEvent>();

			Type typeOfEvent = typeof(TEvent);

			using var connection = ConnectionPool.GetConnection();

			if (!queryBuilder.IsInitialized)
				queryBuilder.LazyInit<TEvent>();

			using var command = new NpgsqlCommand(queryBuilder.Build(), connection);

			for (int i = 0; i < queryBuilder.Parameters.Count; i++)
			{
				command.Parameters.AddWithValue($"@{i}", queryBuilder.Parameters[i]);
			}

			using var reader = command.ExecuteReader();

			while (reader.Read())
			{
				var storableObjectEvent = new StorableObjectEvent(
					reader.GetInt32(0),
					reader.GetInt64(1),
					(EventType)reader.GetInt16(2),
					reader.GetDateTime(3),
					[]
				);

				if (typeOfEvent != typeof(StorableObjectEvent))
				{
					TEvent specificEvent = (TEvent)ConvertToSpecificEvent(storableObjectEvent, reader);

					storableObjectEvents.Add(specificEvent);

					continue;
				}

				if (!preparedEventsToFetch.TryGetValue(storableObjectEvent.EventType, out _))
				{
					preparedEventsToFetch[storableObjectEvent.EventType] = new List<StorableObjectEvent>();
				}

                preparedEventsToFetch[storableObjectEvent.EventType].Add(storableObjectEvent);
			}

			ConnectionPool.ReleaseConnection(connection);

			foreach (var enumEventType in preparedEventsToFetch.Keys)
			{
				storableObjectEvents.AddRange(InvokeSelectForType(queryBuilder, enumEventType, fillObjects).Cast<TEvent>());
			}

            if (!fillObjects)
            {
				return storableObjectEvents;
			}

            var eventIdObjectIds = objectEventDataAccess.SelectObjectIdForEventIds(storableObjectEvents.Select(x => x.Id));
			StorableObjectDataAccess storableObjectDataAccess = new StorableObjectDataAccess();

			foreach (var storableObjectEvent in storableObjectEvents)
            {
                storableObjectEvent.ObjectsInEvent = storableObjectDataAccess.SelectByIds(eventIdObjectIds[storableObjectEvent.Id]).ToList();
			}

			return storableObjectEvents;
		}

		private StorableObjectEvent ConvertToSpecificEvent(StorableObjectEvent baseEvent, NpgsqlDataReader reader)
		{
			return baseEvent.EventType switch
			{
				EventType.Addition => new AdditionEvent(baseEvent, reader.GetInt32(4)),
				EventType.Arrived => new ArrivedEvent(baseEvent, reader.GetString(4), reader.GetInt64(5)),
				EventType.Sent => new SentEvent(baseEvent, reader.GetString(4), reader.GetInt32(5), reader.GetInt32(6)),
				EventType.Reserved => new ReservedEvent(baseEvent, reader.GetString(4), reader.GetInt32(5)),
				EventType.ReserveEnded => new ReserveEndedEvent(baseEvent, reader.GetString(4), reader.GetInt64(5)),
				EventType.Decommissioned => new DecomissionedEvent(baseEvent, reader.GetString(4)),
				_ => throw new NotImplementedException($"EventType {baseEvent.EventType} не поддерживается.")
			};
		}

        private IEnumerable<StorableObjectEvent> InvokeSelectForType(Query.QueryBuilder queryBuilder,EventType typeOfEvent, bool fillObjects)
        {
            queryBuilder.ClearTables();

			return typeOfEvent switch
			{
				EventType.Addition => Select<AdditionEvent>(queryBuilder, fillObjects),
				EventType.Arrived => Select<ArrivedEvent>(queryBuilder, fillObjects),
				EventType.Sent => Select<SentEvent>(queryBuilder, fillObjects),
				EventType.Reserved => Select<ReservedEvent>(queryBuilder, fillObjects),
				EventType.ReserveEnded => Select<ReserveEndedEvent>(queryBuilder, fillObjects),
				EventType.Decommissioned => Select<DecomissionedEvent>(queryBuilder, fillObjects),
				EventType.DataChanged => throw new NotImplementedException(),
				_ => throw new NotImplementedException(),
			};
		}

		public Dictionary<int,List<StorableObjectEvent>> SelectEventsForStorableObjectsIds(IEnumerable<int> storableObjectIds)
		{
			var storableObjectEventIdDictionary = objectEventDataAccess.SelectEventsIdsForStorableObjectsIds(storableObjectIds);
			var storableObjectEvent = new Dictionary<int, List<StorableObjectEvent>>();

			var queryBuilder = new Query.QueryBuilder();

			foreach (var storableObjectLastEventIdPair in storableObjectEventIdDictionary)
			{
				queryBuilder.LazyInit<StorableObjectEvent>().Where($"{nameof(StorableObjectEvent)}.{nameof(StorableObjectEvent.Id)}", "=", storableObjectLastEventIdPair.Value);
				storableObjectEvent.Add(storableObjectLastEventIdPair.Key, Select<StorableObjectEvent>(queryBuilder).ToList());
			}

			return storableObjectEvent;
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

            public Dictionary<int, long> SelectLastEventsIdsForStorableObjects(IEnumerable<int> storableObjectsIds)
            {
                var storableObjectIdLastEventIdDictionary = new Dictionary<int, long>();

                string query = "SELECT object_event.storable_object_id, MAX(event_id) " +
                    "FROM public.storable_object_event AS object_event " +
                    "WHERE object_event.storable_object_id = ANY(@ids) " +
                    "GROUP BY object_event.storable_object_id ";

                using var connection = ConnectionPool.GetConnection();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@ids", storableObjectsIds.ToArray());

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    storableObjectIdLastEventIdDictionary.Add(reader.GetInt32(0), reader.GetInt64(1));
                }

                return storableObjectIdLastEventIdDictionary;
            }

			public Dictionary<int, List<long>> SelectEventsIdsForStorableObjectsIds(IEnumerable<int> storableObjectsIds)
			{
				var storableObjectIdLastEventIdDictionary = storableObjectsIds.ToDictionary(id => id, id => new List<long>());

				string query = "SELECT object_event.storable_object_id, event_id " +
					"FROM public.storable_object_event AS object_event " +
					"WHERE object_event.storable_object_id = ANY(@ids) ";

				using var connection = ConnectionPool.GetConnection();
				using var command = new NpgsqlCommand(query, connection);
				command.Parameters.AddWithValue("@ids", storableObjectsIds.ToArray());

				using var reader = command.ExecuteReader();

				while (reader.Read())
				{
                    storableObjectIdLastEventIdDictionary[reader.GetInt32(0)].Add(reader.GetInt64(1));
				}

				return storableObjectIdLastEventIdDictionary;
			}

			public Dictionary<long,List<int>> SelectObjectIdForEventIds(IEnumerable<long> eventIds)
            {
				var connection = ConnectionPool.GetConnection();

				var eventIdobjectIds = eventIds.ToDictionary(id => id, id => new List<int>());

				string query = "SELECT object_event.event_id, object_event.storable_object_id " +
					"FROM public.storable_object_event AS object_event " +
					"WHERE object_event.event_id = ANY(@ids) ";

				using var command = new NpgsqlCommand(query, connection);
                {
                    command.Parameters.AddWithValue("@ids", eventIds.ToArray());

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        eventIdobjectIds[reader.GetInt64(0)].Add(reader.GetInt32(1));
                    }

                }

				ConnectionPool.ReleaseConnection(connection);

				return eventIdobjectIds;
			}
        }
	}
}
