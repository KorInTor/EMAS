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
    public class DeliveryDataAccess
    {
        private readonly EventDataAccess eventAccess = new();
        private readonly string schemaName = "\"event\".";
        private readonly StorableObjectInEventDataAccess storableObjectInEventDataAccess = new();
        private readonly string tableName = "delivery";
        public string FullTableName => schemaName + tableName;

        public void Add(Delivery newDelivery)
        {
            Add([newDelivery]);
        }

        public void Add(Delivery[] objectToAdd)
        {
            string query = "INSERT INTO " + FullTableName + " (dispatch_event_id, destination_id, departure_id ,dispatch_info) VALUES (@dispatch_event_id, @destination_id, @departure_id ,@dispatch_info); ";
            var connection = ConnectionPool.GetConnection();

            foreach (var delivery in objectToAdd)
            {
                StorableObjectEvent deliveryEvent = new(SessionManager.UserId, 0, EventType.Sent, delivery.DispatchDate, delivery.PackageList);

                eventAccess.Add(deliveryEvent);

                delivery.Id = deliveryEvent.Id;
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@dispatch_event_id", delivery.Id);
                command.Parameters.AddWithValue("@destination_id", delivery.DestinationId);
                command.Parameters.AddWithValue("@departure_id", delivery.DepartureId);
                command.Parameters.AddWithValue("@dispatch_info", delivery.DispatchComment);

                command.ExecuteNonQuery();
            }

            Debug.WriteLine("Успешно добавлена доставка");

            ConnectionPool.ReleaseConnection(connection);
        }

        public void AddOnLocation(Delivery item, int locationId)
        {
            Add(item);
        }

        public void AddOnLocation(Delivery[] item, int locationId)
        {
            throw new NotImplementedException();
        }

        public void Complete(Delivery completedDelivery)
        {
            Complete([completedDelivery]);
        }

        public void Complete(Delivery[] objectToComplete)
        {
            foreach (var delivery in objectToComplete)
            {
                if (!delivery.IsCompleted)
                    throw new InvalidOperationException("Доставка не заполнена нужными значениями, невозможно закончить.");

                StorableObjectEvent newEvent = new(SessionManager.UserId, 0, EventType.Arrived, delivery.ArrivalDate, delivery.PackageList);

                eventAccess.Add(newEvent);

                var equipmentAccess = new EquipmentDataAccess();
                foreach (IStorableObject storableObject in delivery.PackageList)
                {
                    if (storableObject is Equipment equipmentInDelivery)
                    {
                        equipmentAccess.UpdateLocation((equipmentInDelivery, delivery.DestinationId));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                }

                var connection = ConnectionPool.GetConnection();
                using var command = new NpgsqlCommand("UPDATE " + FullTableName + " SET arrival_event_id=@arrival_event_id, arrival_info=@arrivalComment WHERE dispatch_event_id=@sendedEventId ", connection);
                command.Parameters.AddWithValue("@arrival_event_id", newEvent.Id);
                command.Parameters.AddWithValue("@sendedEventId", delivery.Id);
                command.Parameters.AddWithValue("@arrivalComment", delivery.ArrivalComment);

                command.ExecuteNonQuery();

                ConnectionPool.ReleaseConnection(connection);
            }
        }

        public void Delete(Delivery objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Delete(Delivery[] objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Delivery> Select()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Получает соединения из пула соединений, делает запрос к базе, инициализирует Delivery найденными значениями.
        /// </summary>
        /// <param name="id">Id поиска.</param>
        /// <returns>Возвращает инициализированные объект Delivery. Если не найдена доставка с заданным Id вовзращает null.</returns>
        public Delivery? SelectById(long id)
        {
            StorableObjectEvent? disptach_event_info = eventAccess.SelectById(id);
            if (disptach_event_info == null)
            {
                return null;
            }

            Delivery? foundDelivery = null;

            string query = "SELECT D.departure_id, D.destination_id, D.dispatch_info " +
                                "FROM "+FullTableName+" AS D " +
                                "WHERE D.dispatch_event_id = @Id ";

            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id",id);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                foundDelivery = new(disptach_event_info,reader.GetInt32(0),reader.GetInt32(1),reader.GetString(2));
            }

            return foundDelivery;
        }

        /// <summary>
        /// Получает соединения из пула соединений, делает запрос к базе, инициализирует Delivery найденными значениями.
        /// </summary>
        /// <param name="id">Id события завершения доставки.</param>
        /// <returns>Возвращает инициализированные объект Delivery который уже завершён. Если не найдена доставка с заданным Id вовзращает null.</returns>
        public Delivery? SelectCompletedByArrivalId(long id)
        {
            Delivery? foundDelivery = null;

            string query = "SELECT D.dispatch_event_id, D.departure_id, D.destination_id, D.dispatch_info " +
                                "FROM " + FullTableName + " AS D " +
                                "WHERE D.arrival_event_id = @Id ";

            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", id);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                foundDelivery = new(eventAccess.SelectById(reader.GetInt32(0)), reader.GetInt32(1), reader.GetInt32(2), reader.GetString(3));
            }

            ConnectionPool.ReleaseConnection(connection);
            return foundDelivery;
        }

        public List<Delivery> SelectOnLocation(int locationId)
        {
            //TODO: Переработать чтобы инициализация был из StorableObjectEvent.
            var deliveries = new List<Delivery>();

            string query = "SELECT E.date, D.destination_id, D.dispatch_event_id, D.dispatch_info " +
                                "FROM "+FullTableName+" AS D " +
                                "JOIN public.\"event\" AS E ON E.id = D.dispatch_event_id " +
                                "WHERE D.arrival_event_id IS NULL AND D.departure_id = @locationId ";

            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@locationId", locationId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                deliveries.Add(new Delivery(reader.GetInt64(2), locationId, reader.GetInt32(1), reader.GetString(3), reader.GetDateTime(0), storableObjectInEventDataAccess.SelectObjectsInEvent(reader.GetInt64(2))));
                Debug.WriteLine($"Получили delivery идущее ИЗ Объекта с id - {locationId}");
            }

            ConnectionPool.ReleaseConnection(connection);

            return deliveries;
        }

        public void Update(Delivery objectToUpdate)
        {
            throw new NotImplementedException();
        }

        public void Update(Delivery[] objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
