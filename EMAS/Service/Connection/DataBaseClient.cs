using EMAS.Exceptions;
using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Model.HistoryEntry;
using EMAS.Service.Connection.DataAccess;
using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace EMAS.Service.Connection
{
    public class DataBaseClient
    {
        private DeliveryDataAccess deliveryDataAccess;
        private ILocationBoundedDataAccess<Equipment> equipmentDataAccess;
        private IObjectStateLocationBoundedDataAccess<Reservation> reservationDataAccess;
        private IDataAccess<Employee> employeeDataAccess;
        private IDataAccess<Location> locationDataAccess;
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
            equipmentDataAccess = new EquipmentDataAccess();
            reservationDataAccess = new ReservationDataAccess();
            employeeDataAccess = new EmployeeDataAccess();
            locationDataAccess = new LocationDataAccess();
            historyEntryDataAccess = new HistoryEntryDataAccess();
            eventDataAccess = new();
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

        public void AddOnLocation(ILocationBounded objectToAdd, int locationId)
        {
            if (objectToAdd is Equipment newEquipment)
            {
                equipmentDataAccess.AddOnLocation(newEquipment, locationId);
                return;
            }
            if (objectToAdd is Reservation newReservation)
            {
                reservationDataAccess.AddOnLocation(newReservation, locationId);
                return;
            }
            throw new NotSupportedException("Этот тип не поддерживается");
        }

        public void AddOnLocation(ILocationBounded[] objectToAdd, int locationId)
        {
            if (objectToAdd is Equipment[] newEquipment)
            {
                equipmentDataAccess.AddOnLocation(newEquipment, locationId);
                return;
            }
            if (objectToAdd is Reservation[] newReservation)
            {
                reservationDataAccess.AddOnLocation(newReservation, locationId);
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
                equipmentDataAccess.Update(newEquipment);
                return;
            }
            if (objectToUpdate is Reservation newReservation)
            {
                reservationDataAccess.Update(newReservation);
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

        public void Complete(object objecToComplete)
        {
            if(objecToComplete is Delivery completedDelivery)
            {
                deliveryDataAccess.Complete(completedDelivery);
                return;
            }
            throw new NotSupportedException("Этот тип не поддерживается");
        }

        public List<Delivery> GetDeliverysOutOf(int locationId)
        {
            return deliveryDataAccess.SelectOnLocation(locationId);
        }

        public List<Equipment> SelectEquipmentOn(int locationId)
        {
            return equipmentDataAccess.SelectOnLocation(locationId);
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

        public Dictionary<int,string> SelectNamedLocations()
        {
            Dictionary<int, string> namedLocations = [];
            foreach (Location location in SelectLocations())
            {
                namedLocations.Add(location.Id, location.Name);
            }
            return namedLocations;
        }

        public List<HistoryEntryBase> SelectHistoryEntryByEquipmentId(int id)
        {
            return historyEntryDataAccess.SelectByEquipmentId(id);
        }

        public void SyncData(List<Location> locationsToSync)
        {
            long lastDataBaseEventId;
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
                return;
            }
            Debug.WriteLine("UpdatingData");

            List<StorableObjectEvent> newSOEvents = eventDataAccess.SelectEventsAfter(LastEventId);
            LastEventId = lastDataBaseEventId;

            List<IObjectState> objectStates = [];
            foreach (var location in locationsToSync)
            {
                objectStates.AddRange(location.OutgoingDeliveries);
            }

            var locationIdDictionary = new Dictionary<int, Location>();
            foreach (var location in locationsToSync)
            {
                locationIdDictionary.Add(location.Id,location);
            }

            foreach (var newSOEvent in newSOEvents)
            {
                switch (newSOEvent.EventType)
                {
                    case EventType.Arrived:
                        {
                            Delivery arrivedDelivery = deliveryDataAccess.SelectCompletedByArrivalId(newSOEvent.Id);
                            locationIdDictionary[arrivedDelivery.DepartureId].OutgoingDeliveries.RemoveAll(delivery => delivery.Id == arrivedDelivery.Id);
                            locationIdDictionary[arrivedDelivery.DestinationId].IncomingDeliveries.RemoveAll(delivery => delivery.Id == arrivedDelivery.Id);

                            foreach (var storableObject in newSOEvent.ObjectsInEvent)
                            {
                                if (storableObject is Equipment equipment)
                                {
                                    locationIdDictionary[arrivedDelivery.DestinationId].Equipments.Add(equipment);
                                }
                            }
                            break;
                        }
                    case EventType.Sent:
                        {
                            var newDelivery = deliveryDataAccess.SelectById(newSOEvent.Id);
                            locationIdDictionary[newDelivery.DepartureId].OutgoingDeliveries.Add(newDelivery);
                            locationIdDictionary[newDelivery.DestinationId].IncomingDeliveries.Add(newDelivery);

                            foreach (var storableObject in newSOEvent.ObjectsInEvent)
                            {
                                if (storableObject is Equipment equipmentInEvent)
                                {
                                    locationIdDictionary[newDelivery.DepartureId].Equipments.RemoveAll(equipmentOnLocation => equipmentOnLocation.Id == equipmentInEvent.Id);
                                }
                            }
                            break;
                        }
                    case EventType.Decommissioned:
                        {
                            throw new NotImplementedException();
                        }
                    case EventType.Reserved:
                        {
                            throw new NotImplementedException();
                            break;
                        }
                    case EventType.ReserveEnded:
                        {
                            throw new NotImplementedException();
                            break;
                        }
                    case EventType.Addition:
                        {
                            AdditionEvent additionEvent = (AdditionEvent)newSOEvent;
                            foreach (var storableObject in additionEvent.ObjectsInEvent)
                            {
                                if (storableObject is Equipment equipmentInEvent)
                                    locationIdDictionary[additionEvent.LocationId].Equipments.Add(equipmentInEvent);
                            }
                            break;
                        }
                    case EventType.DataChanged:
                        {
                            foreach (var storableObject in newSOEvent.ObjectsInEvent)
                            {
                                if (storableObject is Equipment equipmentInEvent)
                                {
                                    locationIdDictionary[equipmentInEvent.LocationId].Equipments.RemoveAll(equipmentOnLocation => equipmentOnLocation.Id == equipmentInEvent.Id);
                                    locationIdDictionary[equipmentInEvent.LocationId].Equipments.Add(equipmentInEvent);
                                }
                            }
                            break;
                        }
                    default:
                        {
                            throw new NotImplementedException();
                        }
                }
            }
            NewEventsOccured?.Invoke(newSOEvents);
        }
    }
}
