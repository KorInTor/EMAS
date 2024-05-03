using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service.Connection.DataAccess.Interface;

namespace EMAS.Service.Connection.DataAccess
{
    public class StorableObjectDataAccess : IStorableObjectDataAccess<IStorableObject>
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

        public void Add(IEnumerable<IStorableObject> objectsToAdd)
        {
            var splitedArray = SplitByTypes(objectsToAdd);
            
            equipmentDataAccess.Add(splitedArray.Item1);
            //materialDataAccess.Add(splitedArray.Item2);

            EventDataAccess eventDataAccess = new();
            AdditionEvent newEvent = new(SessionManager.UserId, 0, EventType.Addition, DateTime.Now, new List<IStorableObject>(objectsToAdd), objectsToAdd.First().LocationId);
            eventDataAccess.Add(newEvent);
        }

        public void Add(IStorableObject objectToAdd)
        {
            if (objectToAdd is Equipment equipment)
                equipmentDataAccess.Add(equipment);
            else if (objectToAdd is MaterialPiece materialPiece)
                materialDataAccess.Add(materialPiece);
            else
                throw new NotImplementedException("DataAccess не реализован");

            EventDataAccess eventDataAccess = new();
            AdditionEvent newEvent = new(SessionManager.UserId, 0, EventType.Addition, DateTime.Now, [objectToAdd], objectToAdd.LocationId);
            eventDataAccess.Add(newEvent);
        }

        public IStorableObject? SelectById(int id)
        {
            IStorableObject? founded = equipmentDataAccess.SelectById(id);
            founded ??= materialDataAccess.SelectById(id);
            return founded;
        }

        public IEnumerable<IStorableObject> SelectByIds(IEnumerable<int> ids)
        {
            List<IStorableObject> foundedObjects = [];
            foundedObjects.AddRange(equipmentDataAccess.SelectByIds(ids));
            //foundedObjects.AddRange(materialDataAccess.SelectByIds(ids));
            return [.. foundedObjects];
        }

        public IEnumerable<IStorableObject> SelectOnLocation(int locationId)
        {
            List<IStorableObject> foundedObjects = [];
            foundedObjects.AddRange(equipmentDataAccess.SelectOnLocation(locationId));
            //foundedObjects.AddRange(materialDataAccess.SelectOnLocation(locationId));
            return [..foundedObjects];
        }

        public void Update(IEnumerable<IStorableObject> objectsToUpdate)
        {
            var splitedArray = SplitByTypes(objectsToUpdate);

            equipmentDataAccess.Update(splitedArray.Item1);
            //materialDataAccess.Update(splitedArray.Item2);
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
            if (objectToUpdate is Equipment equipment)
                equipmentDataAccess.UpdateLocation(equipment, newLocationId);
            else if (objectToUpdate is MaterialPiece materialPiece)
                materialDataAccess.UpdateLocation(materialPiece, newLocationId);
            else
                throw new NotImplementedException("DataAccess не реализован");
        }

        public void UpdateLocation(IEnumerable<IStorableObject> objectsToUpdate, int newLocationId)
        {
            var splitedArray = SplitByTypes(objectsToUpdate);

            equipmentDataAccess.UpdateLocation(splitedArray.Item1, newLocationId);
            //materialDataAccess.UpdateLocation(splitedArray.Item2, newLocationId);
        }

        private ValueTuple<List<Equipment>, List<MaterialPiece>> SplitByTypes(IEnumerable<IStorableObject> storableObjects)
        {
            List<Equipment> equipmentList = [];
            List<MaterialPiece> materialList = [];

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
            return (equipmentList, materialList);
        }
    }
}
