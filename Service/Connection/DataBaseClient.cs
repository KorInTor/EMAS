using Model;
using Model.Event;
using Service.Connection.DataAccess;
using Service.Connection.DataAccess.Event;
using Service.Connection.DataAccess.Interface;
using Service.Connection.DataAccess.QueryBuilder;
using System.Diagnostics;

namespace Service.Connection
{
    public class DataBaseClient
    {
        private StorableObjectDataAccess storableObjectDataAccess;
        private ISimpleDataAccess<Employee> employeeDataAccess;
        private ISimpleDataAccess<Location> locationDataAccess;
        private long lastEventId;
        public event Action<List<StorableObjectEvent>> NewEventsOccured;
        private EventDataAccess eventDataAccess;

        public long LastEventId
        {
            get
            {
                return lastEventId;
            }
            private set
            {
                lastEventId = value;
            }
        }

        private static DataBaseClient instance;

        private DataBaseClient()
        {
            storableObjectDataAccess = new StorableObjectDataAccess();
            employeeDataAccess = new EmployeeDataAccess();
            locationDataAccess = new LocationDataAccess();
            eventDataAccess = new EventDataAccess();
        }

        public static DataBaseClient GetInstance()
        {
            instance ??= new DataBaseClient();
            return instance;
        }

        public void Add(object objectToAdd)
        {
            if (objectToAdd is Employee newEmployee)
            {
                employeeDataAccess.Add(newEmployee);
                return;
            }

            if (objectToAdd is Location newLocation)
            {
                locationDataAccess.Add(newLocation);
                return;
            }

            if (objectToAdd is StorableObjectEvent objectEvent)
            {
                eventDataAccess.Add(objectEvent);
                return;
            }

            throw new NotSupportedException("Этот тип не поддерживается");
        }

        public void Add(IEnumerable<object> objectToAdd)
        {
            if (objectToAdd is IEnumerable<Employee> newEmployee)
            {
                employeeDataAccess.Add(newEmployee.ToArray());
                return;
            }

            if (objectToAdd is IEnumerable<Location> newLocation)
            {
                locationDataAccess.Add(newLocation.ToArray());
                return;
            }

            if (objectToAdd is IEnumerable<StorableObjectEvent> newEvents)
            {
                eventDataAccess.Add(newEvents);
                return;
            }

            throw new NotSupportedException("Этот тип не поддерживается");
        }

        public void Update(object objectToUpdate)
        {
            if (objectToUpdate is Equipment newEquipment)
            {
                storableObjectDataAccess.Update(newEquipment);
                return;
            }
            if (objectToUpdate is Employee newEmployee)
            {
                employeeDataAccess.Update(newEmployee);
                return;
            }
            if (objectToUpdate is Location newLocation)
            {
                locationDataAccess.Update(newLocation);
                return;
            }
            throw new NotSupportedException("Этот тип не поддерживается");
        }

        public bool IsStorableObjectsNotOccupied(IStorableObject[] storableObjects, out List<IStorableObject> occupiedObject)
        {
            occupiedObject = [];

            foreach (var objectLastEventPair in eventDataAccess.SelectLastEventsForStorableObjects(storableObjects))
            {
                if (objectLastEventPair.Value.EventType == EventType.Sent || objectLastEventPair.Value.EventType == EventType.Reserved || objectLastEventPair.Value.EventType == EventType.Decommissioned)
                {
                    occupiedObject.Add(objectLastEventPair.Key);
                }
            }

            if (occupiedObject.Count == 0)
                return true;
            else
                return false;
        }

        public List<SentEvent> GetDeliverysOutOf(int locationId)
        {
            return eventDataAccess.SelectActiveDeliveriesOutOfLocation(locationId);
        }

        public List<IStorableObject> SelectStorableObjectOn(int locationId)
        {
            return storableObjectDataAccess.SelectOnLocation(locationId).ToList();
        }

        public List<ReservedEvent> GetReservationOn(int locationId)
        {
            var condition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<ReservedEvent>(x => x.LocationId), Comparison.Equal,locationId);
            return (List<ReservedEvent>)Task.Run(() => eventDataAccess.SelectAsync([condition],typeof(ReservedEvent))).Result;
        }

        public List<Employee> SelectEmployee()
        {
            return employeeDataAccess.Select();
        }

        public List<Location> SelectLocations()
        {
            return locationDataAccess.Select();
        }

        public Dictionary<int, string> SelectNamedLocations()
        {
            Dictionary<int, string> namedLocations = [];
            foreach (Location location in SelectLocations())
            {
                namedLocations.Add(location.Id, location.Name);
            }
            return namedLocations;
        }

        public List<StorableObjectEvent> SelectForStorableObjectId(int storableObjectId)
        {
            return eventDataAccess.SelectEventsForStorableObject(storableObjectId).ToList();
        }

        public void SyncData(List<Location> locationsToSync)
        {
            if (IsDataUpToDate(out long lastDataBaseEventId))
                return;


            var condition = new CompareCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.Id), Comparison.GreaterThan, LastEventId);

            List<StorableObjectEvent> newStorableObjectEvents = eventDataAccess.Select([condition]).ToList();

            LastEventId = lastDataBaseEventId;

            var locationIdDictionary = new Dictionary<int, Location>();
            foreach (var location in locationsToSync)
            {
                locationIdDictionary.Add(location.Id, location);
            }

            var eventHandlers = new Dictionary<EventType, Action<Dictionary<int, Location>, StorableObjectEvent>>
            {
                { EventType.Addition, HandleAdditionEvent},
                { EventType.Arrived, HandleArrivalEvent},
                { EventType.Sent, HandleSentEvent},
                { EventType.DataChanged, HandleDataChangedEvent},
                { EventType.Decommissioned, HandleDecomissionedEvent},
                { EventType.Reserved, HandleReservedEvent},
                { EventType.ReserveEnded, HandleReserveEndedEvent},
            };

            foreach (var newStorableObjectEvent in newStorableObjectEvents)
            {
                eventHandlers[newStorableObjectEvent.EventType].Invoke(locationIdDictionary, newStorableObjectEvent);
            }

            NewEventsOccured?.Invoke(newStorableObjectEvents);
        }

        private void HandleReserveEndedEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
        {
            ReserveEndedEvent completedReservation = (ReserveEndedEvent)newStorableObjectEvent;

            ReservedEvent? reservedEvent = null;

            foreach (var location in locationIdDictionary.Values)
            {
                reservedEvent = location.Reservations.Where(reservation => reservation.Id == completedReservation.ReserveEventId).FirstOrDefault();

                location.Reservations.RemoveAll(reservation => reservation.Id == completedReservation.ReserveEventId);

                if (reservedEvent != null)
                    break;
            }

            locationIdDictionary[reservedEvent.LocationId].StorableObjectsList.AddRange(newStorableObjectEvent.ObjectsInEvent);
        }

        private void HandleReservedEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
        {
            ReservedEvent Reservation = (ReservedEvent)newStorableObjectEvent;

            locationIdDictionary[Reservation.LocationId].Reservations.Add(Reservation);

            foreach (var storableObjectInEvent in newStorableObjectEvent.ObjectsInEvent)
            {
                locationIdDictionary[Reservation.LocationId].StorableObjectsList.RemoveAll(StorableObjectOnLocation => StorableObjectOnLocation.Id == storableObjectInEvent.Id);
            }
        }

        private void HandleDecomissionedEvent(Dictionary<int, Location> dictionary, StorableObjectEvent @event)
        {
            throw new NotImplementedException();
        }

        private static void HandleDataChangedEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
        {
            int locationChangedId = 0;
            foreach (var location in locationIdDictionary.Values)
            {
                foreach (var storableObjectInEvent in newStorableObjectEvent.ObjectsInEvent)
                {
                    foreach (var storableObjectInLocation in location.StorableObjectsList)
                    {
                        if (storableObjectInEvent.Id == storableObjectInLocation.Id)
                        {
                            locationChangedId = location.Id;
                            break;
                        }
                    }
                    if (locationChangedId != 0)
                        break;
                }
                if (locationChangedId != 0)
                    break;
            }


            foreach (var storableObject in newStorableObjectEvent.ObjectsInEvent)
            {
                if (storableObject is Equipment equipmentInEvent)
                {
                    locationIdDictionary[locationChangedId].StorableObjectsList.RemoveAll(equipmentOnLocation => equipmentOnLocation.Id == equipmentInEvent.Id);
                    locationIdDictionary[locationChangedId].StorableObjectsList.Add(equipmentInEvent);
                }
            }
        }

        private static void HandleAdditionEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
        {
            AdditionEvent additionEvent = (AdditionEvent)newStorableObjectEvent;
            foreach (var storableObject in additionEvent.ObjectsInEvent)
            {
                locationIdDictionary[additionEvent.LocationId].StorableObjectsList.AddRange(additionEvent.ObjectsInEvent);
            }
        }

        private void HandleSentEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
        {
            SentEvent newDelivery = (SentEvent)newStorableObjectEvent;

            locationIdDictionary[newDelivery.DepartureId].OutgoingDeliveries.Add(newDelivery);
            locationIdDictionary[newDelivery.DestinationId].IncomingDeliveries.Add(newDelivery);

            foreach (var storableObject in newStorableObjectEvent.ObjectsInEvent)
            {
                locationIdDictionary[newDelivery.DepartureId].StorableObjectsList.RemoveAll(objectOnLocation => objectOnLocation.Id == storableObject.Id);
            }
        }

        private void HandleArrivalEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
        {
            ArrivedEvent arrivedDelivery = (ArrivedEvent)newStorableObjectEvent;

            SentEvent completedDelivery = null;

            foreach (var location in locationIdDictionary.Values)
            {
                completedDelivery = location.OutgoingDeliveries.Where(delivery => delivery.Id == arrivedDelivery.SentEventId).FirstOrDefault();
                if (completedDelivery != null)
                    break;
            }
            

            locationIdDictionary[completedDelivery.DepartureId].OutgoingDeliveries.RemoveAll(delivery => delivery.Id == completedDelivery.Id);
            locationIdDictionary[completedDelivery.DestinationId].IncomingDeliveries.RemoveAll(delivery => delivery.Id == completedDelivery.Id);

            locationIdDictionary[completedDelivery.DestinationId].StorableObjectsList.AddRange(newStorableObjectEvent.ObjectsInEvent);
        }

        private bool IsDataUpToDate(out long lastDataBaseEventId)
        {
            var condition = new MaxCondition(SelectQueryBuilder.GetFullPropertyName<StorableObjectEvent>(x => x.Id));

            StorableObjectEvent? lastEvent = eventDataAccess.Select([condition]).First();

            if (lastEvent is null)
            {
                lastDataBaseEventId = 0;
            }
            else
            {
                lastDataBaseEventId = lastEvent.Id;
            }

            if (lastDataBaseEventId == LastEventId)
            {
                Debug.WriteLine("Data is UpToDate");
                return true;
            }
            Debug.WriteLine("Data is OutDated");
            return false;
        }

        public bool IsDataUpToDate()
        {
            return IsDataUpToDate(out _);
        }
    }
}
