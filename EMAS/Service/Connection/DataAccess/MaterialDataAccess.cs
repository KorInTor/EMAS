using EMAS.Model;
using EMAS.Service.Connection.DataAccess.Interface;
using Irony.Parsing;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EMAS.Service.Connection.DataAccess
{
    public class MaterialDataAccess : IStorableObjectDataAccess<MaterialPiece>
    {
        public void Add(IEnumerable<MaterialPiece> objectsToAdd)
        {
            var connection = ConnectionPool.GetConnection();
            foreach (var materialPiece in objectsToAdd)
            {

                string query = @"
               INSERT INTO public.material
               (""type"", ""name"", unit, quantity, additional_info, inventory_number, storage_place, description)
               VALUES( @Type, @Name, @Units, @Amount, @Extras, @Inventory_Number, @Storage_Type, @Description); ";

                using (var command = new NpgsqlCommand(query, connection))
                {
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

        public void Add(MaterialPiece objectToAdd)
        {
            Add([objectToAdd]);
        }

        public MaterialPiece? SelectById(int id)
        {
            IEnumerable<MaterialPiece> founded = SelectByIds([id]);
            if (!founded.Any())
                return null;
            else
                return founded.FirstOrDefault();
        }

        public IEnumerable<MaterialPiece> SelectByIds(IEnumerable<int> ids)
        {
            List<MaterialPiece> foundedMaterialsList = [];
            var connection = ConnectionPool.GetConnection();
            foreach (int id in ids)
            {
                
                string query = @"
                    SELECT ""type"", ""name"", unit, quantity, additional_info, inventory_number, storage_place, description
                    FROM public.equipment
                    WHERE id = @Id;";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@Id", id);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    string type = reader.GetString(0);
                    string name = reader.GetString(1);
                    string unit = reader.GetString(2);
                    int amount = reader.GetInt32(3);
                    string extras = reader.GetString(4);
                    string inventoryNumber = reader.GetString(5);
                    string storageType = reader.GetString(6);
                    string description = reader.GetString(7);

                    var materials = new MaterialPiece() 
                    {
                        Type = type,
                        Name = name,
                        Units = unit,
                        Amount = amount,
                        Extras = extras,
                        InventoryNumber = inventoryNumber,
                        StorageType = storageType,
                        Description = description
                    };

                    foundedMaterialsList.Add(materials);
                }
            }

            ConnectionPool.ReleaseConnection(connection);

            return foundedMaterialsList;
        }

        public void Update(IEnumerable<MaterialPiece> objectsToUpdate)
        {
            throw new NotImplementedException();
        }

        public void Update(MaterialPiece objectToUpdate)
        {
            throw new NotImplementedException();
        }

    }
}
