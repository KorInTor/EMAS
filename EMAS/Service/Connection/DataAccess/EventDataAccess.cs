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
            var Connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand("INSERT INTO public.event (employee_id, event_type, date) VALUES (@emp_id,@eventTypeId,@date) RETURNING id ", Connection);
            command.Parameters.AddWithValue("@emp_id", newEvent.EmployeeId);
            command.Parameters.AddWithValue("@eventTypeId", (int)newEvent.EventType);
            command.Parameters.AddWithValue("@date", newEvent.DateTime);

            newEvent.Id = (long)command.ExecuteScalar();

            if (newEvent is AdditionEvent additionEvent)
            {
                using var command1 = new NpgsqlCommand("INSERT INTO \"event\".addition\r\n(event_id, location_id)\r\nVALUES(@id, @locatioId); ", Connection);
                command1.Parameters.AddWithValue("@id", additionEvent.Id);
                command1.Parameters.AddWithValue("@locatioId", additionEvent.LocationId);
                command1.ExecuteScalar();
            }

            foreach (var storableObject in newEvent.ObjectsInEvent)
            {
                if (storableObject is Equipment)
                {
                    objectEventDataAccess.ObjectName = "equipment";
                    objectEventDataAccess.Add((newEvent.Id,storableObject.Id));
                }

                else 
                {
                    throw new NotImplementedException();
                }
            }

            ConnectionPool.ReleaseConnection(Connection);
        }

        public void Delete(StorableObjectEvent objectToDelete)
        {
            throw new NotImplementedException();
        }

        public StorableObjectEvent? SelectById(long id)
        {
            StorableObjectEvent[]? foundedEvent = SelectByIds([id]);
            if (foundedEvent == null)
                return null;
            else
                return foundedEvent[0];
        }

        public StorableObjectEvent[]? SelectByIds(long[] idsOfEvents)
        {
            List<StorableObjectEvent> storableObjectEvents = [];
            Dictionary<long, IStorableObject[]> objectsInEventDictionary = objectEventDataAccess.SelectObjectsInEvents(idsOfEvents);

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
            return [..storableObjectEvents];
        }

        public StorableObjectEvent? SelectLast()
        {
            var Connection = ConnectionPool.GetConnection();

            string query = "SELECT id FROM public.\"event\" ORDER BY id DESC LIMIT 1;";

            long? lastId = null;

            using var command = new NpgsqlCommand(query, Connection);
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    lastId = reader.GetInt64(0);
                }
                ConnectionPool.ReleaseConnection(Connection);
            }

            if (lastId == null)
                return null;
            else
                return SelectById((long)lastId);
        }

        public List<StorableObjectEvent> SelectEventsAfter(long id)
        {
            var Connection = ConnectionPool.GetConnection();

            List<long> newEventsIds = [];
            List<StorableObjectEvent> newEventsList = [];

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

            return [..SelectByIds([.. newEventsIds])];
        }

        private Dictionary<long, int?> GetLocationIdsForAdditionEvents(long[] eventIds, NpgsqlConnection connection)
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
        public string ObjectName { get; set; } = string.Empty;

        public void Add(ValueTuple<long, int> eventObjectRelation)
        {
            var Connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand("INSERT INTO public." + ObjectName + "_event (" + ObjectName + "_id, event_id) VALUES (@object_id ,@event_id) ", Connection);
            command.Parameters.AddWithValue("@event_id", eventObjectRelation.Item1);
            command.Parameters.AddWithValue("@object_id", eventObjectRelation.Item2);

            command.ExecuteNonQuery();
            ConnectionPool.ReleaseConnection(Connection);
        }

        public List<IStorableObject> SelectObjectsInEvent(long id)
        {
            return new List<IStorableObject>(SelectObjectsInEvents([id])[id]);
        }

        public Dictionary<long, IStorableObject[]> SelectObjectsInEvents(long[] idsOfEvents)
        {
            var connection = ConnectionPool.GetConnection();

            Dictionary<long, IStorableObject[]> EventIdObjectsDictionary = [];

            EquipmentDataAccess equipmentDataAccess = new();
            MaterialDataAccess materialDataAccess = new();

            string query = "SELECT e.id, eq_ev.equipment_id, mat_ev.material_id " +
                "FROM public.\"event\" AS e " +
                "LEFT JOIN public.equipment_event AS eq_ev ON eq_ev.event_id = e.id " +
                "LEFT JOIN public.material_event AS mat_ev ON mat_ev.event_id = e.id " +
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
                        try
                        {
                            objectsInEvent.Add(equipmentDataAccess.SelectById(reader.GetInt32(1)));
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                objectsInEvent.Add(materialDataAccess.SelectById(reader.GetInt32(2)));
                            }
                            catch
                            {
                                throw new NotImplementedException();
                            }
                        }
                    }
                }
                EventIdObjectsDictionary.Add(id, objectsInEvent.ToArray());
            }

            ConnectionPool.ReleaseConnection(connection);
            return EventIdObjectsDictionary;
        }
    }
}
