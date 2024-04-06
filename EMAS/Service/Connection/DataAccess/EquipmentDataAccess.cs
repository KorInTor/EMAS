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
    public class EquipmentDataAccess : ILocationBoundedDataAccess<Equipment>
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

        public List<Equipment> SelectOnLocation(int locationId)
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
            ConnectionPool.ReleaseConnection(connection);
            return list;
        }

        public Equipment SelectById(int id)
        {
            var connection = ConnectionPool.GetConnection();

            string query = @"
    SELECT status, inventory_number, description, accuracy_class, measurment_units, measurment_limit, id, name, manufacturer, type, serial_number, tags
    FROM public.equipment
    WHERE id = @Id;
";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Id", id);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string status = reader.GetString(0);
                        string registrationNumber = reader.GetString(1);
                        string description = reader.GetString(2);
                        string accuracyClass = reader.IsDBNull(3) ? null : reader.GetString(3);
                        string units = reader.GetString(4);
                        string limit = reader.GetString(5);
                        int equipmentId = reader.GetInt32(6);
                        string name = reader.GetString(7);
                        string manufacturer = reader.GetString(8);
                        string type = reader.GetString(9);
                        string factoryNumber = reader.GetString(10);
                        List<string> tags = new List<string>((string[])reader[11]);

                        Equipment equipment = new Equipment(status, registrationNumber, description, accuracyClass, units, limit, equipmentId, name, manufacturer, type, factoryNumber, tags);
                        return equipment;
                    }
                }
            }

            ConnectionPool.ReleaseConnection(connection);

            throw new ArgumentException("No equipment found with the provided id.");
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

        public void AddOnLocation(Equipment item, int locationId)
        {
            var connection = ConnectionPool.GetConnection();

            string query = @"
        INSERT INTO public.equipment
        (id, name, manufacturer, type, measurment_units, accuracy_class, measurment_limit, serial_number, inventory_number, tags, location_id, status, description)
        VALUES(nextval('equipment_id_seq'::regclass), @Name, @Manufacturer, @Type, @Units, @AccuracyClass, @Limit, @FactoryNumber, @RegistrationNumber, @Tags, @LocationId, @Status, @Description);
    ";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Name", item.Name);
                command.Parameters.AddWithValue("@Manufacturer", item.Manufacturer);
                command.Parameters.AddWithValue("@Type", item.Type);
                command.Parameters.AddWithValue("@Units", item.Units);
                command.Parameters.AddWithValue("@AccuracyClass", item.AccuracyClass ?? (object)DBNull.Value);
                command.Parameters.AddWithValue("@Limit", item.Limit);
                command.Parameters.AddWithValue("@FactoryNumber", item.FactoryNumber);
                command.Parameters.AddWithValue("@RegistrationNumber", item.RegistrationNumber);
                command.Parameters.AddWithValue("@Tags", string.Join(",", item.Tags));
                command.Parameters.AddWithValue("@LocationId", locationId);
                command.Parameters.AddWithValue("@Status", item.Status);
                command.Parameters.AddWithValue("@Description", item.Description);

                command.ExecuteNonQuery();
            }

            ConnectionPool.ReleaseConnection(connection);
        }
    }
}
