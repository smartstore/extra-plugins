using System.Collections.Generic;
using System.Web.Routing;
using SmartStore.Core.Domain.Cms;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Cms;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.LivePersonChat
{
	public class LivePersonChatPlugin : BasePlugin, IWidget, IConfigurable, ICookiePublisher
    {
        private readonly LivePersonChatSettings _livePersonChatSettings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly IWidgetService _widgetService;
        private readonly WidgetSettings _widgetSettings;
        private readonly ICommonServices _services;

        public LivePersonChatPlugin(
            LivePersonChatSettings livePersonChatSettings, 
            ILocalizationService localizationService, 
            ISettingService settingService,
            IWidgetService widgetService,
            WidgetSettings widgetSettings,
            ICommonServices services)
        {
            _livePersonChatSettings = livePersonChatSettings;
            _localizationService = localizationService;
            _settingService = settingService;
            _widgetService = widgetService;
            _widgetSettings = widgetSettings;
            _services = services;
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
        /// Gets CookieInfos for display in CookieManager dialog.
        /// </summary>
        /// <returns>CookieInfo containing plugin name, cookie purpose description & cookie type</returns>
        public IEnumerable<CookieInfo> GetCookieInfo()
        {
            var widget = _widgetService.LoadWidgetBySystemName("SmartStore.LivePersonChat");
            if (!widget.IsWidgetActive(_widgetSettings))
                return null;

            var cookieInfo = new CookieInfo
            {
                Name = _services.Localization.GetResource("Plugins.FriendlyName.SmartStore.LivePersonChat"),
                Description = _services.Localization.GetResource("Plugins.Payments.AmazonPay.CookieInfo"),
                CookieType = CookieType.Required
            };

            return new List<CookieInfo> { cookieInfo };
        }

        public override void Install()
        {
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

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
