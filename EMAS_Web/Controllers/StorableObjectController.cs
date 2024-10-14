using EMAS_Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Event;
using Service.Connection;

namespace EMAS_Web.Controllers
{
    [AuthorizationFilter]
    [LocationFilter]
    public class StorableObjectController : Controller
    {
        private readonly DataBaseClient _dataBaseClient;

        public StorableObjectController(DataBaseClient dataBaseClient)
        {
            _dataBaseClient = dataBaseClient;
        }

        [HttpGet]
        public IActionResult AddEquipment(int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            ViewBag.Statuses = _dataBaseClient.SelectEquipmentStatuses();
            ViewBag.DistinctValues = _dataBaseClient.SelectDistinctPropertyValues(typeof(Equipment));

            return View("SingleEquipment");
        }

        [HttpPost]
        public IActionResult AddEquipment(Equipment newEquipment, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            ViewBag.Statuses = _dataBaseClient.SelectEquipmentStatuses();
            var addition = new AdditionEvent((int)userId, 0, EventType.Addition, DateTime.Now, [newEquipment], locationId);

            try
            {
                _dataBaseClient.AddSingle(addition);
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
        public IActionResult AddMaterial(int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав

            ViewBag.DistinctValues = _dataBaseClient.SelectDistinctPropertyValues(typeof(MaterialPiece));

            return View("SingleMaterial");
        }

        [HttpPost]
        public IActionResult AddMaterial(MaterialPiece newMaterial, int locationId)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            try
            {
                var addition = new AdditionEvent((int)userId, 0, EventType.Addition, DateTime.Now, [newMaterial], locationId);
                _dataBaseClient.AddSingle(addition);
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

            Equipment? oldEquipment = (Equipment?)_dataBaseClient.SelectStorableObjectsByIds([objectId]).FirstOrDefault();
            ViewBag.Statuses = _dataBaseClient.SelectEquipmentStatuses();
            if (oldEquipment == null)
            {
                TempData["AlertMessage"] = "Такого оборудования нет в Базе данных";
                return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
            }

            ViewBag.DistinctValues = _dataBaseClient.SelectDistinctPropertyValues(typeof(Equipment));

            return View("SingleEquipment", oldEquipment);
        }

        [HttpPost]
        public IActionResult EditEquipment(Equipment updatedEquipment, int locationId, string comment)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            ViewBag.Statuses = _dataBaseClient.SelectEquipmentStatuses();
            Equipment? oldEquipment = (Equipment?)_dataBaseClient.SelectStorableObjectsByIds([updatedEquipment.Id]).FirstOrDefault();
            if (oldEquipment == null)
            {
                TempData["AlertMessage"] = "Такого оборудования нет в Базе данных";
                return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
            }

            ViewBag.Statuses = _dataBaseClient.SelectEquipmentStatuses();
            var dataChangedEvent = new DataChangedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.DataChanged, DateTime.UtcNow, [updatedEquipment], comment);
            try
            {
                _dataBaseClient.AddSingle(dataChangedEvent);
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
        public IActionResult EditMaterial(int objectId, int locationId)
        {
            //TODO Доабвить server side провепрку на наличие прав
            MaterialPiece? oldMaterial = (MaterialPiece?)_dataBaseClient.SelectStorableObjectsByIds([objectId]).FirstOrDefault();

            if (oldMaterial == null)
            {
                TempData["AlertMessage"] = "Такого материала нет в Базе данных";
                return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
            }

            ViewBag.DistinctValues = _dataBaseClient.SelectDistinctPropertyValues(typeof(MaterialPiece));

            return View("SingleMaterial", oldMaterial);
        }

        [HttpPost]
        public IActionResult EditMaterial(MaterialPiece updatedMaterial, int locationId, string comment)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");

            MaterialPiece? oldMaterial = (MaterialPiece?)_dataBaseClient.SelectStorableObjectsByIds([updatedMaterial.Id]).FirstOrDefault();

            if (oldMaterial == null)
            {
                TempData["AlertMessage"] = "Такого материала нет в Базе данных";
                return RedirectToActionPermanent("Index", "StorableObject", new { locationId = locationId });
            }

            var dataChangedEvent = new DataChangedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.DataChanged, DateTime.UtcNow, [updatedMaterial], comment);

            try
            {
                _dataBaseClient.AddSingle(dataChangedEvent);
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
            ViewBag.PermissionList = (from prm in _dataBaseClient.SelectByIds<Employee>([(int)HttpContext.Session.GetInt32("UserId")], nameof(Employee.Id)).First().Permissions
                                      where prm.LocationId == locationId
                                      select prm).OrderBy(x => (int)x.PermissionType);

            List<IStorableObject> storableObjectList = [];

            if (locationId == LocationFilter.SpecialLocationId)
            {
                foreach (var location in _dataBaseClient.SelectNamedLocations())
                {
                    storableObjectList.AddRange(_dataBaseClient.SelectStorableObjectOn(location.Key));
                }
            }
            else
            {
                storableObjectList = _dataBaseClient.SelectStorableObjectOn(locationId);
            }

            ViewBag.LastEvents = _dataBaseClient.SelectLastEventsForStorableObjects(storableObjectList);
            ViewBag.LocationId = locationId;
            return View(storableObjectList);
        }

    }
}