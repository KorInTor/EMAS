using DocumentFormat.OpenXml.Office2010.PowerPoint;
using EMAS.Exceptions;
using EMAS.Model;
using EMAS.Model.HistoryEntry;
using EMAS.Service.Connection.DataAccess;
using Npgsql;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace EMAS.Service.Connection
{
    public class DataBaseClient
    {
        private IEquipmentStateLocationBoundedDataAccess<Delivery> deliveryDataAccess;
        private ILocationBoundedDataAccess<Equipment> equipmentDataAccess;
        private IEquipmentStateLocationBoundedDataAccess<Reservation> reservationDataAccess;
        private IDataAccess<Employee> employeeDataAccess;
        private IDataAccess<Location> locationDataAccess;
        private HistoryEntryDataAccess historyEntryDataAccess;

        private static DataBaseClient instance;

        private DataBaseClient()
        {
            deliveryDataAccess = new DeliveryDataAccess();
            equipmentDataAccess = new EquipmentDataAccess();
            reservationDataAccess = new ReservationDataAccess();
            employeeDataAccess = new EmployeeDataAccess();
            locationDataAccess = new LocationDataAccess();
            historyEntryDataAccess = new HistoryEntryDataAccess();
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
    }
}
