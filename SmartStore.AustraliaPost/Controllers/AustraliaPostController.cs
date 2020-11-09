using System.Web.Mvc;
using SmartStore.AustraliaPost.Models;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.AustraliaPost.Controllers
{
    [AdminAuthorize]
    public class AustraliaPostController : PluginControllerBase
    {
        private readonly AustraliaPostSettings _australiaPostSettings;
        private readonly ISettingService _settingService;

        public AustraliaPostController(AustraliaPostSettings australiaPostSettings, ISettingService settingService)
        {
            this._australiaPostSettings = australiaPostSettings;
            this._settingService = settingService;
        }

        public ActionResult Configure()
        {
            var model = new AustraliaPostModel();
            model.GatewayUrl = _australiaPostSettings.GatewayUrl;
            model.ShippedFromZipPostalCode = _australiaPostSettings.ShippedFromZipPostalCode;
            model.AdditionalHandlingCharge = _australiaPostSettings.AdditionalHandlingCharge;
            model.PrimaryStoreCurrencyCode = Services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(AustraliaPostModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            _australiaPostSettings.GatewayUrl = model.GatewayUrl;
            _australiaPostSettings.ShippedFromZipPostalCode = model.ShippedFromZipPostalCode;
            _australiaPostSettings.AdditionalHandlingCharge = model.AdditionalHandlingCharge;
            _settingService.SaveSetting(_australiaPostSettings);

            return View(model);
        }

    }
}
