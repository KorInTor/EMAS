using EMAS_Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using Model.Event;
using Service;
using Service.Connection;
using Service.Connection.DataAccess;

namespace EMAS_Web.Controllers
{
    public class StorableObjectController : Controller
    {


        public IActionResult Material(int locationId = 1)
        {
            List<MaterialPiece> materialList = [];

            foreach (var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is MaterialPiece material)
                {
                    materialList.Add(material);
                }
            }

            ViewBag.LocationId = locationId;
            return View(materialList);
        }
        
        public IActionResult Equipment(int locationId = 1)
        {
            EmployeeDataAccess currentUserInfo = new EmployeeDataAccess();

            List<Equipment> equipmentList = [];
            List<Permission> permissions = (from prm in currentUserInfo.SelectEmployeePermissionsList(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")))
                                           where prm.PermissionType.ToString().StartsWith("Equipment") && prm.LocationId == locationId select prm).ToList();

            foreach (var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is Equipment equipment)
                {
                    equipmentList.Add(equipment);
                }
            }
            List<string> permissionsStrings = [];
            foreach(var permission in permissions)
            {
                switch(permission.PermissionType.ToString())
                {
                    case "EquipmentAdd":
                        permissionsStrings.Add("Добавить");
                        break;
                    case "EquipemntDelete":
                        permissionsStrings.Add("Удалить");
                        break;
                    case "EquipemntEdit":
                        permissionsStrings.Add("Изменить");
                        break;
                }
            }

            ViewBag.LocationId = locationId;
            ViewBag.PermissionNames = permissionsStrings;
            ViewBag.Permissions = permissions;
            return View(equipmentList);
        }
       

        public IActionResult AddEquipment()
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public IActionResult AddEquipment(Equipment newEquipment, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            try
            {
                var addition = new AdditionEvent((int)userId, 0, EventType.Addition, DateTime.Now, [newEquipment], locationId);
                DataBaseClient.GetInstance().Add(addition);
            }
            catch(Exception excpetion)
            {
                ViewBag.ErrorMessage = excpetion.Message;
                ViewBag.NoError = false;
                return View();
            }
            ViewBag.NoError = true;
            return View();
        }

        public IActionResult Index(int locationId)
        {
            return View();
        }

        public IActionResult History(int storableObjectId)
        {
            return View(DataBaseClient.GetInstance().SelectForStorableObjectId(storableObjectId));
        }

        /*
        public IActionResult Equipment(int locationId = 1)
        {
            EmployeeDataAccess currentUserInfo = new EmployeeDataAccess();
            int checkId = Convert.ToInt32(HttpContext.Session.GetInt32("UserId"));
            var permissionsRaw = currentUserInfo.SelectEmployeePermissionsList(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")));

            // Custom class intended to be used as parcel containing fields required for Equipment Razor Page to have it's data
            EquipmentModel model = new EquipmentModel();
            List<Equipment> equipmentList = new List<Equipment>();

            foreach(var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is Equipment equipment)
                {
                    equipmentList.Add(equipment);
                }
            }

            List<Permission> permissions = (from ps in permissionsRaw where ps.LocationId == locationId && ps.PermissionType.ToString().StartsWith("Equipment") select ps).ToList();



            model.Equipment = equipmentList;
            model.Permissions = permissions;
            return View(model); 
        }
        */
    }
}
