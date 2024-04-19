using DocumentFormat.OpenXml.Features;
using EMAS.Model.Event;
using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class EventDataAccess : IDataAccess<Event>
    {
        private readonly EquipmentInEventDataAccess EqEventDataAccess = new();

        private static Dictionary<string, short> _eventTypes;

        private static NpgsqlConnection _connection;

        public static NpgsqlConnection Connection
        {
            get
            {
                _connection ??= ConnectionPool.GetConnection();
                return _connection;
            }
            set => _connection = value;
        }

        /// <summary>
        /// Inserts new Event in DataBase.
        /// </summary>
        /// <param name="newEvent"></param>
        public void Add(Event newEvent)
        {
            using var command = new NpgsqlCommand("INSERT INTO public.event (employee_id, event_type) VALUES (@emp_id,@eventTypeId) RETURNING id ", Connection);
            command.Parameters.AddWithValue("@emp_id", newEvent.EmployeeId);
            command.Parameters.AddWithValue("@eventTypeId", (int)newEvent.EventType);

            newEvent.Id = (long)command.ExecuteScalar();

            ConnectionPool.ReleaseConnection(Connection);

            EqEventDataAccess.Add((newEvent.Id, newEvent.ObjectId));
        }

        public void Delete(Event objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Model.Event.Event> Select()
        {
            throw new NotImplementedException();
        }

        public Model.Event.Event SelectById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(Model.Event.Event objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void Add(Model.Event.Event[] objectToAdd)
        {
            throw new NotImplementedException();
        }

        public void Delete(Model.Event.Event[] objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Update(Model.Event.Event[] objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
