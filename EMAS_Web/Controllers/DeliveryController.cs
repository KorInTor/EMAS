using Microsoft.AspNetCore.Mvc;
using Service.Connection;

namespace EMAS_Web.Controllers
{
    public class DeliveryController : Controller
    {
        public IActionResult Index(int locationId = 1)
        {
            return View(DataBaseClient.GetInstance().GetDeliverysOutOf(locationId));
        }

        public IActionResult Confirm(long id)
        {
            return View();
        }
    }
}
