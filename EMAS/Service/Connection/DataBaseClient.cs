using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service.Connection.DataAccess;
using EMAS.Service.Connection.DataAccess.Event;
using EMAS.Service.Connection.DataAccess.Interface;
using System.Diagnostics;

namespace EMAS.Service.Connection
{
    public class DataBaseClient
    {
        private DeliveryDataAccess deliveryDataAccess;
        private StorableObjectDataAccess storableObjectDataAccess;
        private ReservationDataAccess reservationDataAccess;
        private ISimpleDataAccess<Employee> employeeDataAccess;
        private ISimpleDataAccess<Location> locationDataAccess;
        private HistoryEntryDataAccess historyEntryDataAccess;
        private long lastEventId;
        public event Action<List<StorableObjectEvent>> NewEventsOccured;
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
        private EventDataAccess eventDataAccess;
        private static DataBaseClient instance;

        private DataBaseClient()
        {
            deliveryDataAccess = new DeliveryDataAccess();
            storableObjectDataAccess = new StorableObjectDataAccess();
            reservationDataAccess = new ReservationDataAccess();
            employeeDataAccess = new EmployeeDataAccess();
            locationDataAccess = new LocationDataAccess();
            eventDataAccess = new();
        }

        public static DataBaseClient GetInstance()
        {
            instance ??= new DataBaseClient();
            return instance;
        }

        public void Add(IStorableObject storableObject, int locationId)
        {
            storableObjectDataAccess.Add(storableObject, locationId);
            return;
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

            if (objectToAdd is Delivery newDelivery)
            {
                deliveryDataAccess.Add(newDelivery);
                return;
            }

            if (objectToAdd is Reservation newReservation)
            {
                reservationDataAccess.Add(newReservation);
                return;
            }

            throw new NotSupportedException("Этот тип не поддерживается");
        }

        public void Add(object[] objectToAdd)
        {
            if (objectToAdd is Employee[] newEmployee)
            {
                employeeDataAccess.Add(newEmployee);
                return;
            }

            if (objectToAdd is Location[] newLocation)
            {
                locationDataAccess.Add(newLocation);
                return;
            }

            if (objectToAdd is Delivery[] newDelivery)
            {
                deliveryDataAccess.Add(newDelivery);
                return;
            }

            if (objectToAdd is Reservation[] newReservation)
            {
                reservationDataAccess.Add(newReservation);
                return;
            }
            throw new NotSupportedException("Этот тип не поддерживается");
        }

        public void Update(object objectToUpdate)
        {
            if (objectToUpdate is Delivery newDelivery)
            {
                deliveryDataAccess.Update(newDelivery);
                return;
            }
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

            foreach (var objectLastEventPair in eventDataAccess.SelectLastEventsForStorableObject(storableObjects))
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

        public void Complete(object objecToComplete)
        {
            if (objecToComplete is Delivery completedDelivery)
            {
                deliveryDataAccess.Complete(completedDelivery);
                return;
            }
            if (objecToComplete is Reservation completedReservation)
            {
                reservationDataAccess.Complete(completedReservation);
                return;
            }
            throw new NotSupportedException("Этот тип не поддерживается");
        }

        public List<Delivery> GetDeliverysOutOf(int locationId)
        {
            return deliveryDataAccess.SelectDeliveryOutOf(locationId);
        }

        public List<IStorableObject> SelectStorableObjectOn(int locationId)
        {
            return new(storableObjectDataAccess.SelectOnLocation(locationId));
        }

        public List<Reservation> GetReservationOn(int locationId)
        {
            return reservationDataAccess.SelectOnLocation(locationId);
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

        public List<HistoryEntry> SelectHistoryForObject(IStorableObject storableObject)
        {
            return historyEntryDataAccess.SelectByEquipmentId(id);
        }

        public void SyncData(List<Location> locationsToSync)
        {
            if (IsDataUpToDate(out long lastDataBaseEventId))
                return;

            List<StorableObjectEvent> newStorableObjectEvents = eventDataAccess.SelectEventsAfter(LastEventId).ToList();
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
            Reservation completedReservation = reservationDataAccess.SelectCompletedByEndId(newStorableObjectEvent.Id);
            locationIdDictionary[completedReservation.LocationId].Reservations.RemoveAll(reservation => reservation.Id == completedReservation.Id);

            foreach (var storableObject in newStorableObjectEvent.ObjectsInEvent)
            {
                if (storableObject is Equipment equipment)
                {
                    locationIdDictionary[completedReservation.LocationId].StorableObjectsList.Add(equipment);
                }
            }
        }

        private void HandleReservedEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
        {
            var Reservation = reservationDataAccess.SelectById(newStorableObjectEvent.Id);
            locationIdDictionary[Reservation.LocationId].Reservations.Add(Reservation);

            foreach (var storableObject in newStorableObjectEvent.ObjectsInEvent)
            {
                if (storableObject is Equipment equipmentInEvent)
                {
                    locationIdDictionary[Reservation.LocationId].StorableObjectsList.RemoveAll(equipmentOnLocation => equipmentOnLocation.Id == equipmentInEvent.Id);
                }
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
                if (storableObject is Equipment equipmentInEvent)
                    locationIdDictionary[additionEvent.LocationId].StorableObjectsList.Add(equipmentInEvent);
            }
        }

        private void HandleSentEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
        {
            var newDelivery = deliveryDataAccess.SelectById(newStorableObjectEvent.Id);
            locationIdDictionary[newDelivery.DepartureId].OutgoingDeliveries.Add(newDelivery);
            locationIdDictionary[newDelivery.DestinationId].IncomingDeliveries.Add(newDelivery);

            foreach (var storableObject in newStorableObjectEvent.ObjectsInEvent)
            {
                if (storableObject is Equipment equipmentInEvent)
                {
                    locationIdDictionary[newDelivery.DepartureId].StorableObjectsList.RemoveAll(equipmentOnLocation => equipmentOnLocation.Id == equipmentInEvent.Id);
                }
            }
        }

        private void HandleArrivalEvent(Dictionary<int, Location> locationIdDictionary, StorableObjectEvent newStorableObjectEvent)
        {
            Delivery arrivedDelivery = deliveryDataAccess.SelectCompletedByArrivalId(newStorableObjectEvent.Id);
            locationIdDictionary[arrivedDelivery.DepartureId].OutgoingDeliveries.RemoveAll(delivery => delivery.Id == arrivedDelivery.Id);
            locationIdDictionary[arrivedDelivery.DestinationId].IncomingDeliveries.RemoveAll(delivery => delivery.Id == arrivedDelivery.Id);

            foreach (var storableObject in newStorableObjectEvent.ObjectsInEvent)
            {
                if (storableObject is Equipment equipment)
                {
                    locationIdDictionary[arrivedDelivery.DestinationId].StorableObjectsList.Add(equipment);
                }
            }
        }

        private bool IsDataUpToDate(out long lastDataBaseEventId)
        {
            StorableObjectEvent? lastEvent = eventDataAccess.SelectLast();

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
