using DocumentFormat.OpenXml.Office2010.Excel;
using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Media.Media3D;

namespace EMAS.Service.Connection.DataAccess
{
    public class EventDataAccess
    {
        private readonly StorableObjectInEventDataAccess objectEventDataAccess = new();

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
                using var command = new NpgsqlCommand("INSERT INTO public.event (employee_id, event_type, date) VALUES (@emp_id,@eventTypeId,@date) RETURNING id ", connection);
                command.Parameters.AddWithValue("@emp_id", newEvent.EmployeeId);
                command.Parameters.AddWithValue("@eventTypeId", (int)newEvent.EventType);
                command.Parameters.AddWithValue("@date", newEvent.DateTime);

                newEvent.Id = (long)command.ExecuteScalar();

                if (newEvent is AdditionEvent additionEvent)
                    InsertAdditionEvent(connection, additionEvent);

                preparedEquipmentEventObjectRelation.Add(newEvent.Id, newEvent.ObjectsInEvent.ToArray());
            }
            ConnectionPool.ReleaseConnection(connection);

            objectEventDataAccess.Add(preparedEquipmentEventObjectRelation);
        }

        private void InsertAdditionEvent(NpgsqlConnection connection, AdditionEvent additionEvent)
        {
            using var command = new NpgsqlCommand("INSERT INTO \"event\".addition (event_id, location_id) VALUES(@id, @locatioId); ", connection);
            command.Parameters.AddWithValue("@id", additionEvent.Id);
            command.Parameters.AddWithValue("@locatioId", additionEvent.LocationId);
            command.ExecuteScalar();
            command.Dispose();
        }

        public void Delete(StorableObjectEvent objectToDelete)
        {
            throw new NotImplementedException();
        }

        public StorableObjectEvent? SelectById(long id)
        {
            IEnumerable<StorableObjectEvent> foundedEvent = SelectByIds([id]);
            if (!foundedEvent.Any())
                return null;
            else
                return foundedEvent.First();
        }

        public IEnumerable<StorableObjectEvent> SelectByIds(IEnumerable<long> idsOfEvents)
        {
            List<StorableObjectEvent> storableObjectEvents = [];
            Dictionary<long, IEnumerable<IStorableObject>> objectsInEventDictionary = objectEventDataAccess.SelectObjectsInEvents(idsOfEvents);

            string query = "SELECT \"date\", employee_id, event_type FROM public.\"event\" WHERE id=@id;";

            var connection = ConnectionPool.GetConnection();

            foreach (long id in idsOfEvents)
            {
                using var command = new NpgsqlCommand(query, connection);
                {
                    command.Parameters.AddWithValue("@id", id);
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        storableObjectEvents.Add(new StorableObjectEvent(reader.GetInt32(1), id, (EventType)reader.GetInt16(2), reader.GetDateTime(0), [.. objectsInEventDictionary[id]]));
                    }
                    reader.Close();
                }
            }

            List<long> AdditionEventsIds = [];
            foreach (var @event in storableObjectEvents)
            {
                if (@event.EventType == EventType.Addition)
                {
                    AdditionEventsIds.Add(@event.Id);
                }
            }
            var eventIdsLocationIdsDictionary = GetLocationIdsForAdditionEvents([..AdditionEventsIds], connection);

            for (int i=0;i< storableObjectEvents.Count; i++)
            {
                if (storableObjectEvents[i].EventType == EventType.Addition)
                    storableObjectEvents[i] = new AdditionEvent(storableObjectEvents[i].EmployeeId, storableObjectEvents[i].Id, EventType.Addition, storableObjectEvents[i].DateTime, storableObjectEvents[i].ObjectsInEvent, (int)eventIdsLocationIdsDictionary[storableObjectEvents[i].Id]);
            }

            ConnectionPool.ReleaseConnection(connection);
            return storableObjectEvents;
        }

        public StorableObjectEvent? SelectLast()
        {
            var Connection = ConnectionPool.GetConnection();

            string query = "SELECT id FROM public.\"event\" ORDER BY id DESC LIMIT 1;";
            using var command = new NpgsqlCommand(query, Connection);

            long? lastId;
            {
                lastId = (long?)command.ExecuteScalar();
                ConnectionPool.ReleaseConnection(Connection);
            }

            if (lastId == null)
                return null;
            else
                return SelectById((long)lastId);
        }

        public IEnumerable<StorableObjectEvent> SelectEventsAfter(long id)
        {
            var Connection = ConnectionPool.GetConnection();

            List<long> newEventsIds = [];

            string query = "SELECT id FROM public.\"event\" WHERE id > @id";

            using var command = new NpgsqlCommand(query, Connection);
            {
                command.Parameters.AddWithValue("@id", id);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    newEventsIds.Add(reader.GetInt64(0));
                }
            }
            ConnectionPool.ReleaseConnection(Connection);

            return SelectByIds(newEventsIds);
        }

        public StorableObjectEvent SelectLastForStorableObject(IStorableObject storableObject)
        {
            return SelectLastEventsForStorableObject([storableObject]).First().Value;
        }

        public Dictionary<IStorableObject, StorableObjectEvent> SelectLastEventsForStorableObject(IEnumerable<IStorableObject> storableObjects)
        {
            var storableObjectLastEventIdDictionary = objectEventDataAccess.SelectLastEventsIdsForStroableObjects(storableObjects);
            var storableObjectLastEvent = new Dictionary<IStorableObject, StorableObjectEvent>();

            foreach (var storableObjectLastEventId in storableObjectLastEventIdDictionary)
            {
                storableObjectLastEvent.Add(storableObjectLastEventId.Key,SelectById(storableObjectLastEventId.Value));
            }

            return storableObjectLastEvent;
        }

        private Dictionary<long, int?> GetLocationIdsForAdditionEvents(IEnumerable<long> eventIds, NpgsqlConnection connection)
        {
            Dictionary<long, int?> additionIdLocationIdDictionary = [];
            string query = "SELECT location_id FROM \"event\".addition WHERE event_id=@id";
            foreach (long id in eventIds)
            {
                using var command = new NpgsqlCommand(query, connection);
                {
                    command.Parameters.AddWithValue("@id", id);
                    additionIdLocationIdDictionary.Add(id, (int?)command.ExecuteScalar());
                }
            }

            return additionIdLocationIdDictionary;
        }
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

        public Dictionary<IStorableObject, long> SelectLastEventsIdsForStroableObjects(IEnumerable<IStorableObject> storableObjects)
        {
            var connection = ConnectionPool.GetConnection();
            Dictionary<IStorableObject, long> storableObjectIdLastEventIdDictionary = [];
            string query = "SELECT e.id " +
                "FROM public.\"event\" AS e " +
                "JOIN public.storable_object_event AS object_event ON object_event.event_id = e.id " +
            "WHERE object_event.storable_object_id = @id ORDER BY id DESC LIMIT 1";

            foreach (var stroableObject in storableObjects)
            {
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", stroableObject.Id);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    storableObjectIdLastEventIdDictionary.Add(stroableObject, reader.GetInt64(0));
                }

            }
            ConnectionPool.ReleaseConnection(connection);

            return storableObjectIdLastEventIdDictionary;
        }

        public Dictionary<long, IEnumerable<IStorableObject>> SelectObjectsInEvents(IEnumerable<long> idsOfEvents)
        {
            var connection = ConnectionPool.GetConnection();

            Dictionary<long, IEnumerable<IStorableObject>> EventIdObjectsDictionary = [];

            string query = "SELECT e.id, object_event.storable_object_id " +
                "FROM public.\"event\" AS e " +
                "JOIN public.storable_object_event AS object_event ON object_event.event_id = e.id " +
            "WHERE e.id = @id;";

            foreach(long id in idsOfEvents)
            {
                List<IStorableObject> objectsInEvent = [];
                using var command = new NpgsqlCommand(query, connection);
                {
                    command.Parameters.AddWithValue("@id", id);

                    using var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(1))
                        {
                            objectsInEvent.Add(_storableObjectDataAccess.SelectById(reader.GetInt32(1)));
                        }
                    }
                }
                EventIdObjectsDictionary.Add(id, objectsInEvent);
            }

            ConnectionPool.ReleaseConnection(connection);
            return EventIdObjectsDictionary;
        }
    }
}
