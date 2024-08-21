using Model;
using Model.Enum;
using Model.Event;
using Service.Connection.DataAccess.Interface;
using Npgsql;

namespace Service.Connection.DataAccess
{
    public class StorableObjectDataAccess
    {
        private IStorableObjectDataAccess<Equipment> equipmentDataAccess;
        private IStorableObjectDataAccess<MaterialPiece> materialDataAccess;

        public StorableObjectDataAccess()
        {
            equipmentDataAccess = new EquipmentDataAccess();
            materialDataAccess = new MaterialDataAccess();
        }

        public StorableObjectDataAccess(IStorableObjectDataAccess<Equipment> customEquipmentDataAccess, IStorableObjectDataAccess<MaterialPiece> customMaterialDataAccess)
        {
            equipmentDataAccess = customEquipmentDataAccess;
            materialDataAccess = customMaterialDataAccess;
        }

        /// <summary>
        /// Добавляет в базу новый список объектов, подразумевается что все объекты должны быть добавлены на одну локацию.
        /// </summary>
        /// <param name="objectsToAdd">Список на добавление</param>
        public void Add(IEnumerable<IStorableObject> objectsToAdd, int locationId)
        {
            var splitedArray = SplitByTypes(objectsToAdd); //Разбиваем список по типам объектов

            var connection = ConnectionPool.GetConnection();
            string query = "INSERT INTO public.storable_object (type_id, location_id) VALUES (@typeID, @location_id) returning id";

            foreach (var storableObjectTypedList in splitedArray)
            {
                foreach(var storableObject in storableObjectTypedList.Value) //Добавляем в таблицу storable_object данные о типе объекта и возвращаем новый ID.
                {
                    using var command = new NpgsqlCommand(query, connection);
                    command.Parameters.AddWithValue("@typeID", (int)storableObjectTypedList.Key);
                    command.Parameters.AddWithValue("@location_id", locationId);

                    storableObject.Id = (int)command.ExecuteScalar();
                }

                //Добавление в таблицу явного типа объекта (eg. Equipment в public.equipment)

                if (storableObjectTypedList.Key == StorableObjectType.Equipment) 
                {
                    List<Equipment> equipmentList = new List<Equipment>();
                    foreach (var item in storableObjectTypedList.Value)
                    {
                        if (item is Equipment equipment)
                        {
                            equipmentList.Add(equipment);
                        }
                    }

                    equipmentDataAccess.Add(equipmentList);
                }

                if (storableObjectTypedList.Key == StorableObjectType.Material)
                {
                    List<MaterialPiece> materialList = new List<MaterialPiece>();
                    foreach (var item in storableObjectTypedList.Value)
                    {
                        if (item is MaterialPiece material)
                        {
                            materialList.Add(material);
                        }
                    }

                    materialDataAccess.Add(materialList);
                }
            }

            ConnectionPool.ReleaseConnection(connection);
        }

        public void Add(IStorableObject objectToAdd, int locationId)
        {
            Add([objectToAdd],locationId);
        }

        public IStorableObject? SelectById(int id)
        {
            return SelectByIds([id]).FirstOrDefault();
        }

        public IEnumerable<IStorableObject> SelectByIds(IEnumerable<int> ids)
        {
            var connection = ConnectionPool.GetConnection();

            string query = "SELECT id, type_id, location_id FROM public.storable_object WHERE id = @id;";

            Dictionary<StorableObjectType, List<int>> typesIdsDictionary = new Dictionary<StorableObjectType, List<int>>();

            foreach (int id in ids)
            {
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@id", id);

                using var reader = command.ExecuteReader();

                if (reader.Read())
                {
                    if (reader.GetInt32(1) == (int)StorableObjectType.Equipment)
                    {
                        if (!typesIdsDictionary.ContainsKey(StorableObjectType.Equipment))
                        {
                            typesIdsDictionary.Add(StorableObjectType.Equipment, new List<int>());
                        }
                        typesIdsDictionary[StorableObjectType.Equipment].Add(id);
                    }
                    if (reader.GetInt32(1) == (int)StorableObjectType.Material)
                    {
                        if (!typesIdsDictionary.ContainsKey(StorableObjectType.Material))
                        {
                            typesIdsDictionary.Add(StorableObjectType.Material, new List<int>());
                        }
                        typesIdsDictionary[StorableObjectType.Material].Add(id);
                    }
                }
            }

            ConnectionPool.ReleaseConnection(connection);

            List<IStorableObject> foundedObjects = new List<IStorableObject>();

            foreach (var type in typesIdsDictionary.Keys)
            {
                if (type == StorableObjectType.Equipment)
                {
                    foundedObjects.AddRange(equipmentDataAccess.SelectByIds(typesIdsDictionary[type]));
                }
                if (type == StorableObjectType.Material)
                {
                    foundedObjects.AddRange(materialDataAccess.SelectByIds(typesIdsDictionary[type]));
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
            var splitedArray = SplitByTypes(objectsToUpdate);

            foreach (var type in splitedArray.Keys)
            {
                if (type == StorableObjectType.Equipment)
                {
                    equipmentDataAccess.Update(TryDowncastToEquipment(splitedArray[type]));
                }
                if (type == StorableObjectType.Material)
                {
                    materialDataAccess.Update(TryDowncastToMaterial(splitedArray[type]));
                }
            }
        }

        public void Update(IStorableObject objectToUpdate)
        {
            if (objectToUpdate is Equipment equipment)
                equipmentDataAccess.Update(equipment);
            else if (objectToUpdate is MaterialPiece materialPiece)
                materialDataAccess.Update(materialPiece);
            else
                throw new NotImplementedException("DataAccess не реализован");
        }

        public void UpdateLocation(IStorableObject objectToUpdate, int newLocationId)
        {
            UpdateLocation([objectToUpdate], newLocationId);
        }

        public void UpdateLocation(IEnumerable<IStorableObject> objectsToUpdate, int newLocationId)
        {
            var connection = ConnectionPool.GetConnection();

            foreach (var storableObject in objectsToUpdate)
            {
                using var command = new NpgsqlCommand("UPDATE public.storable_object SET location_id=@newLocationId WHERE id=@id ", connection);
                command.Parameters.AddWithValue("@newLocationId", newLocationId);
                command.Parameters.AddWithValue("@id", storableObject.Id);

                command.ExecuteNonQuery();
            }

            ConnectionPool.ReleaseConnection(connection);
        }

        private Dictionary<StorableObjectType,List<IStorableObject>> SplitByTypes(IEnumerable<IStorableObject> storableObjects)
        {
            var SplittedArray = new Dictionary<StorableObjectType, List<IStorableObject>>();

            List<IStorableObject> equipmentList = [];
            List<IStorableObject> materialList = [];

            foreach(var storableObject in storableObjects)
            {
                if (storableObject is Equipment equipment)
                {
                    equipmentList.Add(equipment);
                    continue;
                }
                if (storableObject is MaterialPiece material)
                {
                    materialList.Add(material);
                    continue;
                }
                throw new NotImplementedException("Данный тип не поддерживается");
            }

            if (equipmentList.Count != 0)
            {
                SplittedArray.Add(StorableObjectType.Equipment, equipmentList);
            }
            if (materialList.Count != 0)
            {
                SplittedArray.Add(StorableObjectType.Material, materialList);
            }

            return SplittedArray;
        }
        
        private List<Equipment> TryDowncastToEquipment(List<IStorableObject> storableObjects)
        {
            List<Equipment> equipmentList = [];
            foreach (var storableObject in storableObjects)
            {
                if (storableObject is Equipment equipment)
                {
                    equipmentList.Add(equipment);
                }
                else
                    throw new ArgumentException("Cannot downcast to Equipment if not equipment");
            }
            return equipmentList;
        }

        private List<MaterialPiece> TryDowncastToMaterial(List<IStorableObject> storableObjects)
        {
            List<MaterialPiece> materialPieceList = [];
            foreach (var storableObject in storableObjects)
            {
                if (storableObject is MaterialPiece materialPiece)
                {
                    materialPieceList.Add(materialPiece);
                }
                else
                    throw new ArgumentException("Cannot downcast to Equipment if not equipment");
            }
            return materialPieceList;
        }
    }
}
