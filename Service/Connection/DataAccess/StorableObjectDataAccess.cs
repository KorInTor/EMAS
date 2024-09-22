using Model;
using Model.Enum;
using Npgsql;
using Service.Connection.DataAccess.Interface;

namespace Service.Connection.DataAccess
{
    public class StorableObjectDataAccess
    {
        public IStorableObjectDataAccess<Equipment> equipmentDataAccess;
        public IStorableObjectDataAccess<MaterialPiece> materialDataAccess;
        public IStorableObjectStatusDataAccess<Equipment> equipmentStatusDataAccess;

        public StorableObjectDataAccess()
        {
            equipmentDataAccess = new EquipmentDataAccess();
            equipmentStatusDataAccess = (IStorableObjectStatusDataAccess<Equipment>)equipmentDataAccess;

            materialDataAccess = new MaterialDataAccess();
        }

        /// <summary>
        /// Добавляет в базу новый список объектов, подразумевается что все объекты должны быть добавлены на одну локацию.
        /// </summary>
        /// <param name="objectsToAdd">Список на добавление</param>
        public void Add(IEnumerable<IStorableObject> objectsToAdd, int locationId)
        {
            var splittedObjects = objectsToAdd.GroupByType();

            var connection = ConnectionPool.GetConnection();
            string query = "INSERT INTO public.storable_object (type_id, location_id) VALUES (@typeID, @location_id) returning id";

            //Добавляем в таблицу storable_object данные о типе объекта и возвращаем новый ID.
            foreach (var storableObjectTypedList in splittedObjects)
            {
                foreach(var storableObject in storableObjectTypedList.Value) 
                {
                    using var command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@typeID", (int)storableObjectTypedList.Key);
                    command.Parameters.AddWithValue("@location_id", locationId);

                    storableObject.Id = (int)command.ExecuteScalar();
                }
            }

            ConnectionPool.ReleaseConnection(connection);

            //Добавление в таблицу явного типа объекта (eg. Equipment в public.equipment)
            foreach (var objectTypeList in splittedObjects)
            {
                switch (objectTypeList.Key)
                {
                    case StorableObjectType.Equipment:
                        equipmentDataAccess.Add(objectTypeList.Value.Cast<Equipment>());
                        break;
                    case StorableObjectType.Material:
                        materialDataAccess.Add(objectTypeList.Value.Cast<MaterialPiece>());
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public IEnumerable<IStorableObject> SelectByIds(IEnumerable<int> ids)
        {
            var connection = ConnectionPool.GetConnection();

            string query = "SELECT id, type_id, location_id FROM public.storable_object WHERE id = @id;";

            var objectTypes = Enum.GetValues(typeof(StorableObjectType)).Cast<StorableObjectType>();

            var typesIdsDictionary = objectTypes.ToDictionary(e => e, e => new List<int>());

            foreach (int id in ids)
            {
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    typesIdsDictionary[(StorableObjectType)reader.GetInt32(1)].Add(id);
                }
            }

            ConnectionPool.ReleaseConnection(connection);

            List<IStorableObject> foundedObjects = [];

            foreach (var type in typesIdsDictionary.Keys)
            {
                switch (type)
                {
                    case StorableObjectType.Equipment:
                        foundedObjects.AddRange(equipmentDataAccess.SelectByIds(typesIdsDictionary[type]));
                        break;
                    case StorableObjectType.Material:
                        foundedObjects.AddRange(materialDataAccess.SelectByIds(typesIdsDictionary[type]));
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }

            return foundedObjects;
        }

        public IEnumerable<IStorableObject> SelectOnLocation(int locationId)
        {
            List<int> idsList = [];

            var connection = ConnectionPool.GetConnection();

            string query = "SELECT id FROM public.storable_object WHERE location_id = @locationId";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@locationId", locationId);
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                idsList.Add(reader.GetInt32(0));
            }
            ConnectionPool.ReleaseConnection(connection);

            return SelectByIds(idsList);
        }

        public void Update(IEnumerable<IStorableObject> objectsToUpdate)
        {
            var splitedArray = objectsToUpdate.GroupByType();

			foreach (var type in splitedArray.Keys)
            {
                switch (type)
                {
                    case StorableObjectType.Equipment:
                        equipmentDataAccess.Update(splitedArray[type].Cast<Equipment>());
                        break;
                    case StorableObjectType.Material:
                        materialDataAccess.Update(splitedArray[type].Cast<MaterialPiece>());
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public void UpdateLocation(IEnumerable<IStorableObject> objectsToUpdate, int newLocationId)
        {
            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = "UPDATE public.storable_object SET location_id = @newLocationId WHERE id = ANY(@ids)";
            command.Parameters.AddWithValue("@newLocationId", newLocationId);

            var ids = objectsToUpdate.Select(o => o.Id).ToArray();
            command.Parameters.AddWithValue("@ids", ids);

            command.ExecuteNonQuery();

            ConnectionPool.ReleaseConnection(connection);
        }

        public Dictionary<string, List<string>> SelectDistinct<T>(IEnumerable<string>? propertyToSelect = null) where T : IStorableObject
        {
            return DataAccess<T>().SelectDistinct(propertyToSelect);
        }

        public IStorableObjectDataAccess<T> DataAccess<T>() where T : IStorableObject
        {
            if (typeof(T) == typeof(Equipment))
            {
                return (IStorableObjectDataAccess<T>)new EquipmentDataAccess();
            }

            if (typeof(T) == typeof(MaterialPiece))
            {
                return (IStorableObjectDataAccess<T>)new MaterialDataAccess();
            }
            throw new NotImplementedException();
        }

    }
}
