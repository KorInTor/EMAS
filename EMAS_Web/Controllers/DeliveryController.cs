using EMAS_Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Model;
using Model.Event;
using Service.Connection;

namespace EMAS_Web.Controllers
{
    [AuthorizationFilter]
    public class DeliveryController : Controller
    {
        private readonly DataBaseClient _dataBaseClient;

        public DeliveryController(DataBaseClient dataBaseClient)
        {
            _dataBaseClient = dataBaseClient;
        }
        [LocationFilter]
        public IActionResult Index(int locationId, bool selectOutgoing = false)
        {
            ViewBag.Outgoing = selectOutgoing;
            ViewBag.LocationId = locationId;
            ViewBag.LocationDictionary = _dataBaseClient.SelectNamedLocations();
            ViewBag.HasPermission = _dataBaseClient.SelectByIds<Employee>([(int)HttpContext.Session.GetInt32("UserId")], nameof(Employee.Id)).First()
                .Permissions
                .Where(x => x.LocationId == locationId)
                .Where(x => x.PermissionType == Model.Enum.PermissionType.DeliveryAccess)
                .Any();

            return View(_dataBaseClient.SelectDeliveries(locationId, !selectOutgoing));
        }

        [HttpGet]
        public IActionResult Create(IEnumerable<string> selectedIds, int departureId)
        {

            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["AlertMessage"] = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdsList = selectedIds.Select(int.Parse).ToList();

            ViewBag.SelectedObjects = _dataBaseClient.SelectStorableObjectsByIds(selectedIdsList);
            ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.Locations = _dataBaseClient.SelectNamedLocations();

            foreach (var location in ViewBag.Locations)
            {
                if (location.Key == departureId)
                {
                    ViewBag.DepartureLocation = location;
                    ViewBag.Locations.Remove(location.Key);
                    break;
                }
            }

            return View();
        }

        [HttpPost]
        public IActionResult Create(IEnumerable<string> selectedIds, DateTime dateTime, string comment, int destinationId, int departureId)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["AlertMessage"] = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdsList = selectedIds.Select(int.Parse);

            var storableObjects = _dataBaseClient.SelectStorableObjectsByIds(selectedIdsList);

            var sentEvent = new SentEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.Sent, dateTime, storableObjects, comment, departureId, destinationId);

            try
            {
                _dataBaseClient.AddSingle(sentEvent);
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = ex.Message;
            }

            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "Delivery", new { locationId = departureId, selectOutgoing = true });
        }

        [HttpGet]
        public IActionResult Confirm(long sentEventId)
        {
            var sentEventToConfirm = _dataBaseClient.SelectEventsByIds<SentEvent>([sentEventId]).First();

            ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.MinDateTimeString = sentEventToConfirm.DateTime.ToString("yyyy-MM-ddTHH:mm");

            return View(sentEventToConfirm);
        }

        [HttpPost]
        public IActionResult Confirm(long sentEventId, string comment, DateTime dateTime)
        {
            var sentEventToConfirm = _dataBaseClient.SelectEventsByIds<SentEvent>([sentEventId]).First();

            var arrivedEvent = new ArrivedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.Arrived, dateTime.ToUniversalTime(), sentEventToConfirm.ObjectsInEvent, comment, sentEventId);

            try
            {
                _dataBaseClient.AddSingle(arrivedEvent);
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = ex.Message;
                return View(sentEventToConfirm);
            }

            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "Delivery", new { locationId = sentEventToConfirm.DestinationId, selectIncoming = true });
        }
    }
}
