using EMAS_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Model.Exceptions;
using Service.Connection;

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
            var dbUser = GetUserFromDatabase(username, password);
            if (dbUser != null)
            {
                HttpContext.Session.SetString("UserId", dbUser.Id.ToString());
                HttpContext.Session.SetString("Username", dbUser.Username);

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.ErrorMessage = "Неверное имя пользователя или пароль";
                return View();
            }
        }

        private User? GetUserFromDatabase(string username, string password)
        {
            try
            {
                var User = new User { Id = SessionManager.GetUserId(username, password), Username = username};
                return User;
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
    }
}
