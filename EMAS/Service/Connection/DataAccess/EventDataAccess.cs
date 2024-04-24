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
            var Connection = ConnectionPool.GetConnection();

            StorableObjectEvent storableObjectEvent = null;

            string query = "SELECT \"date\", employee_id, event_type\r\nFROM public.\"event\" WHERE id=@id;";

            using var command = new NpgsqlCommand(query,Connection);
            command.Parameters.AddWithValue("@id",id);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                storableObjectEvent = new StorableObjectEvent(reader.GetInt32(1),id,(EventType)reader.GetInt16(2),reader.GetDateTime(0),objectEventDataAccess.SelectObjectsInEvent(id));
            }
            ConnectionPool.ReleaseConnection(Connection);
            return storableObjectEvent;
        }

        public StorableObjectEvent? SelectLast()
        {
            var Connection = ConnectionPool.GetConnection();

            StorableObjectEvent storableObjectEvent = null;

            string query = "SELECT id, \"date\", employee_id, event_type FROM public.\"event\" ORDER BY id DESC LIMIT 1;";

            using var command = new NpgsqlCommand(query, Connection);
            {
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    storableObjectEvent = new StorableObjectEvent(reader.GetInt32(2), reader.GetInt64(0), (EventType)reader.GetInt16(3), reader.GetDateTime(1), objectEventDataAccess.SelectObjectsInEvent(reader.GetInt64(0)));
                }
                ConnectionPool.ReleaseConnection(Connection);
            }
            return storableObjectEvent;
        }

        public List<StorableObjectEvent> SelectNewEventsAfter(long id)
        {
            var Connection = ConnectionPool.GetConnection();

            List<StorableObjectEvent> newEventsList = [];

            string query = "SELECT id, \"date\", employee_id, event_type FROM public.\"event\" WHERE id > @id";

            using var command = new NpgsqlCommand(query, Connection);
            {
                command.Parameters.AddWithValue("@id", id);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    newEventsList.Add(new StorableObjectEvent(reader.GetInt32(2), reader.GetInt64(0), (EventType)reader.GetInt16(3), reader.GetDateTime(1), objectEventDataAccess.SelectObjectsInEvent(reader.GetInt64(0))));
                }
            }

            ConnectionPool.ReleaseConnection(Connection);
            return newEventsList;
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
            var Connection = ConnectionPool.GetConnection();

            List<IStorableObject> objectsInEvent = [];
            EquipmentDataAccess equipmentDataAccess = new();
            MaterialDataAccess materialDataAccess = new();

            string query = "SELECT e.id, eq_ev.equipment_id, mat_ev.material_id " +
                "FROM public.\"event\" AS e " +
                "LEFT JOIN public.equipment_event AS eq_ev ON eq_ev.event_id = e.id " +
                "LEFT JOIN public.material_event AS mat_ev ON mat_ev.event_id = e.id " +
            "WHERE e.id = @id;";

            using var command = new NpgsqlCommand(query, Connection);
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
            ConnectionPool.ReleaseConnection(Connection);
            return objectsInEvent;
        }
    }
}
