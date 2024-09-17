using Microsoft.AspNetCore.Mvc;
using Model;
using Service.Connection;

namespace EMAS_Web.Components
{
    [ViewComponent]
    public class LocationsComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            List<string> locations = [];
            foreach(Location loc in DataBaseClient.GetInstance().SelectLocations())
            {
                locations.Add(loc.Name);

            }
           
            return View("LocationSwitch", locations);
        }
    }
}
