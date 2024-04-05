using EMAS.Exceptions;
using EMAS.Model;
using EMAS.Model.HistoryEntry;
using Npgsql;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

namespace EMAS.Service.Connection
{
    public static class DataBaseClient
    {
        private static int _currentEmployeeId;

        private static bool? _isCurrentEmployeeAdmin;

        private static string _password = string.Empty;

        private static Dictionary<int, List<string>>? _permissions;

        private static Dictionary<string, short> _eventTypes = [];

        private static string _storedSalt = "0Xin54hFmmX93ljqMUqOzeqhCf8Cpeur";

        private static string _username = string.Empty;

        public static string ConnectionString
        {
            get
            {
                return ConnectionOptions.ConnectionString;
            }
        }

        public static int CurrentEmployeeId
        {
            get
            {
                return _currentEmployeeId;
            }
            private set
            {
                if (value == 0)
                {
                    throw new ArgumentException("Получен неправильный id сотрудника");
                }
                _currentEmployeeId = value;
            }
        }

        public static bool IsCurrentEmployeeAdmin
        {
            get
            {
                return _isCurrentEmployeeAdmin ??= IsCurrentSessionAdmin();
            }
        }

        public static string Password
        {
            get
            {
                return _password;
            }
            set
            {
                if (value == null || value == string.Empty)
                {
                    throw new InvalidPasswordException("Некоректный пароль.");
                    return;
                }
                _password = value;
            }
        }

        public static Dictionary<int, List<string>> Permissions
        {
            get
            {
                return _permissions ??= GetCurrentSessionPermissions();
            }
        }

        public static string Username
        {
            get
            {
                return _username;
            }
            set
            {
                if (value == null || value == string.Empty)
                {
                    throw new InvalidUsernameException("Некоректное имя пользователя.");
                    return;
                }
                _username = value;
            }
        }

        public static void AddNewDelivery(ref Delivery delivery)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            delivery.EventDispatchId = InsertEvent(connection, CurrentEmployeeId, _eventTypes["Sent"]);
            InsertEquipmentEvent(connection, delivery.EventDispatchId, delivery.Equipment.Id);

            string query = "INSERT INTO \"event\".delivery (dispatch_event_id, destination_id, departure_id) VALUES(@dispatch_event_id, @destination_id, @departure_id);";
            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@dispatch_event_id", delivery.EventDispatchId);
            command.Parameters.AddWithValue("@destination_id", delivery.DestinationId);
            command.Parameters.AddWithValue("@departure_id", delivery.DepartureId);

            command.ExecuteNonQuery();

            Debug.WriteLine("Успешно добавлена доставка");

            connection.Close();
        }

        public static void AddNewReservation(ref Reservation reservation, int locartionId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            reservation.Id = InsertEvent(connection, CurrentEmployeeId, _eventTypes["Reserved"]);
            InsertEquipmentEvent(connection, reservation.Id, reservation.Equipment.Id);

            string query = "INSERT INTO \"event\".reservation (start_event_id, location_id, additional_info) VALUES(@start_event_id, @location_id, @additional_info);";
            using var command = new NpgsqlCommand(query, connection);

            command.Parameters.AddWithValue("@start_event_id", reservation.Id);
            command.Parameters.AddWithValue("@location_id", locartionId);
            command.Parameters.AddWithValue("@additional_info", reservation.AdditionalInfo);

            command.ExecuteNonQuery();

            Debug.WriteLine("Успешно добавлена резервация");

            connection.Close();
        }

        public static void AddNewEmployee(Employee employee, string password)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string query = "INSERT INTO public.employee (fullname, email, username, password_hash) VALUES(@name, @email, @username, @password) RETURNING id;";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", employee.Fullname);
            command.Parameters.AddWithValue("@email", employee.Email);
            command.Parameters.AddWithValue("@username", employee.Username);
            command.Parameters.AddWithValue("@password", HashPassword(password));

            employee.Id = (int)command.ExecuteScalar();

            Debug.WriteLine($"Успешно вставлено: Id: {employee.Id}, Name: {employee.Fullname}");

            connection.Close();
        }

        public static void AddNewEquipment(Equipment equipment, int locationId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "INSERT INTO public.equipment (\"name\", manufacturer, \"type\", measurment_units, accuracy_class, measurment_limit, serial_number, inventory_number, tags, location_id, status, description) VALUES (@name, @manufacturer, @type, @measurment_units, @accuracy_class, @measurment_limit, @serial_number, @inventory_number, @tags, @location_id, @status, @description);";

            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@name", equipment.Name);
            command.Parameters.AddWithValue("@manufacturer", equipment.Manufacturer);
            command.Parameters.AddWithValue("@type", equipment.Type);
            command.Parameters.AddWithValue("@measurment_units", equipment.Units);
            command.Parameters.AddWithValue("@accuracy_class", equipment.AccuracyClass);
            command.Parameters.AddWithValue("@measurment_limit", equipment.Limit);
            command.Parameters.AddWithValue("@serial_number", equipment.FactoryNumber);
            command.Parameters.AddWithValue("@inventory_number", equipment.RegistrationNumber);
            command.Parameters.AddWithValue("@tags", equipment.Tags);
            command.Parameters.AddWithValue("@location_id", locationId);
            command.Parameters.AddWithValue("@status", equipment.Status);
            command.Parameters.AddWithValue("@description", equipment.Description);

            command.ExecuteNonQuery();

            connection.Close();
        }

        public static void AddNewLocation(Location location)
        {
            if (location.Name == string.Empty)
            {
                throw new ArgumentNullException(nameof(location));
            }

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string query = "INSERT INTO public.location (name) VALUES (@name)";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", location.Name);
            command.ExecuteNonQuery();

            Debug.WriteLine($"Успешно вставлено: Id: {location.Id}, Name: {location.Name}");

            connection.Close();
        }

        public static void CloseSession()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string query = "DELETE FROM \"session\".active_session WHERE employee_id = @employeeId";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@employeeId", CurrentEmployeeId);
            command.ExecuteNonQuery();

            connection.Close();

            Debug.WriteLine($"Успешно закрыта активная сессия в базе данных для сотрудника с Id = {CurrentEmployeeId}");
        }

        public static List<Employee> GetAllEmployeeData()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT id, fullname, username, email FROM public.employee;";

            using var command = new NpgsqlCommand(sql, connection);

            var list = new List<Employee>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new Employee(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3)));
                }

                foreach (var data in list)
                {
                    Debug.WriteLine($"Получен сотрудник:{data.Id}-{data.Fullname}");
                }

            }
            connection.Close();
            return list;
        }

        public static List<Equipment> GetAllEquipmentData()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT id, \"name\", manufacturer, \"type\", measurment_units, accuracy_class, measurment_limit, serial_number, inventory_number, tags, location_id, status, description FROM public.equipment;";

            using var command = new NpgsqlCommand(sql, connection);

            var list = new List<Equipment>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    List<string> tags = [.. ((string[])reader[9])]; // Считывание tags
                    list.Add(new Equipment(reader.GetString(11), reader.GetString(8), reader.GetString(12), reader.GetString(5), reader.GetString(4), reader.GetString(6), reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(7), tags));
                }

                foreach (var data in list)
                {
                    Debug.WriteLine($"Получено оборудование: {data.Id} - {data.Name}");
                }

            }
            connection.Close();
            return list;
        }

        public static List<Delivery> GetDeliveryTo(int locationId)
        {
            var deliveries = new List<Delivery>();

            string query = "SELECT E.date, EqEvent.equipment_id, D.departure_id, D.dispatch_event_id " +
                                "FROM \"event\".delivery AS D " +
                                "JOIN FROM public.\"event\" AS E ON E.id = D.dispatch_id " +
                                "JOIN FROM public.equipment_event AS EqEvent ON EqEvent.event_id = D.dispatch_id " +
                                "WHERE D.arrival_event_id IS NULL AND D.destination_id = @locationId ";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@locationId", locationId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    deliveries.Add(new Delivery(reader.GetInt64(3), locationId, reader.GetInt32(2), reader.GetDateTime(0), GetEquipmentDataById(reader.GetInt32(1))));
                    Debug.WriteLine($"Получили delivery идущее В Объект с id - {locationId}");
                }
            }

            return deliveries;
        }

        public static Equipment GetEquipmentDataById(int equipmentId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = $"SELECT id, \"name\", manufacturer, \"type\", measurment_units, accuracy_class, measurment_limit, serial_number, inventory_number, tags, location_id, status, description FROM public.equipment WHERE id = {equipmentId};";

            using var command = new NpgsqlCommand(sql, connection);

            Equipment equipment = null;

            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    List<string> tags = [.. ((string[])reader[9])]; // Считывание tags
                    equipment = new Equipment(reader.GetString(11), reader.GetString(8), reader.GetString(12), reader.GetString(5), reader.GetString(4), reader.GetString(6), reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(7), tags);
                    Debug.WriteLine($"Получено оборудование: {equipment.Id} - {equipment.Name}");
                }
            }

            connection.Close();
            return equipment;
        }

        public static List<Reservation> GetReservationsOn(int locationId)
        {
            var reservations = new List<Reservation>();

            string query = "SELECT E.date, EqEvent.equipment_id, R.start_event_id, R.additional_info, E.employee_id" +
                                "FROM \"event\".reservation AS R " +
                                "JOIN public.\"event\" AS E ON E.id = R.start_event_id " +
                                "JOIN public.equipment_event AS EqEvent ON EqEvent.event_id = R.start_event_id " +
                                "WHERE D.end_event_id IS NULL AND D.location_id = @locationId ";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@locationId", locationId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    reservations.Add(new Reservation(reader.GetInt64(2), reader.GetDateTime(0), GetEmployeeInfoById(reader.GetInt32(4)), reader.GetString(3), GetEquipmentDataById(reader.GetInt32(1))));
                    Debug.WriteLine($"Получили delivery идущее ИЗ Объекта с id - {locationId}");
                }
            }

            return reservations;
        }

        public static List<Delivery> GetDeliveryOutOf(int locationId)
        {
            var deliveries = new List<Delivery>();

            string query = "SELECT E.date, EqEvent.equipment_id, D.destination_id, D.dispatch_event_id " +
                                "FROM \"event\".delivery AS D " +
                                "JOIN public.\"event\" AS E ON E.id = D.dispatch_event_id " +
                                "JOIN public.equipment_event AS EqEvent ON EqEvent.event_id = D.dispatch_event_id " +
                                "WHERE D.arrival_event_id IS NULL AND D.departure_id = @locationId ";

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();

                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@locationId", locationId);

                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    deliveries.Add(new Delivery(reader.GetInt64(3), locationId, reader.GetInt32(2), reader.GetDateTime(0), GetEquipmentDataById(reader.GetInt32(1))));
                    Debug.WriteLine($"Получили delivery идущее ИЗ Объекта с id - {locationId}");
                }
            }

            return deliveries;
        }

        public static void CompleteDelivery(Delivery delivery)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            long newEventId = InsertEvent(connection, CurrentEmployeeId, _eventTypes["Arrival"]);
            SetDeliveryComplete(connection, newEventId, delivery.EventDispatchId);
            InsertEquipmentEvent(connection, newEventId, delivery.Equipment.Id);
            MoveEquipmentToLocation(connection, delivery.Equipment.Id, delivery.DestinationId);

            connection.Close();
        }

        private static void MoveEquipmentToLocation(NpgsqlConnection connection, int equipmentId, int newLocationId)
        {
            using var command = new NpgsqlCommand("UPDATE \"public\".equipment SET location_id=@newLocationId, WHERE id=@equipmentId ", connection);
            command.Parameters.AddWithValue("@newLocationId", newLocationId);
            command.Parameters.AddWithValue("@equipmentId", equipmentId);

            command.ExecuteNonQuery();
        }

        private static long InsertEvent(NpgsqlConnection connection, int employeeId, int eventTypeId)
        {
            using var command = new NpgsqlCommand("INSERT INTO public.event (employee_id, event_type) VALUES (@emp_id,@eventTypeId) RETURNING id ", connection);
            command.Parameters.AddWithValue("@emp_id", employeeId);
            command.Parameters.AddWithValue("@eventTypeId", eventTypeId);

            return (long)command.ExecuteScalar();
        }

        private static void SetDeliveryComplete(NpgsqlConnection connection, long newEventId, long sendedEventId)
        {
            using var command = new NpgsqlCommand("UPDATE \"event\".delivery SET arrival_event_id=@arrival_event_id, WHERE dispatch_event_id=@sendedEventId ", connection);
            command.Parameters.AddWithValue("@arrival_event_id", newEventId);
            command.Parameters.AddWithValue("@sendedEventId", sendedEventId);

            command.ExecuteNonQuery();
        }

        private static void InsertEquipmentEvent(NpgsqlConnection connection, long newEventId, int equipmentId)
        {
            using var command = new NpgsqlCommand("INSERT INTO public.\"equipment_event\" (equipment_id, event_id) VALUES (@eq_id ,@new_id) ", connection);
            command.Parameters.AddWithValue("@new_id", newEventId);
            command.Parameters.AddWithValue("@eq_id", equipmentId);

            command.ExecuteNonQuery();
        }

        public static void EndReservation(Reservation reservation)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            long newEventId = InsertEvent(connection, CurrentEmployeeId, _eventTypes["ReserveEnded"]);
            SetReservationComplete(connection, newEventId, reservation.Id);
            InsertEquipmentEvent(connection, newEventId, reservation.Equipment.Id);

            connection.Close();
        }

        private static void SetReservationComplete(NpgsqlConnection connection, long reservationEndedEventId, long reservation_id)
        {
            using var command = new NpgsqlCommand("UPDATE \"event\".reservation SET end_event_id=@start_event_id, WHERE start_event_id=@end_event_id ", connection);

            command.Parameters.AddWithValue("@start_event_id", reservation_id);
            command.Parameters.AddWithValue("@end_event_id", reservationEndedEventId);

            command.ExecuteNonQuery();
        }

        public static List<string> GetDistincEquipmentAccuracyClasses()
        {
            return GetDistinctValues("accuracy_class");
        }

        public static List<string> GetDistincEquipmentManufacturers()
        {
            return GetDistinctValues("manufacturer");
        }

        public static List<string> GetDistincEquipmentMeasurmentLimits()
        {
            return GetDistinctValues("measurment_limit");
        }

        public static List<string> GetDistincEquipmentMeasurmentUnits()
        {
            return GetDistinctValues("measurment_units");
        }

        public static List<string> GetDistincEquipmentNames()
        {
            return GetDistinctValues("\"name\"");
        }

        public static List<string> GetDistincEquipmentTypes()
        {
            return GetDistinctValues("\"type\"");
        }

        public static List<string> GetDistinctValues(string columnName)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = $"SELECT DISTINCT {columnName} FROM public.equipment;";

            using var command = new NpgsqlCommand(sql, connection);

            var list = new List<string>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(reader.GetString(0));
                }
            }

            connection.Close();
            return list;
        }

        public static List<HistoryEntryBase> GetEquipmentHistory(int equipmentId)
        {
            var list = new List<HistoryEntryBase>();
            var eventConstructors = new Dictionary<string, Func<Employee, DateOnly, HistoryEntryBase>>
            {
                { "Addition", (responcible, date) => new AdditionHistoryEntry(responcible, date) },
                { "Departure", (responcible, date) => new SentHistoryEntry(responcible, date) },
                { "Arrival", (responcible, date) => new ReceivedHistoryEntry(responcible, date) },
                { "Decomission", (responcible, date) => new DecommissionedHistoryEntry(responcible, date) },
                { "StartReservation", (responcible, date) => new ReservedHistoryEntry(responcible, date) },
                { "EndReservation", (responcible, date) => new ReservationEndedHistoryEntry(responcible, date) }
            };

            using (var connection = new NpgsqlConnection(ConnectionString))
            {
                connection.Open();
                string query = "SELECT event.date, event.employee_id, event_type.\"name\" " +
                       "FROM public.\"event\" AS event " +
                       "JOIN public.equipment_event AS equipmenEvent " +
                       "JOIN public.event_type AS event_type ON event_type.id = event.event_type " +
                       "AND event.id = equipmenEvent.event_id AND equipmenEvent.equipment_id = @id";

                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", equipmentId);
                using var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    Employee responcible = GetEmployeeInfoById(reader.GetInt32(1));
                    DateOnly date = DateOnly.FromDateTime(reader.GetDateTime(0));
                    string eventType = reader.GetString(2);

                    if (eventConstructors.TryGetValue(eventType, out var constructor))
                    {
                        list.Add(constructor(responcible, date));
                    }
                    else
                    {
                        throw new UnknownEventTypeException($"Полученый тип события = '{eventType}' оказался неизвестным");
                    }
                }
            }

            foreach (var entry in list)
            {
                Debug.WriteLine("Полученны данные: " + entry.ToString());
            }

            return list;
        }

        public static List<Equipment> GetEquipmentOnLocation(int locationId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT id, \"name\", manufacturer, \"type\", measurment_units, accuracy_class, measurment_limit, serial_number, inventory_number, tags, location_id, status, description FROM public.equipment WHERE location_id = @locationId;";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@locationId", locationId);

            var list = new List<Equipment>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    List<string> tags = [.. ((string[])reader[9])]; // Считывание tags
                    list.Add(new Equipment(reader.GetString(11), reader.GetString(8), reader.GetString(12), reader.GetString(5), reader.GetString(4), reader.GetString(6), reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(7), tags));
                }

                foreach (var data in list)
                {
                    Debug.WriteLine($"Получено оборудование: {data.Id} - {data.Name}");
                }
            }
            connection.Close();
            return list;
        }

        public static List<Location> GetLocationData()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT * FROM public.location";
            using var command = new NpgsqlCommand(sql, connection);

            var list = new List<Location>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(new Location(reader.GetInt32(0), reader.GetString(1)));
                }

                foreach (var data in list)
                {
                    Debug.WriteLine($"Получена локация:{data.Id}-{data.Name}");
                }

            }
            connection.Close();
            return list;
        }

        public static List<int> GetLocationsId()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT id FROM public.location";
            using var command = new NpgsqlCommand(sql, connection);

            var list = new List<int>();

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    list.Add(reader.GetInt32(0));
                }

                foreach (var data in list)
                {
                    Debug.WriteLine($"Получено id локации:{data}");
                }

            }
            connection.Close();
            return list;
        }

        public static string HashPassword(string password)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(_storedSalt), 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(hash);
        }

        public static bool IsPasswordCorrect()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT COUNT(*) FROM (SELECT * FROM public.employee WHERE employee.password_hash = @passwordHash AND employee.username = @username) AS subquery;";
            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@username", Username);
            command.Parameters.AddWithValue("@passwordHash", HashPassword(Password));

            Debug.WriteLine($"Хэш для {Password} = {HashPassword(Password)}");

            int? count = (int?)(long?)command.ExecuteScalar();

            connection.Close();

            return count == 1;
        }

        public static bool IsUsernameCorrect()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT COUNT(*) FROM (SELECT * FROM public.employee WHERE employee.username = @username) AS subquery;";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@username", Username);

            int? count = (int?)(long?)command.ExecuteScalar();

            connection.Close();
            return count == 1;
        }

        public static void Login()
        {
            TryConnectToserver();
            if (!IsUsernameCorrect())
            {
                throw new InvalidUsernameException();
            }
            if (!IsPasswordCorrect())
            {
                throw new InvalidPasswordException();
            }
            SetCurrentSessionEmployeeId();
            CloseSession(); //УДАЛИТЬ ПОСЛЕ ВЫПУСКА!!!.
            CreateNewSession();
            InitEventTypeList();
        }

        private static void InitEventTypeList()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT id, \"name\" FROM public.event_type;";
            using var command = new NpgsqlCommand(sql, connection);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    _eventTypes.Add(reader.GetString(1), reader.GetInt16(0));
                }

                foreach (var item in _eventTypes)
                {
                    Debug.WriteLine($"Получено тип события:{item.Key} - {item.Value}");
                }

            }
            connection.Close();
        }

        public static void RemoveEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }

        public static void RemoveEquipment(Equipment equipment)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "DELETE FROM public.equipment WHERE id = @id;";

            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@id", equipment.Id);

            command.ExecuteNonQuery();

            connection.Close();
        }

        public static void TryConnectToserver()
        {
            using var conection = new NpgsqlConnection(ConnectionString);
            try
            {
                conection.Open();
                conection.Close();
            }
            catch (Exception ex)
            {
                throw new ConnectionFailedException(ex.Message);
            }
        }

        public static void UpdateEmployeeData(Employee employee, string? newPassword = null)
        {
            if (employee.Id == 0)
            {
                throw new ArgumentException("Id Сотрудника для изменения не может быть 0");
            }

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string query = "UPDATE public.employee SET fullname=@fullname, email=@email";

            if (newPassword is not null)
            {
                query += ", password_hash=@password";
            }

            query += " WHERE id=@id;";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@fullname", employee.Fullname);
            command.Parameters.AddWithValue("@email", employee.Email);

            if (newPassword is not null)
            {
                command.Parameters.AddWithValue("@password", HashPassword(newPassword));
            }

            command.Parameters.AddWithValue("@id", employee.Id);
            command.ExecuteNonQuery();

            Debug.WriteLine($"Успешно обновлён сотрудник: Id: {employee.Id}, Name: {employee.Fullname}");

            connection.Close();
        }

        public static void UpdateEquipmentData(Equipment equipment, int locationId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "UPDATE public.equipment SET \"name\" = @name, manufacturer = @manufacturer, \"type\" = @type, measurment_units = @measurment_units, accuracy_class = @accuracy_class, measurment_limit = @measurment_limit, serial_number = @serial_number, inventory_number = @inventory_number, tags = @tags, location_id = @location_id, status = @status, description = @description WHERE id = @id;";

            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@id", equipment.Id);
            command.Parameters.AddWithValue("@name", equipment.Name);
            command.Parameters.AddWithValue("@manufacturer", equipment.Manufacturer);
            command.Parameters.AddWithValue("@type", equipment.Type);
            command.Parameters.AddWithValue("@measurment_units", equipment.Units);
            command.Parameters.AddWithValue("@accuracy_class", equipment.AccuracyClass);
            command.Parameters.AddWithValue("@measurment_limit", equipment.Limit);
            command.Parameters.AddWithValue("@serial_number", equipment.FactoryNumber);
            command.Parameters.AddWithValue("@inventory_number", equipment.RegistrationNumber);
            command.Parameters.AddWithValue("@tags", equipment.Tags);
            command.Parameters.AddWithValue("@location_id", locationId);
            command.Parameters.AddWithValue("@status", equipment.Status);
            command.Parameters.AddWithValue("@description", equipment.Description);

            command.ExecuteNonQuery();

            connection.Close();
        }

        public static void UpdateEquipmentData(ref List<Equipment> equipment)
        {
            throw new NotImplementedException();
        }

        private static void CreateNewSession()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string query = "INSERT INTO \"session\".active_session (employee_id) VALUES (@employeeId) ";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@employeeId", CurrentEmployeeId);
            command.ExecuteNonQuery();

            connection.Close();

            Debug.WriteLine($"Успешно установлена сессия в базе данных для сотрудника с Id = {CurrentEmployeeId}");
        }
        private static Dictionary<int, List<string>> GetCurrentSessionPermissions()
        {
            Dictionary<int, List<string>> permissions = [];

            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT location_id FROM \"permission\".employee_permissions " +
                         "JOIN \"permission\".permission_type.name ON permission_type.id = permission_type " +
                         "WHERE employee_id = @employeeId ";


            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", CurrentEmployeeId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    int locationId = reader.GetInt32(0);
                    string permissionType = reader.GetString(1);

                    if (!permissions.TryGetValue(locationId, out List<string>? value))
                    {
                        value = [];
                        permissions[locationId] = value;
                    }
                    value.Add(permissionType);
                }
            }
            connection.Close();
            return permissions;
        }

        private static Employee GetEmployeeInfoById(int employeeId)
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT fullname, email, username FROM public.employee;";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", CurrentEmployeeId);

            Employee employee = null;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    employee = new Employee(employeeId, reader.GetString(0), reader.GetString(1), reader.GetString(2));
                }
            }

            connection.Close();
            return employee;
        }

        private static bool IsCurrentSessionAdmin()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT COUNT(employee_id) FROM \"permission\".\"admin\" WHERE employee_id = @employeeId";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", CurrentEmployeeId);

            long count = (long)command.ExecuteScalar();

            connection.Close();
            return count > 0;
        }

        private static void SetCurrentSessionEmployeeId()
        {
            if (!IsUsernameCorrect())
            {
                throw new InvalidUsernameException();
            }
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string query = "SELECT id FROM public.employee WHERE employee.username = @username";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@username", Username);

            int? EmployeeId = (int?)command.ExecuteScalar();

            if (EmployeeId == null)
            {
                throw new ArgumentNullException(nameof(EmployeeId));
            }
            else
            {
                CurrentEmployeeId = (int)EmployeeId;
            }

            connection.Close();
        }
    }
}
