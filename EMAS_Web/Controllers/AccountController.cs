using EMAS_Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Exceptions;
using Npgsql;
using Service.Connection;
using Service.Connection.DataAccess.Query;
using Service.Security;

namespace EMAS_Web.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString("UserId") != null)
            {
                return RedirectToAction("Index","Home");
            }
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                TempData["AlertMessage"] = "Если ты ещё раз отправишь пустое поле, я солью твои данные в сеть";
                return View();
            }

            var dbUser = GetUserFromDatabase(username, password);
            if (dbUser != null)
            {
                HttpContext.Session.SetInt32("UserId", dbUser.Id);
                HttpContext.Session.SetInt32("Admin", dbUser.PermissionInfo.IsCurrentEmployeeAdmin ? 1 : 0);
                HttpContext.Session.SetString("Username", dbUser.Username);
                HttpContext.Session.SetString("UserFullname", dbUser.Fullname);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                TempData["AlertMessage"] = "Неверное имя пользователя или пароль";
                return View();
            }
        }

        private Employee? GetUserFromDatabase(string username, string password)
        {
            try
            {
                TryLogin(username, PasswordManager.Hash(password));

				QueryBuilder queryBuilder = new QueryBuilder();
				queryBuilder.Where($"{nameof(Employee)}.{nameof(Employee.Username)}", "=", username);

				var currentUser = DataBaseClient.GetInstance().Select<Employee>(queryBuilder).FirstOrDefault();

				return currentUser;
            }
            catch (InvalidUsernameException)
            {
                return null;
            }
            catch (InvalidPasswordException)
            {
                return null;
            }
        }

        private void TryLogin(string username, string passwordHash)
        {
			if (!IsUsernameCorrect(username))
			{
				throw new InvalidUsernameException();
			}
			if (!IsPasswordCorrect(username, passwordHash))
			{
				throw new InvalidPasswordException();
			}
		}

		private bool IsPasswordCorrect(string username, string passwordHash)
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

		private bool IsUsernameCorrect(string username)
		{
			var connection = ConnectionPool.GetConnection();

			string sql = "SELECT COUNT(*) FROM (SELECT * FROM public.employee WHERE employee.username = @username) AS subquery;";
			using var command = new NpgsqlCommand(sql, connection);
			command.Parameters.AddWithValue("@username", username);

			long? count = (long?)command.ExecuteScalar();

			ConnectionPool.ReleaseConnection(connection);
			return count == 1;
		}
	}
}
