using System.Web.Mvc;
using SmartStore.Core.Plugins;
using SmartStore.LivePersonChat.Models;
using SmartStore.Services.Configuration;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.LivePersonChat.Controllers
{
    public class WidgetsLivePersonChatController : PluginControllerBase
    {
        private readonly LivePersonChatSettings _livePersonChatSettings;
        private readonly ISettingService _settingService;
        private readonly ICookieManager _cookieManager;

        public WidgetsLivePersonChatController(
            LivePersonChatSettings livePersonChatSettings, 
            ISettingService settingService,
            ICookieManager cookieManager)
        {
            _livePersonChatSettings = livePersonChatSettings;
            _settingService = settingService;
            _cookieManager = cookieManager;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.MonitoringCode = _livePersonChatSettings.MonitoringCode;

            return View(model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            // save settings
            _livePersonChatSettings.MonitoringCode = model.MonitoringCode;
            _settingService.SaveSetting(_livePersonChatSettings);

            return Configure();
        }

        [ChildActionOnly]
        public ActionResult PublicInfo(string widgetZone)
        {
            var cookiesAllowed = _cookieManager.IsCookieAllowed(this.ControllerContext, CookieType.ThirdParty);
            if (!cookiesAllowed)
                return new EmptyResult();

            var model = new PublicInfoModel();
            model.MonitoringCode = _livePersonChatSettings.MonitoringCode;

            return View(model);
        }
    }
}