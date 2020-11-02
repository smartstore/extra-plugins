using System.Web.Mvc;
using SmartStore.CanadaPost.Models;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.CanadaPost.Controllers
{
    [AdminAuthorize]
    public class CanadaPostController : PluginControllerBase
    {
        private readonly CanadaPostSettings _canadaPostSettings;
        private readonly ISettingService _settingService;

        public CanadaPostController(CanadaPostSettings canadaPostSettings, ISettingService settingService)
        {
            this._canadaPostSettings = canadaPostSettings;
            this._settingService = settingService;
        }

        public ActionResult Configure()
        {
            var model = new CanadaPostModel();
            model.Url = _canadaPostSettings.Url;
            model.Port = _canadaPostSettings.Port;
            model.CustomerId = _canadaPostSettings.CustomerId;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(CanadaPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            //save settings
            _canadaPostSettings.Url = model.Url;
            _canadaPostSettings.Port = model.Port;
            _canadaPostSettings.CustomerId = model.CustomerId.TrimSafe();
            _settingService.SaveSetting(_canadaPostSettings);

            return RedirectToConfiguration("SmartStore.CanadaPost", false);
        }
    }
}
