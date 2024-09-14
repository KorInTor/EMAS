using Microsoft.AspNetCore.Mvc;

namespace EMAS_Web.Controllers
{
    public class EquipmentController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AddToReservedList()
        {

        return View(); 
        }
    }
}
