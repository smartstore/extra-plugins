using System.Web.Mvc;
using SmartStore.LivePersonChat.Models;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.LivePersonChat.Controllers
{

    public class WidgetsLivePersonChatController : PluginControllerBase
    {
        private readonly LivePersonChatSettings _livePersonChatSettings;
        private readonly ISettingService _settingService;

        public WidgetsLivePersonChatController(LivePersonChatSettings livePersonChatSettings, ISettingService settingService)
        {
            this._livePersonChatSettings = livePersonChatSettings;
            this._settingService = settingService;
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
            var model = new PublicInfoModel();
            model.MonitoringCode = _livePersonChatSettings.MonitoringCode;

            return View(model);
        }
    }
}