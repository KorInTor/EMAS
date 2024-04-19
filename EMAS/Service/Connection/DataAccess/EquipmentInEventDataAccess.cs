using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class EquipmentInEventDataAccess : IDataAccess<ValueTuple<long, int>>
    {
        private NpgsqlConnection _connection;

        public NpgsqlConnection Connection 
        { 
            get 
            {
                _connection ??= ConnectionPool.GetConnection();
                return _connection;
            }
            set => _connection = value; 
        }

        /// <summary>
        /// Adds new Equipemnt_Event in table.
        /// </summary>
        /// <param name="objectToAdd">Item1 = newEventId, Item2 = EquipmentId.</param>
        public void Add((long,int) objectToAdd)
        {
            using var command = new NpgsqlCommand("INSERT INTO public.\"equipment_event\" (equipment_id, event_id) VALUES (@eq_id ,@new_id) ", Connection);
            command.Parameters.AddWithValue("@new_id", objectToAdd.Item1);
            command.Parameters.AddWithValue("@eq_id", objectToAdd.Item2);

            command.ExecuteNonQuery();

            ConnectionPool.ReleaseConnection(Connection);
        }

        public void Add((long, int)[] objectToAdd)
        {
            throw new NotImplementedException();
        }

        public void Delete((long, int) objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Delete((long, int)[] objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<(long, int)> Select()
        {
            throw new NotImplementedException();
        }

        public (long, int) SelectById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update((long, int) objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void Update((long, int)[] objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
