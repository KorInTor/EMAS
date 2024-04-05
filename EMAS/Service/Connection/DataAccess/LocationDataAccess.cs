using EMAS.Model;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class LocationDataAccess : IDataAccess<Location>
    {
        public void Add(Location objectToAdd)
        {
            throw new NotImplementedException();
        }

        public void Delete(Location objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Location> Select()
        {
            var connection = ConnectionPool.GetConnection();

            string sql = "SELECT * FROM public.location";
            using var command = new NpgsqlCommand(sql, connection);

            List<Location> list = [];

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new Location(reader.GetInt32(0), reader.GetString(1)));
                }

                foreach (var data in list)
                {
                    Debug.WriteLine($"Получена локация:{data.Id}-{data.Name}");
                }

            }

            ConnectionPool.ReleaseConnection(connection);
            return list;
        }

        public Location SelectById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(Location objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
