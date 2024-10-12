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

        public const string LocationParameterName = "@locationId";
    }
}
