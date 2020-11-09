using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.AustraliaPost
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.AustraliaPost",
                 "Plugins/ShippingAustraliaPost/{action}",
                 new { controller = "AustraliaPost", action = "Configure" },
                 new[] { "SmartStore.AustraliaPost.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.AustraliaPost";
        }

        public int Priority => 0;
    }
}
