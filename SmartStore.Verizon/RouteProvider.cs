using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Verizon
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.Verizon",
                 "Plugins/SmartStore.Verizon/{action}",
                 new { controller = "SmsVerizon", action = "Configure" },
                 new[] { "SmartStore.Verizon.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.Verizon";
        }
        public int Priority => 0;
    }
}
