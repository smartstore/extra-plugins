using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Glimpse
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.Glimpse",
                 "SmartStore.Glimpse/Configure",
                 new { controller = "Glimpse", action = "Configure" },
                 new[] { "SmartStore.Glimpse.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.Glimpse";

            routes.MapRoute("SmartStore.Glimpse.Console",
                 "Glimpse",
                 new { controller = "Glimpse", action = "Index" },
                 new[] { "SmartStore.Glimpse.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.Glimpse";
        }

        public int Priority => 0;
    }
}
