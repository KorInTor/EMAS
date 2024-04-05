using DocumentFormat.OpenXml.Spreadsheet;
using EMAS.Model;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class DeliveryDataAccess : IDataAccess<Delivery>
    {
        private readonly EventDataAccess eventAccess = new();

        public void Add(Delivery newDelivery)
        {
            eventAccess.Add((SessionManager.UserId, EventDataAccess.EventTypes["Sent"], newDelivery.Equipment.Id));

            newDelivery.EventDispatchId = eventAccess.LastEventId;

            var connection = ConnectionPool.GetConnection();

            string query = "INSERT INTO \"event\".delivery (dispatch_event_id, destination_id, departure_id) VALUES(@dispatch_event_id, @destination_id, @departure_id);";
            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@dispatch_event_id", newDelivery.EventDispatchId);
            command.Parameters.AddWithValue("@destination_id", newDelivery.DestinationId);
            command.Parameters.AddWithValue("@departure_id", newDelivery.DepartureId);

            command.ExecuteNonQuery();

            Debug.WriteLine("Успешно добавлена доставка");

            ConnectionPool.ReleaseConnection(connection);
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
            throw new NotImplementedException();
        }

        public void Update(Delivery objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public List<Delivery> SelectDeliveryOutOf(int locationId)
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
            eventAccess.Add((SessionManager.UserId, EventDataAccess.EventTypes["Arrival"], completedDelivery.Equipment.Id));

            var equipmentAccess = new EquipmentDataAccess();
            equipmentAccess.UpdateLocation((completedDelivery.Equipment,completedDelivery.DestinationId));

            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand("UPDATE \"event\".delivery SET arrival_event_id=@arrival_event_id, WHERE dispatch_event_id=@sendedEventId ", connection);
            command.Parameters.AddWithValue("@arrival_event_id", eventAccess.LastEventId);
            command.Parameters.AddWithValue("@sendedEventId", completedDelivery.EventDispatchId);

            command.ExecuteNonQuery();

            ConnectionPool.ReleaseConnection(connection);
        }
    }
}
