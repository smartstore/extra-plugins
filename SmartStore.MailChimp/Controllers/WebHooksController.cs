using System;
using System.Web;
using System.Web.Mvc;
using SmartStore.Services.Messages;

namespace SmartStore.MailChimp.Controllers
{
    public class WebHooksController : Controller
    {
        private readonly MailChimpSettings _settings;
        private readonly HttpContextBase _httpContext;
        private readonly INewsLetterSubscriptionService _newsLetterSubscriptionService;

		private const string EMAIL_KEY_NAME = "data[email]";
		private const string TYPE_KEY_NAME = "type";
		private const string TYPE_VALUE = "unsubscribe";

        public WebHooksController(
            MailChimpSettings settings,
            HttpContextBase httpContext,
            INewsLetterSubscriptionService newsLetterSubscriptionService)
        {
            _settings = settings;
            _httpContext = httpContext;
            _newsLetterSubscriptionService = newsLetterSubscriptionService;
        }

        public ActionResult Index(string webHookKey)
        {
            if (string.IsNullOrWhiteSpace(_settings.WebHookKey))
            {
                return Content("Invalid Request.");
            }

            if (!string.Equals(_settings.WebHookKey, webHookKey, StringComparison.InvariantCultureIgnoreCase))
            {
                return Content("Invalid Request.");
            }

            if (IsUnsubscribe())
            {
				var email = FindEmail();
				if (email.IsEmail())
				{
                    // TODO: multistore capable.
                    var subscriptions = _newsLetterSubscriptionService.GetAllNewsLetterSubscribers(email, 0, int.MaxValue, true);

                    foreach (var subscription in subscriptions)
					{
						// Do not publish unsubscribe event. Or duplicate events will occur.
						_newsLetterSubscriptionService.DeleteNewsLetterSubscription(subscription.Subscription, false);
					}

                    if (subscriptions.Count > 0)
                    {
                        return Content("OK");
                    }
				}
            }

            return Content("Invalid Request.");
        }

        /// <summary>
        /// Finds the email.
        /// </summary>
        /// <returns></returns>
        private string FindEmail()
        {
			return _httpContext.Request.Form[EMAIL_KEY_NAME];
        }

        /// <summary>
        /// Determines whether this instance is unsubscribe.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is unsubscribe; otherwise, <c>false</c>.
        /// </returns>
        private bool IsUnsubscribe()
        {
			return string.Equals(_httpContext.Request.Form[TYPE_KEY_NAME], TYPE_VALUE, StringComparison.InvariantCultureIgnoreCase);
        }
    }
}