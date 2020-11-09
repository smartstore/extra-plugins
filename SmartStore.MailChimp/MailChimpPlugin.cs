using System.Web.Routing;
using SmartStore.Core.Plugins;
using SmartStore.MailChimp.Services;

namespace SmartStore.MailChimp
{
    public class MailChimpPlugin : BasePlugin, IConfigurable
    {
        private readonly MailChimpInstallationService _mailChimpInstallationService;
        private readonly MailChimpSettings _mailChimpSettings;

        public MailChimpPlugin(MailChimpInstallationService mailChimpInstallationService, MailChimpSettings mailChimpSettings)
        {
            this._mailChimpInstallationService = mailChimpInstallationService;
            this._mailChimpSettings = mailChimpSettings;
        }

        /// <summary>
        /// Is plugin configured?
        /// </summary>
        /// <returns></returns>
        public virtual bool IsConfigured()
        {
            return !string.IsNullOrEmpty(_mailChimpSettings.ApiKey);
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            _mailChimpInstallationService.Install(this);
            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            _mailChimpInstallationService.Uninstall(this);
            base.Uninstall();
        }

        /// <summary>
        /// Gets a route for plugin configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "MailChimp";
            routeValues = new RouteValueDictionary { { "area", "SmartStore.MailChimp" } };
        }
    }
}