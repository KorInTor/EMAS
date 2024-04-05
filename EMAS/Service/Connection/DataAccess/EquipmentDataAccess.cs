using DocumentFormat.OpenXml.Spreadsheet;
using EMAS.Model;
using EMAS.ViewModel;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class EquipmentDataAccess : IDataAccess<Equipment>
    {
        public void Add(Equipment objectToAdd)
        {
            throw new NotImplementedException();
        }

        public void Delete(Equipment objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Equipment> Select()
        {
            throw new NotImplementedException();
        }

        public List<Equipment> SelectByLocationId(int locationId)
        {
            var connection = ConnectionPool.GetConnection();

            string sql = "SELECT id, \"name\", manufacturer, \"type\", measurment_units, accuracy_class, measurment_limit, serial_number, inventory_number, tags, location_id, status, description FROM public.equipment WHERE location_id = @locationId;";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@locationId", locationId);

            var list = new List<Equipment>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    List<string> tags = [.. ((string[])reader[9])]; // Считывание tags
                    list.Add(new Equipment(reader.GetString(11), reader.GetString(8), reader.GetString(12), reader.GetString(5), reader.GetString(4), reader.GetString(6), reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(7), tags));
                }

                foreach (var data in list)
                {
                    Debug.WriteLine($"Получено оборудование: {data.Id} - {data.Name}");
                }
            }
            connection.Close();
            return list;
        }

        public Equipment SelectById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(Equipment objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void UpdateLocation(ValueTuple<Equipment,int> equipmentAndLocationId)
        {
            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand("UPDATE \"public\".equipment SET location_id=@newLocationId, WHERE id=@equipmentId ", connection);
            command.Parameters.AddWithValue("@newLocationId", equipmentAndLocationId.Item2);
            command.Parameters.AddWithValue("@equipmentId", equipmentAndLocationId.Item1.Id);

            command.ExecuteNonQuery();

            ConnectionPool.ReleaseConnection(connection);
        }
    }
}
