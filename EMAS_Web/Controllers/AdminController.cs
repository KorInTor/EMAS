using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Enum;
using Service.Connection;
using Service.Security;
using System.Diagnostics;

namespace EMAS_Web.Controllers
{
    public class AdminController : Controller
    {
        public class StatusUpdate
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult EquipmentStatusEditor()
        {
            return View(DataBaseClient.GetInstance().SelectEquipmentStatuses());
        }

        [HttpPost]
        public IActionResult EquipmentStatusEditor([FromBody] IEnumerable<StatusUpdate> statuses)
        {
            if (statuses == null || !statuses.Any())
            {
                Response.StatusCode = 400; // Bad Request
                return Json(new { message = "Список статусов пуст или невалиден." });
            }

            List<(int, string)> statusTuples = [];
            foreach(var status in statuses)
            {
                statusTuples.Add((status.Id, status.Value));
            }

            DataBaseClient.GetInstance().UpdateEquipmentStatuses(statusTuples);

            return Json(new { message = "Список успешно обновлён" });
        }

        public IActionResult EmployeeTable()
        {
            return View(DataBaseClient.GetInstance().Select<Employee>());
        }

        [HttpGet]
        public IActionResult AddEmployee()
        {
            ViewBag.Locations = DataBaseClient.GetInstance().SelectNamedLocations();

            return View("EmployeeEditor", null);
        }

        [HttpPost]
        public IActionResult AddEmployee(IEnumerable<string> checkedPermissions, Employee newEmployee, string? makeAdmin = null)
        {
            var permissions = checkedPermissions.Select(cp =>
            {
                var parts = cp.Split('-');
                return new Permission
                {
                    LocationId = int.Parse(parts[0]),
                    PermissionType = (PermissionType)Enum.Parse(typeof(PermissionType), parts[1])
                };
            }).ToList();

            newEmployee.Permissions = permissions;
            newEmployee.IsAdmin = makeAdmin != null;

            string newPassword = PasswordManager.Generate(10);
            newEmployee.PasswordHash = PasswordManager.Hash(newPassword);
            ViewBag.NewPassword = newPassword;

            try
            {
                DataBaseClient.GetInstance().AddSingle<Employee>(newEmployee);
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = ex.Message;
                return RedirectToActionPermanent("EmployeeTable", "Admin");
            }

            TempData["AlertMessage"] = $"Пароль для сотрудника: {newPassword}";
            return RedirectToActionPermanent("EmployeeTable", "Admin");
        }


        [HttpGet]
        public IActionResult EmployeeEditor(int employeeId)
        {
            ViewBag.Locations = DataBaseClient.GetInstance().SelectNamedLocations();

            return View(DataBaseClient.GetInstance().SelectSingleById<Employee>(employeeId, nameof(Employee.Id)));
        }

        [HttpPost]
        public IActionResult EmployeeEditor(IEnumerable<string> checkedPermissions, Employee updatedEmployee, string? changePassword = null)
        {
            updatedEmployee.Permissions = checkedPermissions.Select(cp =>
            {
                var parts = cp.Split('-');
                return new Permission
                {
                    LocationId = int.Parse(parts[0]),
                    PermissionType = (PermissionType)Enum.Parse(typeof(PermissionType), parts[1])
                };
            }).ToList();
            
            ViewBag.Locations = DataBaseClient.GetInstance().SelectNamedLocations();

			if (DataBaseClient.GetInstance().SelectSingleById<Employee>(updatedEmployee.Id, nameof(Employee.Id)) == null)
            {
                TempData["AlertMessage"] = "Такого сотрудника не существует";
                return RedirectToActionPermanent("EmployeeTable","Admin");
            }

            string passwordChanged = "";
            if (changePassword != null)
            {
                string newPassword = PasswordManager.Generate(10);
                updatedEmployee.PasswordHash = PasswordManager.Hash(newPassword);
                passwordChanged = $"Пароль для сотрудника: {newPassword}";
            }
            
            try
            {
                DataBaseClient.GetInstance().UpdateSingle(updatedEmployee);
            }
            catch(Exception ex)
            {
                TempData["AlertMessage"] = ex.Message;
                return RedirectToActionPermanent("EmployeeTable", "Admin");
            }

            TempData["AlertMessage"] = $"Данные изменены успешно" + passwordChanged;
            return RedirectToActionPermanent("EmployeeTable", "Admin");
        }
    }
}
