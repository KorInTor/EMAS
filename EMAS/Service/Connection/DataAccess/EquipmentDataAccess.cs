using EMAS.Model;
using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;

namespace EMAS.Service.Connection.DataAccess
{
    public class EquipmentDataAccess : IStorableObjectDataAccess<Equipment>
    {

        public IEnumerable<Equipment> SelectOnLocation(int locationId)
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
                    // Tags rework in progress
                    var equipment = new Equipment(reader.GetString(11), reader.GetString(8), reader.GetString(12), reader.GetString(5), reader.GetString(4), reader.GetString(6), reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(7), []);
                    equipment.LocationId = reader.GetInt32(10);
                    list.Add(equipment);
                }

                foreach (var data in list)
                {
                    Debug.WriteLine($"Получено оборудование: {data.Id} - {data.Name}");
                }
            }
            ConnectionPool.ReleaseConnection(connection);
            return list;
        }

        public Equipment? SelectById(int id)
        {
            IEnumerable<Equipment> founded = SelectByIds([id]);
            if (!founded.Any())
                return null;
            else
                return founded.FirstOrDefault();
        }


        public void Update(Equipment objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void UpdateLocation(Equipment equipment, int newLocationId)
        {
            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand("UPDATE \"public\".equipment SET location_id=@newLocationId WHERE id=@equipmentId ", connection);
            command.Parameters.AddWithValue("@newLocationId", newLocationId);
            command.Parameters.AddWithValue("@equipmentId", equipment.Id);

            command.ExecuteNonQuery();

            ConnectionPool.ReleaseConnection(connection);
        }

        public void UpdateLocation(IEnumerable<Equipment> equipmentsToUpdate, int newLocationId)
        {
            var connection = ConnectionPool.GetConnection();

            foreach (var equipment in equipmentsToUpdate)
            {
                using var command = new NpgsqlCommand("UPDATE \"public\".equipment SET location_id=@newLocationId WHERE id=@equipmentId ", connection);
                command.Parameters.AddWithValue("@newLocationId", newLocationId);
                command.Parameters.AddWithValue("@equipmentId", equipment.Id);

                command.ExecuteNonQuery();
            }

            ConnectionPool.ReleaseConnection(connection);
        }

        public void Delete(IEnumerable<Equipment> objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<Equipment> objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void Add(IEnumerable<Equipment> objectsToAdd)
        {
            var connection = ConnectionPool.GetConnection();
            foreach (var equipment in objectsToAdd)
            {
                if (equipment.LocationId == 0)
                    throw new InvalidOperationException("Невозможно добавить оборудование если не задан id локации.");

                string query = @"
                INSERT INTO public.equipment
                (name, manufacturer, type, measurment_units, accuracy_class, measurment_limit, serial_number, inventory_number, location_id, status, description)
                VALUES(@Name, @Manufacturer, @Type, @Units, @AccuracyClass, @Limit, @FactoryNumber, @RegistrationNumber, @LocationId, @Status, @Description) RETURNING id; ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Name", equipment.Name);
                    command.Parameters.AddWithValue("@Manufacturer", equipment.Manufacturer);
                    command.Parameters.AddWithValue("@Type", equipment.Type);
                    command.Parameters.AddWithValue("@Units", equipment.Units);
                    command.Parameters.AddWithValue("@AccuracyClass", equipment.AccuracyClass ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Limit", equipment.Limit);
                    command.Parameters.AddWithValue("@FactoryNumber", equipment.FactoryNumber);
                    command.Parameters.AddWithValue("@RegistrationNumber", equipment.RegistrationNumber);
                    //command.Parameters.AddWithValue("@Tags", string.Join(",", item.Tags)); Tags Will be Reworked
                    command.Parameters.AddWithValue("@LocationId", equipment.LocationId);
                    command.Parameters.AddWithValue("@Status", equipment.Status);
                    command.Parameters.AddWithValue("@Description", equipment.Description);

                    equipment.Id = (int)command.ExecuteScalar();
                }

            }
            ConnectionPool.ReleaseConnection(connection);
        }

        public void Add(Equipment objectToAdd)
        {
            Add([objectToAdd]);
        }

        public IEnumerable<Equipment> SelectByIds(IEnumerable<int> ids)
        {
            List<Equipment> foundedEquipmentList = [];
            var connection = ConnectionPool.GetConnection();
            foreach (int id in ids)
            {
                string query = @"
                    SELECT status, inventory_number, description, accuracy_class, measurment_units, measurment_limit, id, name, manufacturer, type, serial_number, location_id 
                    FROM public.equipment
                    WHERE id = @Id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string status = reader.GetString(0);
                    string registrationNumber = reader.GetString(1);
                    string description = reader.GetString(2);
                    string? accuracyClass = reader.IsDBNull(3) ? null : reader.GetString(3);
                    string units = reader.GetString(4);
                    string limit = reader.GetString(5);
                    int equipmentId = reader.GetInt32(6);
                    string name = reader.GetString(7);
                    string manufacturer = reader.GetString(8);
                    string type = reader.GetString(9);
                    string factoryNumber = reader.GetString(10);
                    //List<string> tags = new List<string>((string[])reader[11]); Tags Will be Reworked

                    var equipment = new Equipment(status, registrationNumber, description, accuracyClass, units, limit, equipmentId, name, manufacturer, type, factoryNumber, [])
                    {
                        LocationId = reader.GetInt32(11)
                    };

                    foundedEquipmentList.Add(equipment);
                }
            }

            ConnectionPool.ReleaseConnection(connection);

            return foundedEquipmentList;
        }
    }
}
