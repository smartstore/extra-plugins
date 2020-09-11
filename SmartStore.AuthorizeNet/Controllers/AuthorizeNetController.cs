using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using SmartStore.AuthorizeNet.Models;
using SmartStore.Services.Payments;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.AuthorizeNet.Controllers
{
    public class AuthorizeNetController : PaymentControllerBase
    {
        private readonly AuthorizeNetSettings _authorizeNetPaymentSettings;
        private readonly HttpContextBase _httpContext;

        public AuthorizeNetController(
            AuthorizeNetSettings authorizeNetPaymentSettings,
            HttpContextBase httpContext)
        {
            _authorizeNetPaymentSettings = authorizeNetPaymentSettings;
            _httpContext = httpContext;
        }

        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure()
        {
            var model = new ConfigurationModel();
            model.UseSandbox = _authorizeNetPaymentSettings.UseSandbox;
            model.TransactModeId = Convert.ToInt32(_authorizeNetPaymentSettings.TransactMode);
            model.TransactionKey = _authorizeNetPaymentSettings.TransactionKey;
            model.LoginId = _authorizeNetPaymentSettings.LoginId;
            model.AdditionalFee = _authorizeNetPaymentSettings.AdditionalFee;
            model.TransactModeValues = _authorizeNetPaymentSettings.TransactMode.ToSelectList();
            model.PrimaryStoreCurrencyCode = Services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            return View(model);
        }

        [HttpPost]
        [AdminAuthorize]
        [ChildActionOnly]
        public ActionResult Configure(ConfigurationModel model)
        {
            if (!ModelState.IsValid)
                return Configure();

            _authorizeNetPaymentSettings.UseSandbox = model.UseSandbox;
            _authorizeNetPaymentSettings.TransactMode = (TransactMode)model.TransactModeId;
            _authorizeNetPaymentSettings.TransactionKey = model.TransactionKey.TrimSafe();
            _authorizeNetPaymentSettings.LoginId = model.LoginId.TrimSafe();
            _authorizeNetPaymentSettings.AdditionalFee = model.AdditionalFee;

            Services.Settings.SaveSetting(_authorizeNetPaymentSettings);

            model.TransactModeValues = _authorizeNetPaymentSettings.TransactMode.ToSelectList();

            return RedirectToConfiguration("SmartStore.AuthorizeNet", false);
        }

        public ActionResult PaymentInfo()
        {
            var model = new PaymentInfoModel();

            // CC types.
            model.CreditCardTypes.Add(new SelectListItem { Text = "Visa", Value = "Visa" });
            model.CreditCardTypes.Add(new SelectListItem { Text = "Master card", Value = "MasterCard" });
            model.CreditCardTypes.Add(new SelectListItem { Text = "Discover", Value = "Discover" });
            model.CreditCardTypes.Add(new SelectListItem { Text = "Amex", Value = "Amex" });

            // Years.
            for (int i = 0; i < 15; i++)
            {
                string year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem
                {
                    Text = year,
                    Value = year,
                });
            }

            // Months.
            for (int i = 1; i <= 12; i++)
            {
                string text = (i < 10) ? "0" + i.ToString() : i.ToString();
                model.ExpireMonths.Add(new SelectListItem
                {
                    Text = text,
                    Value = i.ToString(),
                });
            }

            // Set postback values.
            var paymentData = _httpContext.GetCheckoutState().PaymentData;
            model.CardholderName = (string)paymentData.Get("CardholderName");
            model.CardNumber = (string)paymentData.Get("CardNumber");
            model.CardCode = (string)paymentData.Get("CardCode");

            var creditCardType = (string)paymentData.Get("CreditCardType");
            var selectedCcType = model.CreditCardTypes.Where(x => x.Value.Equals(creditCardType, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedCcType != null)
                selectedCcType.Selected = true;

            var expireMonth = (string)paymentData.Get("ExpireMonth");
            var selectedMonth = model.ExpireMonths.Where(x => x.Value.Equals(expireMonth, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedMonth != null)
                selectedMonth.Selected = true;

            var expireYear = (string)paymentData.Get("ExpireYear");
            var selectedYear = model.ExpireYears.Where(x => x.Value.Equals(expireYear, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
            if (selectedYear != null)
                selectedYear.Selected = true;

            return PartialView(model);
        }

        [NonAction]
        public override IList<string> ValidatePaymentForm(FormCollection form)
        {
            var warnings = new List<string>();
            var validator = new PaymentInfoValidator(T);
            var model = new PaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };

            var validationResult = validator.Validate(model);
            if (!validationResult.IsValid)
            {
                foreach (var error in validationResult.Errors)
                {
                    warnings.Add(error.ErrorMessage);
                }
            }

            return warnings;
        }

        [NonAction]
        public override ProcessPaymentRequest GetPaymentInfo(FormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest();
            paymentInfo.CreditCardType = form["CreditCardType"];
            paymentInfo.CreditCardName = form["CardholderName"];
            paymentInfo.CreditCardNumber = form["CardNumber"];
            paymentInfo.CreditCardExpireMonth = int.Parse(form["ExpireMonth"]);
            paymentInfo.CreditCardExpireYear = int.Parse(form["ExpireYear"]);
            paymentInfo.CreditCardCvv2 = form["CardCode"];
            return paymentInfo;
        }

        [NonAction]
        public override string GetPaymentSummary(FormCollection form)
        {
            var number = form["CardNumber"];
            return "{0}, {1}, {2}".FormatCurrent(
                form["CreditCardType"],
                form["CardholderName"],
                number.Mask(4)
            );
        }
    }
}