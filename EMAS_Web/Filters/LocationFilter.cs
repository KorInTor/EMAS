using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Service.Connection;

namespace EMAS_Web.Filters
{
    public class LocationFilter : Attribute, IResourceFilter
    {
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var locationIdString = context.HttpContext.Request.Query["locationId"].ToString();
            if (string.IsNullOrEmpty(locationIdString))
            {
                context.Result = new ViewResult
                {
                    ViewName = "Error/InvalidLocation"
                };
                return;
            }

            if (!Int32.TryParse(locationIdString, out int locationId))
            {
                context.Result = new ViewResult
                {
                    ViewName = "Error/InvalidLocation"
                };
                return;
            }

            if (!IsValidLocationId(locationId))
            {
                context.Result = new ViewResult
                {
                    ViewName = "Error/InvalidLocation"
                };
                return;
            }
        }

        private bool IsValidLocationId(int locationId)
        {
            var locations = DataBaseClient.GetInstance().SelectNamedLocations();
            return locations.ContainsKey(locationId);
        }

        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            
        }
    }
}
