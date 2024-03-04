using EMAS.Exceptions;
using EMAS.Model;
using EMAS.Model.HistoryEntry;
using Npgsql;
using System.Collections.Generic;
using System.Diagnostics;
using System.Formats.Asn1;
using System.Security.Cryptography;
using System.Text;

namespace EMAS.Service.Connection
{
    public static class DataBaseClient
    {
        private static string _host = "26.34.196.234";

        private static string _port = "5432";

        private static string _dataBase = "postgres";

        private static string _DBMSlogin = "praktikant";

        private static string _DBMSpassword = "hPS2lwTK0XaE";

        private static string _storedSalt = "0Xin54hFmmX93ljqMUqOzeqhCf8Cpeur";

        private static int _currentEmployeeId;

        public static string ConnectionString
        {
            get
            {
                return $"Host={_host};Port={_port};Username={_DBMSlogin};Password={_DBMSpassword};Database={_dataBase}";
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
                    throw new InvalidUsernameException("Некоректное имя пользователя.");
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
                    throw new InvalidPasswordException("Некоректный пароль.");
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

        private static Employee GetEmployeeInfoById(int employeeId)
        {
            throw new NotImplementedException();
        }

        public static List<Delivery> GetDeliveryTo(int locationId)
        {
            throw new NotImplementedException();
        }

        public static void AddNewEquipment(Equipment equipment)
        {
            throw new NotImplementedException();
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

        public static void UpdateEquipmentData(Equipment equipment)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public static void RemoveEmployee(Employee employee)
        {
            throw new NotImplementedException();
        }
    }
}
