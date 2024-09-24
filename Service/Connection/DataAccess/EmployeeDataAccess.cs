using Model;
using Model.Enum;
using Npgsql;
using Service.Connection.DataAccess.Query;
using System.Diagnostics;

namespace Service.Connection.DataAccess
{
    public class EmployeeDataAccess
    {
		private PermissionDataAccess permissionDataAccess = new();

        public void Add(IEnumerable<Employee> objectToAdd)
        {
            throw new NotImplementedException();
        }

        public void Delete(IEnumerable<Employee> objectToDelete)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Employee> Select(QueryBuilder queryBuilder)
        {
            queryBuilder.LazyInit<Employee>();

			var connection = ConnectionPool.GetConnection();

            using var command = new NpgsqlCommand(queryBuilder.Build(), connection);

			for (int i = 0; i < queryBuilder.Parameters.Count; i++)
			{
				command.Parameters.AddWithValue($"@{i}", queryBuilder.Parameters[i]);
			}

			List<Employee> employeeList = [];

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    employeeList.Add(new Employee(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), permissionDataAccess.SelectPermissions(reader.GetInt32(0)), permissionDataAccess.IsEmployeeAdmin(reader.GetInt32(0))));
                }
            }

            ConnectionPool.ReleaseConnection(connection);

            return employeeList;
        }

        public void Update(IEnumerable<Employee> employeesToUpdate)
        {
            var connection = ConnectionPool.GetConnection();

			List<ValueTuple<int, List<Permission>>> permissionsToUpdate = [];

			foreach(var employeeToUpdate in employeesToUpdate)
			{
				string query = "UPDATE public.employee SET fullname=@fullname, email=@email,";
				if (employeeToUpdate.PasswordHash != string.Empty)
				{
					query += (" password_hash=@password_hash,");
				}
				query += ("  username=@username WHERE id=@id;");

				using var command = new NpgsqlCommand(query, connection);
				command.Parameters.AddWithValue("@fullname", employeeToUpdate.Fullname);
				command.Parameters.AddWithValue("@email", employeeToUpdate.Email);
				if (employeeToUpdate.PasswordHash != string.Empty)
					command.Parameters.AddWithValue("@password_hash", employeeToUpdate.PasswordHash);
				command.Parameters.AddWithValue("@username", employeeToUpdate.Username);
				command.Parameters.AddWithValue("@id", employeeToUpdate.Id);

				command.ExecuteNonQuery();

				ConnectionPool.ReleaseConnection(connection);

				permissionsToUpdate.Add((employeeToUpdate.Id, employeeToUpdate.Permissions));

				Debug.WriteLine($"Успешно обновлён сотрудник: Id: {employeeToUpdate.Id}, Name: {employeeToUpdate.Fullname}");
			}

			permissionsToUpdate.ForEach(x => permissionDataAccess.UpdatePermissions(x.Item1, x.Item2));
		}
	}

    public class PermissionDataAccess
    {
		public void UpdatePermissions(int employeeId, IEnumerable<Permission> newPermissions)
		{
			var connection = ConnectionPool.GetConnection();

			string deleteQuery = "DELETE FROM \"permission\".employee_permissions WHERE employee_id=@employeeId;";
			using var deleteCommand = new NpgsqlCommand(deleteQuery, connection);
			deleteCommand.Parameters.AddWithValue("@employeeId", employeeId);
			deleteCommand.ExecuteNonQuery();

			string insertQuery = "INSERT INTO \"permission\".employee_permissions (employee_id, permission_type, location_id) VALUES (@employeeId, @permissionType, @location_id);";
			foreach (var permission in newPermissions)
			{
				using var insertCommand = new NpgsqlCommand(insertQuery, connection);
				insertCommand.Parameters.AddWithValue("@employeeId", employeeId);
				insertCommand.Parameters.AddWithValue("@location_id", permission.LocationId);
				insertCommand.Parameters.AddWithValue("@permissionType", (int)permission.PermissionType);
				insertCommand.ExecuteNonQuery();
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

		public List<Permission> SelectPermissions(int employeeId)
		{
			var connection = ConnectionPool.GetConnection();

			List<Permission> permissions = [];

			string sql = "SELECT permission_type, location_id " +
			"FROM \"permission\".employee_permissions " +
			"WHERE employee_id = @employeeId";

			using var command = new NpgsqlCommand(sql, connection);
			command.Parameters.AddWithValue("@employeeId", employeeId);

			using (var reader = command.ExecuteReader())
			{
				while (reader.Read())
				{
					var permission = new Permission(reader.GetInt32(1), (PermissionType)reader.GetInt32(0));
					permissions.Add(permission);
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
