using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.Plugin.Shipping.CanadaPost
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.CanadaPost",
                 "Plugins/CanadaPost/{action}",
                 new { controller = "CanadaPost", action = "Configure" },
                 new[] { "SmartStore.CanadaPost.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.CanadaPost";
        }

        public int Priority => 0;
    }
}
