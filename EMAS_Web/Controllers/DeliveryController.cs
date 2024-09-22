using EMAS_Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Model.Event;
using Service.Connection;
using System.Diagnostics;

namespace EMAS_Web.Controllers
{
    [AuthorizationFilter]
    public class DeliveryController : Controller
    {
        [LocationFilter]
        public IActionResult Index(int locationId, bool selectOutgoing = false)
        {
            ViewBag.Outgoing = selectOutgoing;
            ViewBag.LocationId = locationId;
            ViewBag.LocationDictionary = DataBaseClient.GetInstance().SelectNamedLocations();
            ViewBag.HasPermission = DataBaseClient.GetInstance().SelectEmployee(Convert.ToInt32(HttpContext.Session.GetInt32("UserId")))
                .Permissions
                .Where(x => x.LocationId == locationId)
                .Where(x => x.PermissionType == Model.Enum.PermissionType.DeliveryAccess)
                .Any();

            return View(DataBaseClient.GetInstance().SelectDeliveries(locationId, !selectOutgoing));
        }

        [HttpGet]
        public IActionResult Create(IEnumerable<string> selectedIds, int departureId)
        {

            if (selectedIds == null || !selectedIds.Any())
            {
                ViewBag.Message = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdsList = selectedIds.Select(int.Parse).ToList();

            ViewBag.SelectedObjects = DataBaseClient.GetInstance().SelectStorableObjectsByIds(selectedIdsList);
            ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.Locations = DataBaseClient.GetInstance().SelectNamedLocations();

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
                ViewBag.Message = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdsList = selectedIds.Select(int.Parse);

            var storableObjects = DataBaseClient.GetInstance().SelectStorableObjectsByIds(selectedIdsList);

            var sentEvent = new SentEvent((int)HttpContext.Session.GetInt32("UserId"),0,EventType.Sent,dateTime, storableObjects, comment, departureId, destinationId);

            DataBaseClient.GetInstance().Add(sentEvent);

            return RedirectToActionPermanent("Index", "Delivery", new { locationId = departureId, selectOutgoing = true });
        }

        [HttpGet]
        public IActionResult Confirm(long sentEventId)
        {
            var sentEventToConfirm = DataBaseClient.GetInstance().SelectEventsByIds<SentEvent>([sentEventId]).First();

            ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.MinDateTimeString = sentEventToConfirm.DateTime.ToString("yyyy-MM-ddTHH:mm");

            return View(sentEventToConfirm);
        }

        [HttpPost]
        public IActionResult Confirm(long sentEventId, string comment, DateTime dateTime)
        {
            var sentEventToConfirm = DataBaseClient.GetInstance().SelectEventsByIds<SentEvent>([sentEventId]).First();

            var arrivedEvent = new ArrivedEvent((int)HttpContext.Session.GetInt32("UserId"),0,EventType.Arrived,dateTime.ToUniversalTime(), sentEventToConfirm.ObjectsInEvent, comment,sentEventId);

            try
            {
                DataBaseClient.GetInstance().Add(arrivedEvent);
            }
            catch(Exception ex)
            {
                ViewBag.Error = true;
                ViewBag.ErrorMessage = ex.Message;
                return View(sentEventToConfirm);
            }

            return RedirectToActionPermanent("Index", "Delivery", new { locationId = sentEventToConfirm.DestinationId, selectIncoming = true });
        }
    }
}
