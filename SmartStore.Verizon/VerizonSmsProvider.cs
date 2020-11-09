using System;
using System.Web.Routing;
using SmartStore.Core.Domain.Messages;
using SmartStore.Core.Logging;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Messages;

namespace SmartStore.Verizon
{
    /// <summary>
    /// Represents the Verizon SMS provider
    /// </summary>
    public class VerizonSmsProvider : BasePlugin, IConfigurable
    {
        private readonly VerizonSettings _verizonSettings;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly ILogger _logger;
        private readonly ICommonServices _services;

        public VerizonSmsProvider(VerizonSettings verizonSettings,
            IQueuedEmailService queuedEmailService,
            IEmailAccountService emailAccountService,
            ILogger logger,
            ICommonServices services)
        {
            _verizonSettings = verizonSettings;
            _queuedEmailService = queuedEmailService;
            _emailAccountService = emailAccountService;
            _logger = logger;
            _services = services;
        }

        /// <summary>
        /// Sends SMS
        /// </summary>
        /// <param name="text">SMS text</param>
        /// <returns>Result</returns>
        public bool SendSms(string text)
        {
            try
            {
                var emailAccount = _emailAccountService.GetDefaultEmailAccount();
                if (emailAccount == null)
                    throw new Exception(_services.Localization.GetResource("Common.Error.NoEmailAccount"));

                var queuedEmail = new QueuedEmail
                {
                    Priority = 5,
                    From = emailAccount.ToEmailAddress(),
                    To = _verizonSettings.Email,
                    Subject = _services.StoreContext.CurrentStore.Name,
                    Body = text,
                    CreatedOnUtc = DateTime.UtcNow,
                    EmailAccountId = emailAccount.Id
                };

                _queuedEmailService.InsertQueuedEmail(queuedEmail);

                return true;
            }
            catch (Exception exception)
            {
                _logger.ErrorsAll(exception);
                return false;
            }
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
            controllerName = "SmsVerizon";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.Verizon" } };
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            var settings = new VerizonSettings
            {
                Email = "yournumber@vtext.com",
            };

            _services.Settings.SaveSetting(settings);

            _services.Localization.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            _services.Settings.DeleteSetting<VerizonSettings>();

            _services.Localization.DeleteLocaleStringResources(PluginDescriptor.ResourceRootKey);

            base.Uninstall();
        }
    }
}
