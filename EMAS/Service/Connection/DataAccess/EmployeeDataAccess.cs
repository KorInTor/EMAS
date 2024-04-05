using EMAS.Model;
using EMAS.ViewModel;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMAS.Service.Connection.DataAccess
{
    public class EmployeeDataAccess : IDataAccess<Employee>
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
            throw new NotImplementedException();
        }

        public Employee SelectById(int id)
        {
            using var connection = ConnectionPool.GetConnection();
            
            string sql = "SELECT fullname, email, username FROM public.employee;";

            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", id);

            Employee employee = null;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    employee = new Employee(id, reader.GetString(0), reader.GetString(1), reader.GetString(2));
                }
            }

            ConnectionPool.ReleaseConnection(connection);
            return employee;
        }

        public Employee SelectByUsername(string username)
        {
            using var connection = ConnectionPool.GetConnection();

            string sql = "SELECT id, fullname, email, username FROM public.employee WHERE username=@username;";
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@username", username);

            Employee employee = null;

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    employee = new Employee(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3));
                }
            }

            ConnectionPool.ReleaseConnection(connection);
            return employee;
        }

        public void Update(Employee objectToUpdate)
        {
            var connection = ConnectionPool.GetConnection();

            string query = "UPDATE public.employee SET fullname=@fullname, email=@email, password_hash=@password_hash, username=@username WHERE id=@id;";

            using var command = new NpgsqlCommand(query, connection);
            command.Parameters.AddWithValue("@fullname", objectToUpdate.Fullname);
            command.Parameters.AddWithValue("@email", objectToUpdate.Email);
            command.Parameters.AddWithValue("@password_hash", objectToUpdate.PasswordHash);
            command.Parameters.AddWithValue("@username",objectToUpdate.Username);
            command.Parameters.AddWithValue("@id", objectToUpdate.Id);

            command.ExecuteNonQuery();

            Debug.WriteLine($"Успешно обновлён сотрудник: Id: {objectToUpdate.Id}, Name: {objectToUpdate.Fullname}");

            ConnectionPool.ReleaseConnection(connection);
        }

        public Dictionary<int, List<string>> SelectEmployeePermissions(int employeeId)
        {
            var connection = ConnectionPool.GetConnection();

            Dictionary<int, List<string>> permissions = [];

            connection.Open();

            string sql = "SELECT location_id FROM \"permission\".employee_permissions " +
                         "JOIN \"permission\".permission_type.name ON permission_type.id = permission_type " +
                         "WHERE employee_id = @employeeId ";


            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@employeeId", employeeId);

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
    }
}
