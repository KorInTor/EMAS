using Model;
using Model.Enum;
using Npgsql;
using Service.Connection.DataAccess.Interface;
using System.Diagnostics;

namespace Service.Connection.DataAccess
{
    public class EmployeeDataAccess : ISimpleDataAccess<Employee>
    {
        public void Add(Employee objectToAdd)
        {
            throw new NotImplementedException();
        }

        public void Delete(Employee objectToDelete)
        {
            throw new NotImplementedException();
        }

        public List<Employee> Select()
        {
            var connection = ConnectionPool.GetConnection();

            string sql = "SELECT id, fullname, email, username FROM public.employee;";

            using var command = new NpgsqlCommand(sql, connection);

            List<Employee> employeeList = [];

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    PermissionInfo permissionInfo = GetPermissionInfo(reader.GetInt32(0));
                    employeeList.Add(new Employee(reader.GetInt32(0), reader.GetString(1), reader.GetString(3), reader.GetString(2), permissionInfo));
                }
            }

            ConnectionPool.ReleaseConnection(connection);
            return employeeList;
        }

        public Employee SelectById(int id)
        {
            var connection = ConnectionPool.GetConnection();

            string sql = "SELECT fullname, username, email FROM public.employee WHERE id=@employeeId;";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", id);

            Employee employee = null;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    PermissionInfo permissionInfo = GetPermissionInfo(id);
                    employee = new Employee(id, reader.GetString(0), reader.GetString(1), reader.GetString(2), permissionInfo);
                }
            }

            ConnectionPool.ReleaseConnection(connection);
            return employee;
        }

        public Employee SelectByUsername(string username)
        {
            var connection = ConnectionPool.GetConnection();

            string sql = "SELECT id, fullname, email, username FROM public.employee WHERE username=@username;";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            Employee employee = null;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    PermissionInfo permissionInfo = GetPermissionInfo(reader.GetInt32(0));
                    employee = new Employee(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), permissionInfo);
                }
            }

            ConnectionPool.ReleaseConnection(connection);
            return employee;
        }

        public void Update(Employee objectToUpdate)
        {
            var connection = ConnectionPool.GetConnection();

            string query = "UPDATE public.employee SET fullname=@fullname, email=@email,";
            if (objectToUpdate.PasswordHash != string.Empty)
            {
                query += (" password_hash=@password_hash,");
            }
            query += ("  username=@username WHERE id=@id;");

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@fullname", objectToUpdate.Fullname);
            command.Parameters.AddWithValue("@email", objectToUpdate.Email);
            if (objectToUpdate.PasswordHash != string.Empty)
                command.Parameters.AddWithValue("@password_hash", objectToUpdate.PasswordHash);
            command.Parameters.AddWithValue("@username", objectToUpdate.Username);
            command.Parameters.AddWithValue("@id", objectToUpdate.Id);

            command.ExecuteNonQuery();

            ConnectionPool.ReleaseConnection(connection);

            UpdatePermissions(objectToUpdate);

            Debug.WriteLine($"Успешно обновлён сотрудник: Id: {objectToUpdate.Id}, Name: {objectToUpdate.Fullname}");

        }

        public void UpdatePermissions(Employee objectToUpdate)
        {
            var connection = ConnectionPool.GetConnection();

            string deleteQuery = "DELETE FROM \"permission\".employee_permissions WHERE employee_id=@employeeId;";
            using var deleteCommand = new NpgsqlCommand(deleteQuery, connection);
            deleteCommand.Parameters.AddWithValue("@employeeId", objectToUpdate.Id);
            deleteCommand.ExecuteNonQuery();

            string insertQuery = "INSERT INTO \"permission\".employee_permissions (employee_id, permission_type, location_id) VALUES (@employeeId, @permissionType, @location_id);";
            foreach (var permissionsOnLocation in objectToUpdate.PermissionInfo.Permissions)
            {
                foreach (var permission in permissionsOnLocation.Value)
                {
                    using var insertCommand = new NpgsqlCommand(insertQuery, connection);
                    insertCommand.Parameters.AddWithValue("@employeeId", objectToUpdate.Id);
                    insertCommand.Parameters.AddWithValue("@location_id", permissionsOnLocation.Key);
                    insertCommand.Parameters.AddWithValue("@permissionType", (int)Enum.Parse(typeof(PermissionType), permission));
                    insertCommand.ExecuteNonQuery();
                }
            }

            ConnectionPool.ReleaseConnection(connection);
        }

        public Dictionary<int, List<string>> SelectEmployeePermissions(int employeeId)
        {
            var connection = ConnectionPool.GetConnection();

            Dictionary<int, List<string>> permissions = [];

            string locationIdSql = "SELECT id FROM public.\"location\";";
            using var command1 = new NpgsqlCommand(locationIdSql, connection);
            using (var reader = command1.ExecuteReader())
            {
                while (reader.Read())
                {
                    permissions.Add(reader.GetInt32(0), []);
                }
            }

            string sql = "SELECT \"permission\".permission_type.\"name\", location_id " +
            "FROM \"permission\".employee_permissions JOIN \"permission\".permission_type ON permission_type.id = employee_permissions.permission_type " +
            "WHERE employee_id = @employeeId";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    permissions[reader.GetInt32(1)].Add(reader.GetString(0));
                }
            }
            ConnectionPool.ReleaseConnection(connection);
            return permissions;
        }

        public bool IsEmployeeAdmin(int employeeId)
        {
            var connection = ConnectionPool.GetConnection();

            string sql = "SELECT COUNT(employee_id) FROM \"permission\".\"admin\" WHERE employee_id = @employeeId";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);

            long count = (long)command.ExecuteScalar();

            ConnectionPool.ReleaseConnection(connection);
            return count > 0;
        }

        public PermissionInfo GetPermissionInfo(int employeeId)
        {
            return new PermissionInfo(IsEmployeeAdmin(employeeId), SelectEmployeePermissions(employeeId));
        }

        public void Add(Employee[] objectToAdd)
        {
            throw new NotImplementedException();
        }

        public void Delete(Employee[] objectToDelete)
        {
            throw new NotImplementedException();
        }

        public void Update(Employee[] objectToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}
