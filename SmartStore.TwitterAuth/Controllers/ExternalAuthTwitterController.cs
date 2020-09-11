using System.Collections.Generic;
using System.Web.Mvc;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Security;
using SmartStore.Services;
using SmartStore.Services.Authentication.External;
using SmartStore.TwitterAuth.Models;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.TwitterAuth.Controllers
{
    public class ExternalAuthTwitterController : PluginControllerBase
    {
        private readonly IOpenAuthenticationService _openAuthenticationService;
        private readonly ExternalAuthenticationSettings _externalAuthenticationSettings;

        public ExternalAuthTwitterController(
            IOpenAuthenticationService openAuthenticationService,
            ExternalAuthenticationSettings externalAuthenticationSettings)
        {
            _openAuthenticationService = openAuthenticationService;
            _externalAuthenticationSettings = externalAuthenticationSettings;
        }

        [AdminAuthorize, ChildActionOnly, LoadSetting]
        [Permission(Permissions.Configuration.Authentication.Read)]
        public ActionResult Configure(TwitterExternalAuthSettings settings)
        {
            var model = new ConfigurationModel();
            model.ConsumerKey = settings.ConsumerKey;
            model.ConsumerSecret = settings.ConsumerSecret;
            model.CallbackUrls = new List<string>();

            var store = Services.StoreContext.CurrentStore;
            var storeLocation = Services.WebHelper.GetStoreLocation(false);

            model.CallbackUrls.Add(Services.WebHelper.GetStoreLocation(false) + "Plugins/SmartStore.TwitterAuth/LoginCallback/");

            if (store.SslEnabled)
            {
                model.CallbackUrls.Add(Services.WebHelper.GetStoreLocation(true) + "Plugins/SmartStore.TwitterAuth/LoginCallback/");
            }
            else
            {
                model.CallbackUrls.Add(Services.WebHelper.GetStoreLocation(false).EnsureEndsWith("/"));
            }

            return View(model);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly, SaveSetting]
        [Permission(Permissions.Configuration.Authentication.Update)]
        public ActionResult Configure(ConfigurationModel model, TwitterExternalAuthSettings settings)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }

            settings.ConsumerKey = model.ConsumerKey.TrimSafe();
            settings.ConsumerSecret = model.ConsumerSecret.TrimSafe();

            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

            return RedirectToConfiguration(TwitterExternalAuthMethod.SystemName);
        }

        [ChildActionOnly]
        public ActionResult PublicInfo()
        {
            return View();
        }

        public ActionResult Login(string returnUrl)
        {
            // Request authentication.
            return LoginInternal(returnUrl, false);
        }

        public ActionResult LoginCallback(string returnUrl)
        {
            // Verify authentication.
            return LoginInternal(returnUrl, true);
        }

        private ActionResult LoginInternal(string returnUrl, bool verifyResponse)
        {
            var processor = _openAuthenticationService.LoadExternalAuthenticationMethodBySystemName(TwitterExternalAuthMethod.SystemName, Services.StoreContext.CurrentStore.Id);
            if (processor == null || !processor.IsMethodActive(_externalAuthenticationSettings))
            {
                NotifyError(T("Plugins.CannotLoadModule", T("Plugins.FriendlyName.SmartStore.TwitterAuth")));
                return new RedirectResult(Url.LogOn(returnUrl));
            }

            var authorizer = Services.ResolveNamed<IExternalProviderAuthorizer>("twitter");
            var result = authorizer.Authorize(returnUrl, verifyResponse);

            switch (result.AuthenticationStatus)
            {
                case OpenAuthenticationStatus.Error:
                    result.Errors.Each(x => NotifyError(x));
                    return new RedirectResult(Url.LogOn(returnUrl));
                case OpenAuthenticationStatus.AssociateOnLogon:
                    return new RedirectResult(Url.LogOn(returnUrl));
                case OpenAuthenticationStatus.AutoRegisteredEmailValidation:
                    return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.EmailValidation, returnUrl });
                case OpenAuthenticationStatus.AutoRegisteredAdminApproval:
                    return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.AdminApproval, returnUrl });
                case OpenAuthenticationStatus.AutoRegisteredStandard:
                    return RedirectToRoute("RegisterResult", new { resultId = (int)UserRegistrationType.Standard, returnUrl });
                default:
                    if (result.Result != null)
                    {
                        return result.Result;
                    }

                    if (HttpContext.Request.IsAuthenticated)
                    {
                        return RedirectToReferrer(returnUrl, "~/");
                    }

                    return new RedirectResult(Url.LogOn(returnUrl));
            }
        }
    }
}