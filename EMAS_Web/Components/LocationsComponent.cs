using Microsoft.AspNetCore.Mvc;
using Model;
using Service.Connection;

namespace EMAS_Web.Components
{
    [ViewComponent]
    public class LocationsComponent : ViewComponent
    {
        public IViewComponentResult Invoke(int? currentLocationId)
        {
            ViewBag.CurrentLocationId = currentLocationId;
            return View("LocationSwitch", DataBaseClient.GetInstance().SelectNamedLocations());
        }
    }
}
