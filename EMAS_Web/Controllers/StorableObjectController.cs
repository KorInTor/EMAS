using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Model;
using Model.Event;
using EMAS_Web.Filters;
using Service;
using Service.Connection;
using Service.Connection.DataAccess.Event;
using Microsoft.AspNetCore.Http.Extensions;

namespace EMAS_Web.Controllers
{
    [AuthorizationFilter]
    [LocationFilter]
    public class StorableObjectController : Controller
    {
        [HttpGet]
        public IActionResult AddEquipment(int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(Equipment));

            return View("SingleEquipment");
        }

        [HttpPost]
        public IActionResult AddEquipment(Equipment newEquipment, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();

            try
            {
                var addition = new AdditionEvent((int)userId, 0, EventType.Addition, DateTime.Now, [newEquipment], locationId);
                DataBaseClient.GetInstance().AddSingle(addition);
            }
            catch (Exception excpetion)
            {
                TempData["AlertMessage"] = excpetion.Message;
                return View("SingleEquipment");
            }
            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId});
        }

        [HttpGet]
        public IActionResult AddMaterial(int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(MaterialPiece));

            return View("SingleMaterial");
        }

        [HttpPost]
        public IActionResult AddMaterial(MaterialPiece newMaterial, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            try
            {
                var addition = new AdditionEvent((int)userId, 0, EventType.Addition, DateTime.Now, [newMaterial], locationId);
                DataBaseClient.GetInstance().AddSingle(addition);
            }
            catch (Exception excpetion)
            {
                TempData["AlertMessage"] = excpetion.Message;
                return View("SingleMaterial");
            }

            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
        }

        [HttpGet]
        public IActionResult EditEquipment(int objectId, int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            Equipment? oldEquipment = (Equipment?)DataBaseClient.GetInstance().SelectStorableObjectsByIds([objectId]).FirstOrDefault();
			ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
			if (oldEquipment == null)
            {
                TempData["AlertMessage"] = "Такого оборудования нет в Базе данных";
                return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
            }

            ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(Equipment));

            return View("SingleEquipment", oldEquipment);
        }

        [HttpPost]
        public IActionResult EditEquipment(Equipment updatedEquipment, int locationId, string comment)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
			ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
			Equipment? oldEquipment = (Equipment?)DataBaseClient.GetInstance().SelectStorableObjectsByIds([updatedEquipment.Id]).FirstOrDefault();
			if (oldEquipment == null)
			{
				TempData["AlertMessage"] = "Такого оборудования нет в Базе данных";
				return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
			}

			ViewBag.Statuses = DataBaseClient.GetInstance().SelectEquipmentStatuses();
            var dataChangedEvent = new DataChangedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.DataChanged, DateTime.UtcNow, [updatedEquipment], comment);
            try
            {
                DataBaseClient.GetInstance().AddSingle(dataChangedEvent);
            }
            catch (Exception excpetion)
            {
                TempData["AlertMessage"] = excpetion.Message;
                return View("SingleEquipment");
            }
            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
        }

        [HttpGet]
        public IActionResult EditMaterial(int objectId,int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав
            MaterialPiece? oldMaterial = (MaterialPiece?)DataBaseClient.GetInstance().SelectStorableObjectsByIds([objectId]).FirstOrDefault();

			if (oldMaterial == null)
			{
				TempData["AlertMessage"] = "Такого материала нет в Базе данных";
				return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
			}

			ViewBag.DistinctValues = DataBaseClient.GetInstance().SelectDistinctPropertyValues(typeof(MaterialPiece));

            return View("SingleMaterial", oldMaterial);
        }

        [HttpPost]
        public IActionResult EditMaterial(MaterialPiece updatedMaterial, int locationId, string comment)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            MaterialPiece? oldMaterial = (MaterialPiece?)DataBaseClient.GetInstance().SelectStorableObjectsByIds([updatedMaterial.Id]).FirstOrDefault();

			if (oldMaterial == null)
            {
                TempData["AlertMessage"] = "Такого материала нет в Базе данных";
                return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
            }

            var dataChangedEvent = new DataChangedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.DataChanged, DateTime.UtcNow, [updatedMaterial],comment);

            try
            {
                DataBaseClient.GetInstance().AddSingle(dataChangedEvent);
            }
            catch (Exception excpetion)
            {
                TempData["AlertMessage"] = excpetion.Message;
                return View("SingleMaterial");
            }

            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
        }

        [AllowSpecialLocationId]
        public IActionResult Index(int locationId)
        {
			ViewBag.PermissionList = (from prm in DataBaseClient.GetInstance().SelectByIds<Employee>([(int)HttpContext.Session.GetInt32("UserId")], nameof(Employee.Id)).First().Permissions
												where prm.LocationId == locationId
												select prm).OrderBy(x => (int)x.PermissionType);

            List<IStorableObject> storableObjectList = [];

            if(locationId == LocationFilter.AllLocationCase)
            {
                foreach(var location in DataBaseClient.GetInstance().SelectNamedLocations())
                {
                    storableObjectList.AddRange(DataBaseClient.GetInstance().SelectStorableObjectOn(location.Key));
                }
            }
            else
            {
                storableObjectList = DataBaseClient.GetInstance().SelectStorableObjectOn(locationId);
            }
            
			ViewBag.LastEvents = DataBaseClient.GetInstance().SelectLastEventsForStorableObjects(storableObjectList);
            ViewBag.LocationId = locationId;
            return View(storableObjectList);
        }

    }
}