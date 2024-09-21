using Model;
using Npgsql;

namespace Service.Connection.DataAccess
{
    public class EquipmentDataAccess
    {
        private static Dictionary<string, string> _propertyColumnNames;

        public static Dictionary<string,string> PropertyColumnNames
        {
            get
            {
                if (_propertyColumnNames != null)
                    return _propertyColumnNames;

                _propertyColumnNames = [];

                _propertyColumnNames.Add(nameof(Equipment.Name),"\"name\"");
                _propertyColumnNames.Add(nameof(Equipment.Manufacturer), "manufacturer");
                _propertyColumnNames.Add(nameof(Equipment.Type), "type");
                _propertyColumnNames.Add(nameof(Equipment.Units), "measurment_units");
                _propertyColumnNames.Add(nameof(Equipment.AccuracyClass), "accuracy_class");
                _propertyColumnNames.Add(nameof(Equipment.Limit), "measurment_limit");
                //_propertyColumnNames.Add(nameof(Equipment.FactoryNumber), "serial_number");
                //_propertyColumnNames.Add(nameof(Equipment.RegistrationNumber), "inventory_number");

                return _propertyColumnNames;
            }
        }

        public Equipment? SelectById(int id)
        {
            IEnumerable<Equipment> founded = SelectByIds([id]);
            if (!founded.Any())
                return null;
            else
                return founded.FirstOrDefault();
        }

        public Dictionary<int,string> SelectStatuses()
        {
            Dictionary<int, string> statuses = [];

            string query = "SELECT id, \"name\"\r\nFROM public.equipment_status";
			var connection = ConnectionPool.GetConnection();

			using var command = new NpgsqlCommand(query, connection);

			using var reader = command.ExecuteReader();
			while (reader.Read())
			{
                statuses.Add(reader.GetInt32(0),reader.GetString(1));
			}

			ConnectionPool.ReleaseConnection(connection);

			return statuses;
		}

        public Dictionary<string, List<string>> SelectDistinct(IEnumerable<string>? propertyToSelect = null)
        {
            Dictionary<string, List<string>> distinctPropertyValues = [];

            IEnumerable<string> properties;
            if (propertyToSelect != null)
            {
                properties = propertyToSelect;
            }
            else
            {
                properties = PropertyColumnNames.Keys;
            }

            string distinctQueryBlank = "SELECT distinct - FROM public.equipment;";

            var connection = ConnectionPool.GetConnection();
            foreach (string property in properties)
            {
                string distinctQuery = distinctQueryBlank.Replace("-", PropertyColumnNames[property]);

                var command = new NpgsqlCommand(distinctQuery, connection);
                var reader = command.ExecuteReader();
                distinctPropertyValues.Add(property, []);
                while (reader.Read())
                {
                    distinctPropertyValues[property].Add(reader.GetString(0));
                }
                reader.Close();
                command.Dispose();
            }

            ConnectionPool.ReleaseConnection(connection);

            return distinctPropertyValues;
        }

        public void UpdateStatuses(IEnumerable<(int,string)> statuses)
        {
            string insertQuery = "INSERT INTO public.equipment_status (\"name\") VALUES(@statusName);";
            string deleteQuery = "DELETE FROM public.equipment_status WHERE id=@id;";
            string updateQuery = "UPDATE public.equipment_status SET \"name\"=@newName WHERE id=@id;";

            List<(int, string)> statusesToUpdate = [];
            List<(int, string)> statusesToDelete = [];
            List<(int, string)> statusesToInsert = [];

            Dictionary<int, string> oldStatuses = SelectStatuses();
            oldStatuses.Remove(-1);
            oldStatuses.Remove(-2);

            foreach (var status in statuses)
            {
                if (status.Item1 == 0)
                {
                    statusesToInsert.Add(status);
                    continue;
                }

                foreach (var oldInt in oldStatuses.Keys)
                {
                    if (status.Item1 == oldInt)
                    {
                        statusesToUpdate.Add(status);
                        oldStatuses.Remove(oldInt);
                        break;
                    }
                }
            }

            foreach (var oldStatus in oldStatuses)
            {
                statusesToDelete.Add((oldStatus.Key, oldStatus.Value));
            }

            var connection = ConnectionPool.GetConnection();

            foreach (var newStatus in statusesToInsert)
            {
                using (var command = new NpgsqlCommand(insertQuery, connection))
                {
                    command.Parameters.AddWithValue("@statusName", newStatus.Item2);
                    command.ExecuteNonQuery();
                }
            }

            foreach (var updatedStatus in statusesToUpdate)
            {
                using (var command = new NpgsqlCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", updatedStatus.Item1);
                    command.Parameters.AddWithValue("@newName", updatedStatus.Item2);
                    command.ExecuteNonQuery();
                }
            }

            foreach (var removedStatus in statusesToDelete)
            {
                using (var command = new NpgsqlCommand(deleteQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", removedStatus.Item1);
                    command.ExecuteNonQuery();
                }
            }

            ConnectionPool.ReleaseConnection(connection);
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
			Dictionary<int, string> statusesDictionary = SelectStatuses();

			foreach (var equipment in objectsToAdd)
            {
                bool isStatusValid = false;
                int statusId = 0;
                foreach(KeyValuePair<int,string> status in statusesDictionary)
                {
                    if (equipment.Status == status.Value)
                    {
                        statusId = status.Key;
						isStatusValid = true;
					}
                        
                }

                if (!isStatusValid)
                {
                    throw new ArgumentException("Такого статуса нет в базе данных");
                }

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
                    command.Parameters.AddWithValue("@Status", statusId);
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
            Dictionary<int, string> statusesDictionary = SelectStatuses(); 
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
                    string status = statusesDictionary[reader.GetInt32(0)];
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
