using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.LivePersonChat
{
    /// <summary>
    /// Live person provider
    /// </summary>
	public class LivePersonChatPlugin : BasePlugin, IWidget, IConfigurable
    {
        private readonly LivePersonChatSettings _livePersonChatSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        public LivePersonChatPlugin(LivePersonChatSettings livePersonChatSettings, ILocalizationService localizationService, ISettingService settingService)
        {
            _livePersonChatSettings = livePersonChatSettings;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        /// <summary>
        /// Gets widget zones where this widget should be rendered
        /// </summary>
        /// <returns>Widget zones</returns>
        public IList<string> GetWidgetZones()
        {
            return new List<string>() { "head_html_tag" };
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "WidgetsLivePersonChat";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.LivePersonChat" } };
        }

        /// <summary>
        /// Gets a route for displaying widget
        /// </summary>
        /// <param name="widgetZone">Widget zone where it's displayed</param>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
		public void GetDisplayWidgetRoute(string widgetZone, object model, int storeId, out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PublicInfo";
            controllerName = "WidgetsLivePersonChat";
            routeValues = new RouteValueDictionary()
            {
                {"Namespaces", "SmartStore.LivePersonChat.Controllers"},
                {"area", "SmartStore.LivePersonChat"},
                {"widgetZone", widgetZone}
            };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Widgets.LivePersonChat", false);

            _settingService.DeleteSetting<LivePersonChatSettings>();

            base.Uninstall();
        }
    }
}
