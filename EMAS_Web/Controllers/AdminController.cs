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

            Debug.WriteLine("Полученные статусы");
            foreach (var status in statuses)
            {
                Debug.WriteLine(status);
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
            return View(DataBaseClient.GetInstance().SelectEmployee());
        }

        [HttpGet]
        public IActionResult EmployeeEditor(int employeeId)
        {
			List<ValueTuple<int, string>> locationsList = [];
			foreach (var location in DataBaseClient.GetInstance().SelectNamedLocations())
			{
				locationsList.Add((location.Key, location.Value));
			}
			ViewBag.Locations = locationsList;
            
            return View(DataBaseClient.GetInstance().SelectEmployee(employeeId));
        }

        [HttpPost]
        public IActionResult EmployeeEditor(IEnumerable<string> checkedPermissions, int employeeId, string fullname, string email,string username,string? changePassword = null)
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

            List<(int, string)> locationsList = [];
            foreach (var location in DataBaseClient.GetInstance().SelectNamedLocations())
            {
                locationsList.Add((location.Key, location.Value));
            }
            ViewBag.Locations = locationsList;

            var employee = DataBaseClient.GetInstance().SelectEmployee(employeeId);

            if (employee == null)
            {
                return RedirectToActionPermanent("EmployeeTable","Admin");
            }

            employee.Fullname = fullname;
            employee.Email = email;
            employee.Username = username;
            employee.Permissions = permissions;

            if (changePassword != null)
            {
                string newPassword = PasswordManager.Generate(10);
                employee.PasswordHash = PasswordManager.Hash(newPassword);
                ViewBag.NewPassword = newPassword;
            }
            
            try
            {
                DataBaseClient.GetInstance().Update(employee);
                ViewBag.Success = true;
            }
            catch(Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }

            return View(employee);
        }
    }
}
