using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;

namespace SmartStore.Glimpse
{

    public class GlimpsePlugin : BasePlugin, IConfigurable
    {
        private readonly GlimpseSettings _settings;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;

        public GlimpsePlugin(GlimpseSettings settings, ILocalizationService localizationService, ISettingService settingService)
        {
            _settings = settings;
            _localizationService = localizationService;
            _settingService = settingService;
        }

        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "Glimpse";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.Glimpse" } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _localizationService.ImportPluginResourcesFromXml(this.PluginDescriptor);

            var settings = new GlimpseSettings()
            {
                IsEnabled = false,
                ShowConsoleInAdminArea = false,
                AllowAdministratorsOnly = true,
                EnableOnRemoteServer = true,
                ShowConfigurationTab = true,
                ShowEnvironmentTab = true,
                ShowExecutionTab = true,
                ShowMetadataTab = true,
                ShowRequestTab = true,
                ShowModelBindingTab = true,
                ShowRoutesTab = true,
                ShowServerTab = true,
                ShowSessionTab = false,
                ShowSqlTab = false,
                ShowTimelineTab = true,
                ShowTraceTab = true,
                ShowViewsTab = true
            };
            _settingService.SaveSetting(settings);

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //locales
            _localizationService.DeleteLocaleStringResources(this.PluginDescriptor.ResourceRootKey);
            _localizationService.DeleteLocaleStringResources("Plugins.FriendlyName.Developer.Glimpse", false);

            _settingService.DeleteSetting<GlimpseSettings>();

            base.Uninstall();
        }
    }
}
