using EMAS.Model;
using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;

namespace EMAS.Service.Connection.DataAccess
{
    public class EquipmentDataAccess : IStorableObjectDataAccess<Equipment>
    {

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
                
                string query = @"
                INSERT INTO public.equipment
                (id, name, manufacturer, type, measurment_units, accuracy_class, measurment_limit, serial_number, inventory_number, status, description)
                VALUES(@id, @Name, @Manufacturer, @Type, @Units, @AccuracyClass, @Limit, @FactoryNumber, @RegistrationNumber, @Status, @Description) ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Id", equipment.Id);
                    command.Parameters.AddWithValue("@Name", equipment.Name);
                    command.Parameters.AddWithValue("@Manufacturer", equipment.Manufacturer);
                    command.Parameters.AddWithValue("@Type", equipment.Type);
                    command.Parameters.AddWithValue("@Units", equipment.Units);
                    command.Parameters.AddWithValue("@AccuracyClass", equipment.AccuracyClass ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Limit", equipment.Limit);
                    command.Parameters.AddWithValue("@FactoryNumber", equipment.FactoryNumber);
                    command.Parameters.AddWithValue("@RegistrationNumber", equipment.RegistrationNumber);
                    //command.Parameters.AddWithValue("@Tags", string.Join(",", item.Tags)); Tags Will be Reworked
                    command.Parameters.AddWithValue("@Status", equipment.Status);
                    command.Parameters.AddWithValue("@Description", equipment.Description);

                    _ = command.ExecuteNonQuery();
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
                    SELECT status, inventory_number, description, accuracy_class, measurment_units, measurment_limit, id, name, manufacturer, type, serial_number
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

                    var equipment = new Equipment(status, registrationNumber, description, accuracyClass, units, limit, equipmentId, name, manufacturer, type, factoryNumber, []);

                    foundedEquipmentList.Add(equipment);
                }
            }

            ConnectionPool.ReleaseConnection(connection);

            return foundedEquipmentList;
        }
    }
}
