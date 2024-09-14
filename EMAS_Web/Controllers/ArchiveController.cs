using Microsoft.AspNetCore.Mvc;
using Model.Event;
using Service.Connection;
using Service.Connection.DataAccess.QueryBuilder;

namespace EMAS_Web.Controllers
{
    public class ArchiveController : Controller
    {
        [HttpGet]
        public IActionResult Index(int locationId = 1, DateTime? floorValue = null, DateTime? ceilingValue = null)
        { 
            return View(DataBaseClient.GetInstance().SelectLocationBoundedEvents(locationId, floorValue, ceilingValue));
        }
    }
}
