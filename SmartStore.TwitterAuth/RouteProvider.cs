using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.TwitterAuth
{
    public partial class RouteProvider : IRouteProvider
    {
        public int Priority => 0;

        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute(TwitterExternalAuthMethod.SystemName,
                 "Plugins/SmartStore.TwitterAuth/{action}",
                 new { controller = "ExternalAuthTwitter", action = "Configure" },
                 new[] { "SmartStore.TwitterAuth.Controllers" }
            )
            .DataTokens["area"] = TwitterExternalAuthMethod.SystemName;
        }
    }
}
