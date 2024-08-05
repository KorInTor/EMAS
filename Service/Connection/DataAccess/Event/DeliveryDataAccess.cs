using Model.Event;
using Npgsql;
using System.Diagnostics;

namespace Service.Connection.DataAccess.Event
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

        public void Add(IEnumerable<SentEvent> objectToAdd)
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

        public void Complete(IEnumerable<ArrivedEvent> objectToComplete)
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
            using var command = new NpgsqlCommand("SELECT arrival_event_id FROM" + FullTableName + " WHERE dispatch_event_id= ANY(@SentEventIds) ", connection);
            command.Parameters.AddWithValue("@SentEventIds", objectToComplete.Select(x => x.Id).ToArray());
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                if (!reader.IsDBNull(0))
                {
                    return true;
                }
            }

            ConnectionPool.ReleaseConnection(connection);
            return false;
        }

        public SentEvent? SelectSentEvent(StorableObjectEvent storableObjectEvent)
        {
            return SelectSentEvent([storableObjectEvent]).FirstOrDefault();
        }

        public List<SentEvent> SelectSentEvent(IEnumerable<StorableObjectEvent> storableObjectEvents)
        {
            List<SentEvent> sentEvents = [];

            string query = "SELECT D.departure_id, D.destination_id, D.dispatch_info, D.dispatch_event_id" +
                                "FROM " + FullTableName + " AS D " +
                                "WHERE D.dispatch_event_id = Any(@ids) ";

            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand(query, connection);

            List<long> ids = storableObjectEvents.Select(x => x.Id).ToList();

            command.Parameters.AddWithValue("@ids", ids.ToArray());

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                sentEvents.Add(new(storableObjectEvents.First(x => x.Id == reader.GetInt64(3)), reader.GetString(2), reader.GetInt32(0), reader.GetInt32(1)));
            }

            return sentEvents;
        }

        public List<ArrivedEvent> SelectArrivedEvents(IEnumerable<StorableObjectEvent> storableObjectsEvents)
        {
            List<ArrivedEvent> arrivedEvents = [];

            string query = "SELECT D.dispatch_event_id, D.arrival_info, D.arrival_event_id" +
                                "FROM " + FullTableName + " AS D " +
                                "WHERE D.arrival_event_id = Any(@Ids) ";

            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@Ids", storableObjectsEvents.Select(x => x.Id).ToArray());

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                arrivedEvents.Add(new(storableObjectsEvents.First(x => x.Id == reader.GetInt64(2)), reader.GetString(1), reader.GetInt64(0)));
            }

            return arrivedEvents;
        }

        public List<long> SelectActiveDeliveryIdsFromLocation(int locationId)
        {
            var deliveries = new List<long>();

            string query = "SELECT D.dispatch_event_id" +
                                "FROM " + FullTableName + " AS D " +
                                "WHERE D.arrival_event_id IS NULL AND D.departure_id = @locationId ";

            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@locationId", locationId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                deliveries.Add(reader.GetInt64(0));
                Debug.WriteLine($"Получили {reader.GetInt64(0)}-delivery идущее ИЗ Объекта с id - {locationId}");
            }

            ConnectionPool.ReleaseConnection(connection);

            return deliveries;
        }

        public List<ValueTuple<long, long>> SelectCompletedDeliveryIdsFromLocation(int locationId)
        {
            List<ValueTuple<long, long>> SentIdArrivedIdList = [];

            string query = "SELECT D.dispatch_event_id, D.arrival_event_id" +
                                "FROM " + FullTableName + " AS D " +
                                "WHERE D.arrival_event_id IS NOT NULL AND D.departure_id = @locationId ";

            var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@locationId", locationId);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                SentIdArrivedIdList.Add((reader.GetInt64(0), reader.GetInt64(1)));
            }

            ConnectionPool.ReleaseConnection(connection);

            return SentIdArrivedIdList;
        }

        public string SelectSentEventQuery()
        {
            return "SELECT D.dispatch_event_id FROM " + FullTableName + " AS D WHERE D.arrival_event_id IS NULL AND D.departure_id = " + LocationParameterName;
        }

        public const string LocationParameterName = "@locationId";
    }
}
