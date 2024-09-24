using Model;
using Npgsql;
using Service.Connection.DataAccess.Interface;
using System.Diagnostics;

namespace Service.Connection.DataAccess
{
    public class MaterialDataAccess : IStorableObjectDataAccess<MaterialPiece>
    {
        private static Dictionary<string, string> _propertyColumnNames;

        public static Dictionary<string, string> PropertyColumnNames
        {
            get
            {
                if (_propertyColumnNames != null)
                    return _propertyColumnNames;

                _propertyColumnNames = [];

                _propertyColumnNames.Add(nameof(MaterialPiece.Name), "\"name\"");
                _propertyColumnNames.Add(nameof(MaterialPiece.StorageType), "storage_place");
                _propertyColumnNames.Add(nameof(MaterialPiece.Type), "type");
                _propertyColumnNames.Add(nameof(MaterialPiece.Units), "unit");
                //_propertyColumnNames.Add(nameof(MaterialPiece.FactoryNumber), "serial_number");
                //_propertyColumnNames.Add(nameof(MaterialPiece.RegistrationNumber), "inventory_number");

                return _propertyColumnNames;
            }
        }

        public void Add(IEnumerable<MaterialPiece> objectsToAdd)
        {
            var connection = ConnectionPool.GetConnection();
            foreach (var materialPiece in objectsToAdd)
            {

                string query = @"
               INSERT INTO public.material
               (id, ""type"", ""name"", unit, quantity, additional_info, inventory_number, storage_place, description)
               VALUES(@id , @Type, @Name, @Units, @Amount, @Extras, @Inventory_Number, @Storage_Type, @Description); ";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", materialPiece.Id);
                    command.Parameters.AddWithValue("@Type", materialPiece.Type);
                    command.Parameters.AddWithValue("@Name", materialPiece.Name);
                    command.Parameters.AddWithValue("@Units", materialPiece.Units);
                    command.Parameters.AddWithValue("@Amount", materialPiece.Amount);
                    command.Parameters.AddWithValue("@Extras", materialPiece.Extras);
                    command.Parameters.AddWithValue("@Inventory_Number", materialPiece.InventoryNumber);
                    command.Parameters.AddWithValue("@Storage_Type", materialPiece.StorageType);
                    command.Parameters.AddWithValue("@Description", materialPiece.Description);

                    _ = command.ExecuteNonQuery();
                }

            }
            ConnectionPool.ReleaseConnection(connection);
        }

        public void Delete(IEnumerable<MaterialPiece> objectToDelete)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<MaterialPiece> SelectByIds(IEnumerable<int> ids)
        {
            List<MaterialPiece> foundedMaterialsList = [];
            var connection = ConnectionPool.GetConnection();
            string query = @"
                    SELECT ""type"", ""name"", unit, quantity, additional_info, inventory_number, storage_place, description, id 
                    FROM public.material
                    WHERE id = ANY(@Id);";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", ids.ToArray());

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                int id = reader.GetInt32(8);
                string type = reader.GetString(0);
                string name = reader.GetString(1);
                string unit = reader.GetString(2);
                int amount = reader.GetInt32(3);
                string extras = reader.GetString(4);
                string inventoryNumber = reader.GetString(5);
                string storageType = reader.GetString(6);
                string description = reader.GetString(7);

                MaterialPiece material = new MaterialPiece()
                {
                    Id = id,
                    Type = type,
                    Name = name,
                    Units = unit,
                    Amount = amount,
                    Extras = extras,
                    InventoryNumber = inventoryNumber,
                    StorageType = storageType,
                    Description = description
                };
                var mat = material;
                foundedMaterialsList.Add(material);
            }

            ConnectionPool.ReleaseConnection(connection);

            return foundedMaterialsList;
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

            string distinctQueryBlank = "SELECT distinct - FROM public.material;";

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

        public void Update(IEnumerable<MaterialPiece> objectsToUpdate)
        {
            var connection = ConnectionPool.GetConnection();
            foreach (var materialPiece in objectsToUpdate)
            {
                string query = @"
        UPDATE public.material
        SET 
            ""type"" = @Type, 
            ""name"" = @Name, 
            unit = @Units, 
            quantity = @Amount, 
            additional_info = @Extras, 
            inventory_number = @Inventory_Number, 
            storage_place = @Storage_Type, 
            description = @Description
        WHERE id = @id;";

                using (var command = new NpgsqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", materialPiece.Id);
                    command.Parameters.AddWithValue("@Type", materialPiece.Type);
                    command.Parameters.AddWithValue("@Name", materialPiece.Name);
                    command.Parameters.AddWithValue("@Units", materialPiece.Units);
                    command.Parameters.AddWithValue("@Amount", materialPiece.Amount);
                    command.Parameters.AddWithValue("@Extras", materialPiece.Extras);
                    command.Parameters.AddWithValue("@Inventory_Number", materialPiece.InventoryNumber);
                    command.Parameters.AddWithValue("@Storage_Type", materialPiece.StorageType);
                    command.Parameters.AddWithValue("@Description", materialPiece.Description);

                    _ = command.ExecuteNonQuery();
                }
            }
            ConnectionPool.ReleaseConnection(connection);
        }
    }
}
