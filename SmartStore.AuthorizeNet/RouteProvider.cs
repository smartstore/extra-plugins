using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.AuthorizeNet
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.AuthorizeNet",
                 "Plugins/SmartStore.AuthorizeNet/{action}",
                 new { controller = "AuthorizeNet" },
                 new[] { "SmartStore.AuthorizeNet.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.AuthorizeNet";
        }
        public int Priority => 0;
    }
}
