using EMAS_Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Model.Event;
using Service.Connection;

namespace EMAS_Web.Controllers
{
    [AuthorizationFilter]
    public class DecomissionController : Controller
    {
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

            ViewBag.SelectedObjects = DataBaseClient.GetInstance().SelectStorableObjectsByIds(selectedIdsList);
            ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");

            foreach (var location in DataBaseClient.GetInstance().SelectNamedLocations())
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
        public IActionResult Create(IEnumerable<string> selectedIds, int locationId, DateTime dateTime, string comment)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["AlertMessage"] = "Не выбрано ни одного элемента.";
                return View();
            }

            var selectedIdList = selectedIds.Select(int.Parse);

            var storableObjects = DataBaseClient.GetInstance().SelectStorableObjectsByIds(selectedIdList);

            var decomissionedEvent = new DecomissionedEvent((int)HttpContext.Session.GetInt32("UserId"), 0, EventType.Decommissioned, dateTime, storableObjects, comment, locationId);

            ViewBag.SelectedObjects = storableObjects;
            ViewBag.MaxDateTimeString = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm");

            try
            {
                DataBaseClient.GetInstance().AddSingle(decomissionedEvent);
            }
            catch (Exception exception)
            {
                TempData["AlertMessage"] = exception.Message;
                return View();
            }

            TempData["AlertMessage"] = "Успех";
            return RedirectToActionPermanent("Index", "StorableObject");
        }
    }
}
