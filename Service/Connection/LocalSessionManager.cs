using Model.Exceptions;
using Service.Connection.DataAccess;
using Service.Security;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Service.Connection
{
    public static class LocalSessionManager
    {
        public static PermissionInfo PermissionInfo { get; private set; }

        public static int UserId { get; private set; }

        public static void Login(string username,string password)
        {
            string passwordHash = PasswordManager.Hash(password);
            ConnectionPool.TryConnect();
            if (!IsUsernameCorrect(username))
            {
                throw new InvalidUsernameException();
            }
            if (!IsPasswordCorrect(username, passwordHash))
            {
                throw new InvalidPasswordException();
            }
            SetCurrentSessionProperties(username);
            CloseSession(); //Временное решение для Debug. Удалить после выпуска.
            CreateNewSession();
        }

        public static int GetUserId(string username, string password)
        {
            string passwordHash = PasswordManager.Hash(password);
            ConnectionPool.TryConnect();
            if (!IsUsernameCorrect(username))
            {
                throw new InvalidUsernameException();
            }
            if (!IsPasswordCorrect(username, passwordHash))
            {
                throw new InvalidPasswordException();
            }
            var employeeAccess = new EmployeeDataAccess();
            return employeeAccess.SelectByUsername(username).Id;
        }

        private static void SetCurrentSessionProperties(string username)
        {
            var employeeAccess = new EmployeeDataAccess();
            UserId = employeeAccess.SelectByUsername(username).Id;

            PermissionInfo = employeeAccess.GetPermissionInfo(UserId);
        }

        public static bool IsPasswordCorrect(string username,string passwordHash)
        {
            var connection = ConnectionPool.GetConnection();

            string sql = "SELECT COUNT(*) FROM (SELECT * FROM public.employee WHERE employee.password_hash = @passwordHash AND employee.username = @username) AS subquery;";
            using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@username", username);
            command.Parameters.AddWithValue("@passwordHash", passwordHash);


            long? count = (long?)command.ExecuteScalar();

            ConnectionPool.ReleaseConnection(connection);

            return count == 1;
        }

        public static bool IsUsernameCorrect(string username)
        {
            var connection = ConnectionPool.GetConnection();

            string sql = "SELECT COUNT(*) FROM (SELECT * FROM public.employee WHERE employee.username = @username) AS subquery;";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            long? count = (long?)command.ExecuteScalar();

            ConnectionPool.ReleaseConnection(connection);
            return count == 1;
        }

        public static void CloseSession()
        {
            var connection = ConnectionPool.GetConnection();

            string query = "DELETE FROM \"session\".active_session WHERE employee_id = @employeeId";
            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@employeeId", UserId);
            command.ExecuteNonQuery();

            ConnectionPool.ReleaseConnection(connection);
            Debug.WriteLine($"Успешно закрыта активная сессия в базе данных для сотрудника с Id = {UserId}");
        }

        private static void CreateNewSession()
        {
            var connection = ConnectionPool.GetConnection();

            string query = "INSERT INTO \"session\".active_session (employee_id) VALUES (@employeeId) ";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@employeeId", UserId);
            command.ExecuteNonQuery();

            ConnectionPool.ReleaseConnection(connection);
            Debug.WriteLine($"Успешно установлена сессия в базе данных для сотрудника с Id = {UserId}");
        }
    }
}
