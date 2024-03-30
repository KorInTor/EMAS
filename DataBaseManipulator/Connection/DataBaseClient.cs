using EMAS.Exceptions;
using EMAS.Model;
using EMAS.Model.HistoryEntry;
using EMAS.Service.Connection;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Text;

namespace DataBaseManipulator.Connection
{
    public static class DataBaseClient
    {
        public static List<int> IdsOfLocations()
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

        private static string _storedSalt = "0Xin54hFmmX93ljqMUqOzeqhCf8Cpeur";

        private static int _currentEmployeeId;

        private static Dictionary<int, List<string>>? _permissions;

        private static bool? _isCurrentEmployeeAdmin;

        public static string ConnectionString
        {
            get
            {
                return ConnectionOptions.ConnectionString;
            }
        }

        private static string _username = "Пряхин Д.С.";

        private static string _password = "ps123123";

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
                    //throw new InvalidUsernameException("Некоректное имя пользователя.");
                    return;
                }
                _username = value;
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
                    //throw new InvalidPasswordException("Некоректный пароль.");
                    return;
                }
                _password = value;
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

        public static Dictionary<int, List<string>> Permissions
        {
            get
            {
                return _permissions ??= GetCurrentSessionPermissions();
            }
        }

        public static bool IsCurrentEmployeeAdmin
        {
            get
            {
                return _isCurrentEmployeeAdmin ??= IsCurrentSessionAdmin();
            }
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
            CreateNewSession();
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

        public static string HashPassword(string password)
        {
            var pbkdf2 = new Rfc2898DeriveBytes(password, Encoding.UTF8.GetBytes(_storedSalt), 10000);
            byte[] hash = pbkdf2.GetBytes(20);
            return Convert.ToBase64String(hash);
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

        public static List<Equipment> GetEquipmentOnLocation(int locationId)
        {
            throw new NotImplementedException();
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

        private static bool IsCurrentSessionAdmin()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT employee_id FROM \"permission\".\"admin\" WHERE employee_id = @employeeId";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", CurrentEmployeeId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.GetString(0) is not null)
                    {
                        connection.Close();
                        return true;
                    }
                }
            }
            connection.Close();

            return false;
        } 

        public static Dictionary<int, List<string>> GetPermissions()
        {
            throw new NotImplementedException();
        }

        private static Employee GetEmployeeInfoById(int employeeId)
        {
            throw new NotImplementedException();
        }

        public static List<Delivery> GetDeliveryTo(int locationId)
        {
            throw new NotImplementedException();
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


        public static void AddNewDelivery(Delivery delivery)
        {
            throw new NotImplementedException();
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

        public static void RemoveEmployee(Employee employee)
        {
            throw new NotImplementedException();
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

        public static List<string> GetDistincEquipmentNames()
        {
            return GetDistinctValues("\"name\"");
        }

        public static List<string> GetDistincEquipmentTypes()
        {
            return GetDistinctValues("\"type\"");
        }

        public static List<string> GetDistincEquipmentMeasurmentUnits()
        {
            return GetDistinctValues("measurment_units");
        }

        public static List<string> GetDistincEquipmentAccuracyClasses()
        {
            return GetDistinctValues("accuracy_class");
        }

        public static List<string> GetDistincEquipmentMeasurmentLimits()
        {
            return GetDistinctValues("measurment_limit");
        }

        public static List<string> GetDistincEquipmentManufacturers()
        {
            return GetDistinctValues("manufacturer");
        }
    }
}
