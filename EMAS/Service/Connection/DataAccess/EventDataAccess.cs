using DocumentFormat.OpenXml.Features;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class EventDataAccess : IDataAccess<ValueTuple<int, int, int>>
    {
        public long LastEventId { get; private set; }

        private readonly EquipmentInEventDataAccess EqEventDataAccess = new();

        private static Dictionary<string, short> _eventTypes;

        private static NpgsqlConnection _connection;

        public static NpgsqlConnection Connection
        {
            get
            {
                if (_connection is null)
                {
                    Connection ??= ConnectionPool.GetConnection();
                }
                return _connection;
            }
            set => _connection = value;
        }

        public static Dictionary<string, short> EventTypes
        {
            get
            {
                if (_eventTypes is null)
                {
                    _eventTypes = SelectEventTypes();
                }
                return _eventTypes;
            }
        }

        /// <summary>
        /// Inserts new Event in DataBase.
        /// </summary>
        /// <param name="objectToAdd">Item1 = EmployeeID, Item2 = EventTypeId (Can Get From <see cref="EventTypes"/>), Item3 = EquipmentId</param>
        public void Add((int, int, int) objectToAdd)
        {
            using var command = new NpgsqlCommand("INSERT INTO public.event (employee_id, event_type) VALUES (@emp_id,@eventTypeId) RETURNING id ", Connection);
            command.Parameters.AddWithValue("@emp_id", objectToAdd.Item1);
            command.Parameters.AddWithValue("@eventTypeId", objectToAdd.Item2);

            LastEventId = (long)command.ExecuteScalar();

            ConnectionPool.ReleaseConnection(Connection);

            EqEventDataAccess.Add((LastEventId, objectToAdd.Item3));
        }

        public void Delete((int, int, int) objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<(int, int, int)> Select()
        {
            throw new NotImplementedException();
        }

        public (int, int, int) SelectById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update((int, int, int) objectToUpdate)
        {
            throw new NotImplementedException();
        }

        private static Dictionary<string, short> SelectEventTypes()
        {
            var eventTypes = new Dictionary<string,short>();

            string sql = "SELECT id, \"name\" FROM public.event_type;";
            using var command = new NpgsqlCommand(sql, Connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    eventTypes.Add(reader.GetString(1), reader.GetInt16(0));
                }

                foreach (var item in eventTypes)
                {
                    Debug.WriteLine($"Получено тип события:{item.Key} - {item.Value}");
                }

            }

            return eventTypes;
        }
    }
}
