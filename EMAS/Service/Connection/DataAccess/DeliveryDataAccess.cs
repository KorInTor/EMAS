using DocumentFormat.OpenXml.Spreadsheet;
using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class DeliveryDataAccess : IObjectStateLocationBoundedDataAccess<Delivery>
    {
        private readonly EventDataAccess eventAccess = new();
        private readonly string schemaName = "\"event\"";
        private readonly string tableName = "delivery";
        private string FullTableName => schemaName + tableName;
        public void Add(Delivery newDelivery)
        {
            Add([newDelivery]);
        }

        public void Delete(Delivery objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Delivery> Select()
        {
            throw new NotImplementedException();
        }

        public Delivery SelectById(int id)
        {
            return SelectById(id);
        }

        public void Update(Delivery objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public List<Delivery> SelectOnLocation(int locationId)
        {
            var equipmentAccess = new EquipmentDataAccess();
            var deliveries = new List<Delivery>();

            string query = "SELECT E.date, EqEvent.equipment_id, D.destination_id, D.dispatch_event_id " +
                                "FROM \"event\".delivery AS D " +
                                "JOIN public.\"event\" AS E ON E.id = D.dispatch_event_id " +
                                "JOIN public.equipment_event AS EqEvent ON EqEvent.event_id = D.dispatch_event_id " +
                                "WHERE D.arrival_event_id IS NULL AND D.departure_id = @locationId ";


            var connection = ConnectionPool.GetConnection();

            using var cmd = new NpgsqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@locationId", locationId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                deliveries.Add(new Delivery(reader.GetInt64(3), locationId, reader.GetInt32(2), reader.GetDateTime(0), equipmentAccess.SelectById(reader.GetInt32(1))));
                Debug.WriteLine($"Получили delivery идущее ИЗ Объекта с id - {locationId}");
            }

            ConnectionPool.ReleaseConnection(connection);

            return deliveries;
        }

        public void Complete(Delivery completedDelivery)
        {
            Complete([completedDelivery]);
        }

        public void AddOnLocation(Delivery item, int locationId)
        {
            Add(item);
        }

        public void Add(Delivery[] objectToAdd)
        {
            string query = "INSERT INTO " + FullTableName + " (dispatch_event_id, destination_id, departure_id) VALUES(@dispatch_event_id, @destination_id, @departure_id);";
            var connection = ConnectionPool.GetConnection();

            foreach (var delivery in objectToAdd)
            {
                Event newEvent = new(SessionManager.UserId, 0, EventType.Sent, delivery.PackageList.Id);

                eventAccess.Add(newEvent);

                delivery.Id = newEvent.Id;
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@dispatch_event_id", delivery.Id);
                command.Parameters.AddWithValue("@destination_id", delivery.DestinationId);
                command.Parameters.AddWithValue("@departure_id", delivery.DepartureId);

                command.ExecuteNonQuery();
            }
            
            Debug.WriteLine("Успешно добавлена доставка");

            ConnectionPool.ReleaseConnection(connection);
        }

        public void Delete(Delivery[] objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Update(Delivery[] objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void AddOnLocation(Delivery[] item, int locationId)
        {
            throw new NotImplementedException();
        }

        public void Complete(Delivery[] objectToComplete)
        {
            foreach (var delivery in objectToComplete)
            {
                Event newEvent = new(SessionManager.UserId, 0, EventType.Arrived, delivery.PackageList.Id);

                eventAccess.Add(newEvent);

                var equipmentAccess = new EquipmentDataAccess();
                equipmentAccess.UpdateLocation((delivery.PackageList, delivery.DestinationId));


                var connection = ConnectionPool.GetConnection();
                using var command = new NpgsqlCommand("UPDATE" + FullTableName + "SET arrival_event_id=@arrival_event_id, WHERE dispatch_event_id=@sendedEventId ", connection);
                command.Parameters.AddWithValue("@arrival_event_id", newEvent.Id);
                command.Parameters.AddWithValue("@sendedEventId", delivery.Id);

                command.ExecuteNonQuery();

                ConnectionPool.ReleaseConnection(connection);
            }
        }

        public Delivery SelectById(long Id)
        {
            throw new NotImplementedException();
        }
    }
}
