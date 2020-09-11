using System.Web.Mvc;
using System.Web.Routing;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.MailChimp
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("SmartStore.MailChimp",
                "Plugins/SmartStore.MailChimp/{action}",
                new { controller = "MailChimp", action = "Configure" },
                new[] { "SmartStore.MailChimp.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.MailChimp";

            routes.MapRoute("SmartStore.MailChimp.WebHook",
                "Plugins/SmartStore.MailChimp/WebHook/{webHookKey}",
                new { controller = "WebHooks", action = "index" },
                new[] { "SmartStore.MailChimp.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.MailChimp";

            // Legacy
            routes.MapRoute("SmartStore.MailChimp.WebHook.Legacy",
                "Plugins/MiscMailChimp/WebHook/{webHookKey}",
                new { controller = "WebHooks", action = "index" },
                new[] { "SmartStore.MailChimp.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.MailChimp";
        }

        public int Priority => 0;

    }
}