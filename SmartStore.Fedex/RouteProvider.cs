using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Fedex
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.FedEx",
                 "Plugins/Fedex/Configure",
                 new { controller = "Fedex", action = "Configure" },
                 new[] { "SmartStore.Fedex.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.FedEx";
        }

        public int Priority => 0;
    }
}
