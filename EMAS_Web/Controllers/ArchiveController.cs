using EMAS_Web.Filters;
using Microsoft.AspNetCore.Mvc;
using Model.Event;
using Service.Connection;


namespace EMAS_Web.Controllers
{
    public class ArchiveController : Controller
    {
        [HttpGet]
        [LocationFilter]
        public IActionResult Index(int locationId = 1, DateTime? floorValue = null, DateTime? ceilingValue = null)
        { 
            return View(DataBaseClient.GetInstance().SelectLocationBoundedEvents(locationId, floorValue, ceilingValue));
        }
    }
}
