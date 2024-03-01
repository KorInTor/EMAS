using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using EMAS.Model;
using Npgsql;

namespace EMAS.Service.Connection
{
    public static class DataBaseClient
    {
        private static string _host = "26.34.196.234";

        private static string _port = "5432";

        private static string _dataBase = "EquipmentMovement";

        private static string _DBMSlogin = "praktikant";

        private static string _DBMSpassword = "hPS2lwTK0XaE";

        private static string _storedSalt = "0Xin54hFmmX93ljqMUqOzeqhCf8Cpeur";

        public static string ConnectionString
        {
            get
            {
                return $"Host={_host};Port={_port};Username={_DBMSlogin};Password={_DBMSpassword};Database={_dataBase}";
            }
        }

        private static string _username;

        private static string _password;

        public static string Username
        {
            get
            {
                return _username;
            }
            set
            {
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
                _password = value;
            }
        }

        public static bool ConnectionSuccesfull()
        {
            try
            {
                using var con = new NpgsqlConnection(ConnectionString);
                con.Open();
                return true;
            }
            catch (NpgsqlException)
            {
                return false;
            }
        }

        public static bool IsUsernameCorrect()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT COUNT(*) FROM (SELECT * FROM public.employee WHERE employee.username = @username) AS subquery;";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@username", Username);

            int count = (int)command.ExecuteScalar();

            connection.Close();
            return count == 1 ? true : false;
        }

        public static bool IsPasswordCorrect()
        {
            using var connection = new NpgsqlConnection(ConnectionString);
            connection.Open();

            string sql = "SELECT COUNT(*) FROM (SELECT * FROM public.employee WHERE employee.passwordHash = @password AND employee.username = @username) AS subquery;";
            using var command = new NpgsqlCommand(sql, connection);

            var hashOfEnteredPassword = HashPassword(Password);

            command.Parameters.AddWithValue("@username", Username);
            command.Parameters.AddWithValue("@password", hashOfEnteredPassword);

            int count = (int)command.ExecuteScalar();

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
            throw new NotImplementedException();
        }

        public static List<Equipment> GetEquipmentOnLocation(int locationId)
        {
            throw new NotImplementedException();
        }

        public static List<Employee> GetEmployeeData()
        {
            throw new NotImplementedException();
        }

        public static List<HistoryEntryBase> GetHistoryDataForEquipment(Equipment equipment)
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

        public static void AddNewEmployee(Employee employee, string password, string username)
        {
            throw new NotImplementedException();
        }

        public static void AddNewLocation(Location location)
        {
            throw new NotImplementedException();
        }

        public static void UpdateEquipmentData(Equipment equipment)
        {
            throw new NotImplementedException();
        }

        public static void UpdateEquipmentData(ref List<Equipment> equipment)
        {
            throw new NotImplementedException();
        }

        public static void UpdateEmployeeData()
        {
            throw new NotImplementedException();
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
