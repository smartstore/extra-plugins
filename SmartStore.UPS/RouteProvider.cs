using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.UPS
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.UPS",
                 "Plugins/ShippingUPS/{action}",
                 new { controller = "UPS", action = "Configure" },
                 new[] { "SmartStore.UPS.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.UPS";
        }

        public int Priority => 0;
    }
}
