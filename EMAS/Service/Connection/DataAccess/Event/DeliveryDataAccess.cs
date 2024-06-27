using DocumentFormat.OpenXml.Spreadsheet;
using EMAS.Exceptions;
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

namespace EMAS.Service.Connection.DataAccess.Event
{
    public class DeliveryDataAccess
    {
        private readonly string schemaName = "\"event\".";
        private readonly string tableName = "delivery";
        public string FullTableName => schemaName + tableName;

        public void Add(SentEvent newDelivery)
        {
            Add([newDelivery]);
        }

        public void Add(SentEvent[] objectToAdd)
        {
            string query = "INSERT INTO " + FullTableName + " (dispatch_event_id, destination_id, departure_id ,dispatch_info) VALUES (@dispatch_event_id, @destination_id, @departure_id ,@dispatch_info); ";
            var connection = ConnectionPool.GetConnection();

            foreach (var sentEvent in objectToAdd)
            {
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@dispatch_event_id", sentEvent.Id);
                command.Parameters.AddWithValue("@destination_id", sentEvent.DestinationId);
                command.Parameters.AddWithValue("@departure_id", sentEvent.DepartureId);
                command.Parameters.AddWithValue("@dispatch_info", sentEvent.Comment);

                command.ExecuteNonQuery();
            }

            Debug.WriteLine("Успешно добавлено отправление");

            ConnectionPool.ReleaseConnection(connection);
        }

        public void Complete(ArrivedEvent completedDelivery)
        {
            Complete([completedDelivery]);
        }

        public void Complete(ArrivedEvent[] objectToComplete)
        {
            foreach (var arriveEvent in objectToComplete)
            {
                var connection = ConnectionPool.GetConnection();
                using var command = new NpgsqlCommand("UPDATE " + FullTableName + " SET arrival_event_id=@arrival_event_id, arrival_info=@arrivalComment WHERE dispatch_event_id=@sendedEventId ", connection);
                command.Parameters.AddWithValue("@arrival_event_id", arriveEvent.Id);
                command.Parameters.AddWithValue("@sendedEventId", arriveEvent.SentEventId);
                command.Parameters.AddWithValue("@arrivalComment", arriveEvent.Comment);

                command.ExecuteNonQuery();

                ConnectionPool.ReleaseConnection(connection);
            }
        }

        public bool IsCompleted(IEnumerable<ArrivedEvent> objectToComplete)
        {
            var connection = ConnectionPool.GetConnection();
            foreach (var arriveEvent in objectToComplete)
            {
                using var command = new NpgsqlCommand("SELECT arrival_event_id FROM" + FullTableName + " WHERE dispatch_event_id=@SentEventId ", connection);
                command.Parameters.AddWithValue("@SentEventId", arriveEvent.SentEventId);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        return true;
                    }
                }

            }

            ConnectionPool.ReleaseConnection(connection);
            return false;
        }

        /// <summary>
        /// Получает соединения из пула соединений, делает запрос к базе, инициализирует Delivery найденными значениями.
        /// </summary>
        /// <param name="id">Id поиска.</param>
        /// <returns>Возвращает инициализированные объект Delivery. Если не найдена доставка с заданным Id вовзращает null.</returns>
        public SentEvent? SelectById(StorableObjectEvent storableObjectEvent)
        {
            SentEvent? foundDelivery = null;

            string query = "SELECT D.departure_id, D.destination_id, D.dispatch_info " +
                                "FROM " + FullTableName + " AS D " +
                                "WHERE D.dispatch_event_id = @Id ";

            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", storableObjectEvent.Id);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                foundDelivery = new(storableObjectEvent, reader.GetString(2), reader.GetInt32(0), reader.GetInt32(1));
            }

            ConnectionPool.ReleaseConnection(connection);
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
                var foundedEvent = eventAccess.SelectById(reader.GetInt32(0));
                if (foundedEvent == null)
                    return null;
                foundDelivery = new(foundedEvent, reader.GetInt32(1), reader.GetInt32(2), reader.GetString(3));
            }

            ConnectionPool.ReleaseConnection(connection);
            return foundDelivery;
        }

        public List<SentEvent> SelectDeliveryOutOf(int locationId)
        {
            //TODO: Переработать чтобы инициализация был из StorableObjectEvent.
            var deliveries = new List<SentEvent>();

            string query = "SELECT E.date, D.destination_id, D.dispatch_event_id, D.dispatch_info " +
                                "FROM " + FullTableName + " AS D " +
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
