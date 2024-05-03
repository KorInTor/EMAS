using DocumentFormat.OpenXml.Office2010.Excel;
using EMAS.Exceptions;
using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System.Diagnostics;

namespace EMAS.Service.Connection.DataAccess
{
    public class ReservationDataAccess
    {
        private readonly EventDataAccess eventAccess = new();
        private readonly string schemaName = "\"event\".";
        private readonly string tableName = "reservation";

        public string FullTableName => schemaName + tableName;

        public void Add(Reservation objectToAdd)
        {
            Add([objectToAdd]);
        }

        public void Complete(Reservation completedReservation)
        {
            Complete([completedReservation]);
        }

        public Reservation? SelectById(long id)
        {
            return SelectByIds([id])[0];
        }

        public Reservation[] SelectByIds(long[] ids)
        {
            List<Reservation> foundedReservations = [];
            IEnumerable<StorableObjectEvent> foundedStorableObjectEvents = eventAccess.SelectByIds(ids);
            var connection = ConnectionPool.GetConnection();
            foreach (var reserve_start_event_info in foundedStorableObjectEvents)
            {
                string query = "SELECT R.location_id, R.reserve_start_info " +
                                    "FROM " + FullTableName + " AS R " +
                                    "WHERE R.start_event_id = @id ";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", reserve_start_event_info.Id);

                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    foundedReservations.Add(new(reserve_start_event_info, reader.GetString(1), reader.GetInt32(0)));
                }
            }
            ConnectionPool.ReleaseConnection(connection);
            return [..foundedReservations];
        }

        public List<Reservation> SelectOnLocation(int locationId)
        {
            Reservation[]? reservations = SelectByIds(SelectIdsByLocation(locationId));
            if (reservations is not null)
            {
                return new List<Reservation>(reservations);
            }
            else
                return [];
        }

        public long[] SelectIdsByLocation(int locationId)
        {
            List<long> foundedReservationIds = [];

            string query = "SELECT R.start_event_id" +
                                "FROM " + FullTableName + " AS R " +
                                "WHERE R.end_event_id IS NULL AND R.location_id = @locationId ";

            var connection = ConnectionPool.GetConnection();
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@locationId", locationId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                foundedReservationIds.Add(reader.GetInt64(0));
            }
            ConnectionPool.ReleaseConnection(connection);

            return [..foundedReservationIds];
        }

        public void Complete(Reservation[] reservations)
        {
            if (!CanComplete(reservations))
                throw new EventAlreadyCompletedException();

            foreach (var completedReservation in reservations)
            {
                if (!completedReservation.IsCompleted)
                {
                    throw new InvalidOperationException("Невозможно завершить резрвацию, не все данные заполнены");
                }

                StorableObjectEvent newEvent = new(SessionManager.UserId, 0, EventType.ReserveEnded, completedReservation.EndDate, completedReservation.ReservedObjectsList);
                eventAccess.Add(newEvent);

                using var connection = ConnectionPool.GetConnection();
                using var command = new NpgsqlCommand("UPDATE " + FullTableName + " SET end_event_id=@end_event_id, reserve_end_info=@reserve_end_info WHERE start_event_id=@start_event_id ", connection);

                command.Parameters.AddWithValue("@start_event_id", completedReservation.Id);
                command.Parameters.AddWithValue("@end_event_id", newEvent.Id);
                command.Parameters.AddWithValue("@reserve_end_info", completedReservation.ReserveEndInfo);

                command.ExecuteNonQuery();

                ConnectionPool.ReleaseConnection(connection);
            }
        }

        private bool CanComplete(Reservation[] objectToComplete)
        {
            var connection = ConnectionPool.GetConnection();
            foreach (var reservation in objectToComplete)
            {
                using var command = new NpgsqlCommand("SELECT end_event_id FROM " + FullTableName + " WHERE start_event_id=@start_event_id ", connection);
                command.Parameters.AddWithValue("@start_event_id", reservation.Id);
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        return false;
                    }
                }

            }

            ConnectionPool.ReleaseConnection(connection);
            return true;
        }

        public void Add(Reservation[] reservations)
        {
            var connection = ConnectionPool.GetConnection();
            foreach (var objectToAdd in reservations)
            {
                StorableObjectEvent newEvent = new(SessionManager.UserId, 0, EventType.Reserved, objectToAdd.StartDate ,objectToAdd.ReservedObjectsList);
                eventAccess.Add(newEvent);
                objectToAdd.Id = newEvent.Id;

                string query = "INSERT INTO " + FullTableName + " (start_event_id, location_id, reserve_start_info) VALUES(@start_event_id, @location_id, @reserve_start_info);";
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@start_event_id", objectToAdd.Id);
                command.Parameters.AddWithValue("@location_id", objectToAdd.LocationId);
                command.Parameters.AddWithValue("@reserve_start_info", objectToAdd.ReserveStartInfo);

                command.ExecuteNonQuery();

                Debug.WriteLine("Успешно добавлена резервация");
            }
            ConnectionPool.ReleaseConnection(connection);
        }

        public Reservation? SelectCompletedByEndId(long id)
        {
            List<Reservation> foundedReservations = [];
            StorableObjectEvent? foundedEndEvent = eventAccess.SelectById(id);
            if (foundedEndEvent == null)
                return null;

            var connection = ConnectionPool.GetConnection();

            string query = "SELECT R.location_id, R.reserve_start_info, R.reserve_end_info, R.start_event_id " +
                                "FROM " + FullTableName + " AS R " +
                                "WHERE R.end_event_id = @id ";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", foundedEndEvent.Id);

            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var foundedStartEvent = eventAccess.SelectById(reader.GetInt64(3));
                var reservation = new Reservation(foundedStartEvent, reader.GetString(1), reader.GetInt32(0));
                reservation.Complete(foundedEndEvent.DateTime,reader.GetString(2));
                foundedReservations.Add(reservation);
            }
            ConnectionPool.ReleaseConnection(connection);
            return foundedReservations.FirstOrDefault();
        }
    }
}
