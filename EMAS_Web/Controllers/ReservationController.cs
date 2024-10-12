using EMAS_Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Model.Event;
using Service.Connection;

namespace EMAS_Web.Controllers
{
    [AuthorizationFilter]
    public class ReservationController : Controller
    {
        private readonly DataBaseClient _dataBaseClient;

        public ReservationController(DataBaseClient dataBaseClient)
        {
            _dataBaseClient = dataBaseClient;
        }

        [HttpGet]
        [LocationFilter]
        public IActionResult Index(int locationId)
        {
            return View(_dataBaseClient.SelectReservationOn(locationId));
        }

        [HttpGet]
        [LocationFilter]
        public IActionResult Create(IEnumerable<string> selectedIds, int locationId)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["AlertMessage"] = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdsList = selectedIds.Select(int.Parse);

            ViewBag.SelectedObjects = _dataBaseClient.SelectStorableObjectsByIds(selectedIdsList);
            ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");

            foreach (var location in _dataBaseClient.SelectNamedLocations())
            {
                if (location.Key == locationId)
                {
                    ViewBag.CurrentLocation = (location.Key, location.Value);
                    continue;
                }
            }

            return View();
        }

        [HttpPost]
        [LocationFilter]
        public IActionResult Create(IEnumerable<string> selectedIds, int locationId,DateTime dateTime, string comment)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["AlertMessage"] = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdList = selectedIds.Select(id => int.Parse(id));

            var storableObjects = _dataBaseClient.SelectStorableObjectsByIds(selectedIdList);

            var reservedEvent = new ReservedEvent((int)HttpContext.Session.GetInt32("UserId"),0,EventType.Reserved,dateTime,storableObjects,comment,locationId);

			ViewBag.SelectedObjects = storableObjects;
			ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");

			try
            {
                _dataBaseClient.AddSingle(reservedEvent);
            }
            catch(Exception exception)
            {
                TempData["AlertMessage"] = exception.Message;
                return View();
            }
            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index","Reservation", new { locationId = locationId });
        }

        [HttpGet]
        [LocationFilter]
        public IActionResult Confirm(long reservedEventId)
        {
            var reservedEventToConfirm = _dataBaseClient.SelectEventsByIds<ReservedEvent>([reservedEventId]).First();

            ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.MinDateTimeString = reservedEventToConfirm.DateTime.ToString("yyyy-MM-ddTHH:mm");

            return View(reservedEventToConfirm);
        }

        [HttpPost]
        [LocationFilter]
        public IActionResult Confirm(long reservedEventId, string comment, DateTime dateTime, bool isDecomissioned)
        {
			var reservedEventToConfirm = _dataBaseClient.SelectEventsByIds<ReservedEvent>([reservedEventId]).First();

            ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");
            ViewBag.MinDateTimeString = reservedEventToConfirm.DateTime.ToString("yyyy-MM-ddTHH:mm");

            StorableObjectEvent endEvent;
            if (isDecomissioned)
            {
                endEvent = new ReserveEndedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.ReserveEnded, dateTime.ToUniversalTime(), reservedEventToConfirm.ObjectsInEvent, comment, reservedEventId);
            }
            else
            {
                endEvent = new DecomissionedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.Decommissioned, dateTime.ToUniversalTime(), reservedEventToConfirm.ObjectsInEvent, comment, EventType.Reserved, reservedEventId, reservedEventToConfirm.LocationId);
            }

            try
            {
                
                _dataBaseClient.AddSingle(endEvent);
            }
            catch (Exception ex)
            {
                TempData["AlertMessage"] = ex.Message;
                return View(reservedEventToConfirm);
            }
            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "Reservation", new { locationId = reservedEventToConfirm.LocationId });
        }
    }
}
