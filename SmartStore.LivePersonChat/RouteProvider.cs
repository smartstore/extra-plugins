using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.LivePersonChat
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.LivePersonChat",
                 "Plugins/SmartStore.LivePersonChat/{action}",
                 new { controller = "WidgetsLivePersonChat", action = "Configure" },
                 new[] { "SmartStore.LivePersonChat.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.LivePersonChat";
        }

        public int Priority => 0;
    }
}
