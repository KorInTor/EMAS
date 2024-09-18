using Model.Exceptions;
using Model.Event;
using Npgsql;
using System.Diagnostics;

namespace Service.Connection.DataAccess.Event
{
    public class ReservationDataAccess
    {
        private readonly string schemaName = "\"event\".";
        private readonly string tableName = "reservation";

        public string FullTableName => schemaName + tableName;

        public void Add(ReservedEvent objectToAdd)
        {
            Add([objectToAdd]);
        }

        public void Complete(ReserveEndedEvent completedReservation)
        {
            Complete([completedReservation]);
        }

        public void Complete(IEnumerable<ReserveEndedEvent> reservations)
        {
            foreach (var endReservationEvent in reservations)
            {
                using var connection = ConnectionPool.GetConnection();
                using var command = new NpgsqlCommand("UPDATE " + FullTableName + " SET end_event_id=@end_event_id, reserve_end_info=@reserve_end_info WHERE start_event_id=@start_event_id ", connection);

                command.Parameters.AddWithValue("@start_event_id", endReservationEvent.ReserveEventId);
                command.Parameters.AddWithValue("@end_event_id", endReservationEvent.Id);
                command.Parameters.AddWithValue("@reserve_end_info", endReservationEvent.Comment);

                command.ExecuteNonQuery();

                ConnectionPool.ReleaseConnection(connection);
            }
        }

        public bool IsCompleted(IEnumerable<ReserveEndedEvent> objectToComplete)
        {
            var connection = ConnectionPool.GetConnection();
            foreach (var reservation in objectToComplete)
            {
                using var command = new NpgsqlCommand("SELECT end_event_id FROM " + FullTableName + " WHERE start_event_id=@start_event_id ", connection);
                command.Parameters.AddWithValue("@start_event_id", reservation.ReserveEventId);
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

        public void Add(IEnumerable<ReservedEvent> reservations)
        {
            var connection = ConnectionPool.GetConnection();
            foreach (var objectToAdd in reservations)
            {
                string query = "INSERT INTO " + FullTableName + " (start_event_id, location_id, reserve_start_info) VALUES(@start_event_id, @location_id, @reserve_start_info);";
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@start_event_id", objectToAdd.Id);
                command.Parameters.AddWithValue("@location_id", objectToAdd.LocationId);
                command.Parameters.AddWithValue("@reserve_start_info", objectToAdd.Comment);

                command.ExecuteNonQuery();

                Debug.WriteLine("Успешно добавлена резервация");
            }
            ConnectionPool.ReleaseConnection(connection);
        }
    }
}
