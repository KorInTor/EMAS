using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using Model.Event;
using EMAS_Web.Filters;
using Service;
using Service.Connection;
using Service.Connection.DataAccess.Event;

namespace EMAS_Web.Controllers
{
    public class StorableObjectController : Controller
    {

        [LocationFilter]
        public IActionResult Material(int locationId)
        {
            List<MaterialPiece> materialList = [];

            List<Permission> permissionsList = (from prm in DataBaseClient.GetInstance().SelectEmployee(Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))).Permissions
                                                where  prm.LocationId == locationId
                                                select prm).ToList();
            

            foreach (var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is MaterialPiece material)
                {
                    materialList.Add(material);
                }
            }

            ViewBag.PermissionList = permissionsList;
            ViewBag.LocationId = locationId;
            return View(materialList);
        }

        [LocationFilter]
        public IActionResult Equipment(int locationId)
        {
            List<Equipment> equipmentList = [];

            List<Permission> permissionsList = (from prm in DataBaseClient.GetInstance().SelectEmployee(Convert.ToInt32(HttpContext.Session.GetInt32("UserId"))).Permissions
                                                where prm.LocationId == locationId
                                                select prm).ToList();


            foreach (var item in DataBaseClient.GetInstance().SelectStorableObjectOn(locationId))
            {
                if (item is Equipment equipment)
                {
                    equipmentList.Add(equipment);
                }
            }
            ViewBag.PermissionList = permissionsList;
            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            ViewBag.LocationId = locationId;
            return View(equipmentList);
        }

        [AuthorizationFilter]
        [LocationFilter]
        public IActionResult AddEquipment(int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(Equipment));

            return View();
        }

        [HttpPost]
        [AuthorizationFilter]
        [LocationFilter]
        public IActionResult AddEquipment(Equipment newEquipment, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();

            try
            {
                var addition = new AdditionEvent((int)userId, 0, EventType.Addition, DateTime.Now, [newEquipment], locationId);
                DataBaseClient.GetInstance().Add(addition);
            }
            catch (Exception excpetion)
            {
                ViewBag.ErrorMessage = excpetion.Message;
                ViewBag.NoError = false;
                return View();
            }
            ViewBag.NoError = true;

            return RedirectToActionPermanent("Equipment", "StorableObject", new { locationId = locationId});
        }

        [LocationFilter]
        public IActionResult Index(int locationId)
        {
            //TODO Combine Equipment And Material Tables Here.
            return View();
        }

        public IActionResult History(int storableObjectId)
        {
            return View(DataBaseClient.GetInstance().SelectForStorableObjectId(storableObjectId));
        }

    }
}