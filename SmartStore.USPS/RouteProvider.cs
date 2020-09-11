using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.USPS
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.USPS",
                 "Plugins/ShippingUSPS/{action}",
                 new { controller = "USPS", action = "Configure" },
                 new[] { "SmartStore.USPS.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.USPS";
        }

        public int Priority => 0;
    }
}
