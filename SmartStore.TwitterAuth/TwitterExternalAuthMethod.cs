using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Authentication.External;
using SmartStore.Services.Localization;

namespace SmartStore.TwitterAuth
{
    public class TwitterExternalAuthMethod : BasePlugin, IExternalAuthenticationMethod, IConfigurable
    {
        private readonly ILocalizationService _localizationService;

        public TwitterExternalAuthMethod(
            ILocalizationService localizationService)
        {
            _localizationService = localizationService;
        }

        public static string SystemName => "SmartStore.TwitterAuth";

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "ExternalAuthTwitter";
            routeValues = new RouteValueDictionary(new { Namespaces = "SmartStore.TwitterAuth.Controllers", area = SystemName });
        }

        public void GetPublicInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "ExternalAuthTwitter";
            routeValues = new RouteValueDictionary(new { Namespaces = "SmartStore.TwitterAuth.Controllers", area = SystemName });
        }

        public override void Install()
        {
            _localizationService.ImportPluginResourcesFromXml(PluginDescriptor);

            base.Install();
        }

        public override void Uninstall()
        {
            _localizationService.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

            base.Uninstall();
        }
    }
}
