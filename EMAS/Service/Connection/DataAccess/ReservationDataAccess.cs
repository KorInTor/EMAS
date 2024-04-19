using EMAS.Model;
using EMAS.Model.Event;
using EMAS.Service.Connection.DataAccess.Interface;
using Npgsql;
using System.Diagnostics;

namespace EMAS.Service.Connection.DataAccess
{
    public class ReservationDataAccess : IObjectStateLocationBoundedDataAccess<Reservation>
    {
        private readonly EventDataAccess eventAccess = new();

        public void Add(Reservation objectToAdd)
        {
            Add([objectToAdd]);
        }

        public void Delete(Reservation objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Reservation> Select()
        {
            throw new NotImplementedException();
        }

        public Reservation SelectById(int id)
        {
            throw new NotImplementedException();
        }

        public void Update(Reservation objectToUpdate)
        {
            throw new NotImplementedException();
        }
        public void Complete(Reservation completedReservation)
        {
            Complete([completedReservation]);
        }

        public void SelectById(long Id)
        {
            throw new NotImplementedException();
        }

        public void AddOnLocation(Reservation item, int locationId)
        {
            throw new NotImplementedException();
        }

        public List<Reservation> SelectOnLocation(int locationId)
        {
            var equipmentAccess = new EquipmentDataAccess();
            var employeeAccess = new EmployeeDataAccess();
            var reservations = new List<Reservation>();

            string query = "SELECT E.date, EqEvent.equipment_id, R.start_event_id, R.additional_info, E.employee_id" +
                                "FROM \"event\".reservation AS R " +
                                "JOIN public.\"event\" AS E ON E.id = R.start_event_id " +
                                "JOIN public.equipment_event AS EqEvent ON EqEvent.event_id = R.start_event_id " +
                                "WHERE R.end_event_id IS NULL AND R.location_id = @locationId ";

            var connection = ConnectionPool.GetConnection();
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@locationId", locationId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                reservations.Add(new Reservation(reader.GetInt64(2), reader.GetDateTime(0), employeeAccess.SelectById(reader.GetInt32(4)), reader.GetString(3), equipmentAccess.SelectById(reader.GetInt32(1))));
                Debug.WriteLine($"Получили Reservation идущее ИЗ Объекта с id - {locationId}");
            }
            ConnectionPool.ReleaseConnection(connection);
            return reservations;
        }

        public void AddOnLocation(Reservation[] item, int locationId)
        {
            throw new NotImplementedException();
        }

        public void Complete(Reservation[] reservations)
        {
            foreach (var completedReservation in reservations)
            {
                using var connection = ConnectionPool.GetConnection();
                Event newEvent = new(SessionManager.UserId, 0, EventType.ReserveEnded, completedReservation.ReservedObjectsList.Id);
                eventAccess.Add(newEvent);

                using var command = new NpgsqlCommand("UPDATE \"event\".reservation SET end_event_id=@start_event_id, WHERE start_event_id=@end_event_id ", connection);

                command.Parameters.AddWithValue("@start_event_id", completedReservation.Id);
                command.Parameters.AddWithValue("@end_event_id", newEvent.Id);

                command.ExecuteNonQuery();

                ConnectionPool.ReleaseConnection(connection);
            }
        }

        Reservation IObjectStateDataAccess<Reservation>.SelectById(long Id)
        {
            throw new NotImplementedException();
        }

        public void Add(Reservation[] reservations)
        {
            foreach(var objectToAdd in reservations)
            {
                var connection = ConnectionPool.GetConnection();
                Event newEvent = new(SessionManager.UserId, 0, EventType.Reserved, objectToAdd.ReservedObjectsList.Id);
                eventAccess.Add(newEvent);
                objectToAdd.Id = newEvent.Id;

                string query = "INSERT INTO \"event\".reservation (start_event_id, location_id, additional_info) VALUES(@start_event_id, @location_id, @additional_info);";
                using var command = new NpgsqlCommand(query, connection);

                command.Parameters.AddWithValue("@start_event_id", objectToAdd.Id);
                command.Parameters.AddWithValue("@location_id", objectToAdd.LocationId);
                command.Parameters.AddWithValue("@additional_info", objectToAdd.AdditionalInfo);

                command.ExecuteNonQuery();

                Debug.WriteLine("Успешно добавлена резервация");

                ConnectionPool.ReleaseConnection(connection);
            }
        }

        public void Delete(Reservation[] objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Update(Reservation[] objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
