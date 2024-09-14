using Model;
using Service.Connection.DataAccess.Interface;
using Npgsql;
using System.Diagnostics;

namespace Service.Connection.DataAccess
{
    public class LocationDataAccess : ISimpleDataAccess<Location>
    {
        public void Add(Location objectToAdd)
        {
            Add([objectToAdd]);
        }

        public void Add(Location[] objectToAdd)
        {
            foreach (Location location in objectToAdd)
            {
                var connection = ConnectionPool.GetConnection();

                string sql = "INSERT INTO public.\"location\" (\"name\") VALUES(@name) returning id;";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@name", location.Name);

                command.ExecuteNonQuery();
                //objectToAdd.Id = (int)command.ExecuteScalar();
            }
        }

        public void Delete(Location objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Delete(Location[] objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Location> Select()
        {
            var connection = ConnectionPool.GetConnection();

            string sql = "SELECT * FROM public.location WHERE id > 0";
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
            Update([objectToUpdate]);
        }

        public void Update(Location[] objectToUpdate)
        {
            foreach (var location in objectToUpdate)
            {
                var connection = ConnectionPool.GetConnection();

                string sql = "UPDATE public.\"location\" SET \"name\"=@name WHERE id=@id;";
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@name", location.Name);
                command.Parameters.AddWithValue("@id", location.Id);

                command.ExecuteNonQuery();
            }
        }
    }
}
