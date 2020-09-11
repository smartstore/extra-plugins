using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Net;
using System.Text;
using System.Web.Routing;
using SmartStore.AuthorizeNet.Controllers;
using SmartStore.AuthorizeNet.net.authorize.api;
using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Domain.Payments;
using SmartStore.Core.Plugins;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Services.Payments;

namespace SmartStore.AuthorizeNet
{
    /// <summary>
    /// AuthorizeNet payment processor
    /// </summary>
    [DisplayOrder(77)]
    public class Plugin : PaymentPluginBase, IConfigurable
    {
        #region Fields

        private readonly AuthorizeNetSettings _authorizeNetPaymentSettings;
        private readonly ICustomerService _customerService;
        private readonly ICommonServices _services;

        #endregion

        #region Ctor

        public Plugin(AuthorizeNetSettings authorizeNetPaymentSettings,
            ICustomerService customerService,
            ICommonServices services)
        {
            _authorizeNetPaymentSettings = authorizeNetPaymentSettings;
            _customerService = customerService;
            _services = services;
        }

        #endregion

        #region Utilities


        /// <summary>
        /// Gets Authorize.NET URL
        /// </summary>
        /// <returns></returns>
        private string GetAuthorizeNETUrl()
        {
            return _authorizeNetPaymentSettings.UseSandbox ? "https://test.authorize.net/gateway/transact.dll" :
                "https://secure.authorize.net/gateway/transact.dll";
        }

        /// <summary>
        /// Gets Authorize.NET API version
        /// </summary>
        private string GetApiVersion()
        {
            return "3.1";
        }

        // Populate merchant authentication (ARB Support)
        private MerchantAuthenticationType PopulateMerchantAuthentication()
        {
            var authentication = new MerchantAuthenticationType();
            authentication.name = _authorizeNetPaymentSettings.LoginId;
            authentication.transactionKey = _authorizeNetPaymentSettings.TransactionKey;
            return authentication;
        }
        /// <summary>
        ///  Get errors (ARB Support)
        /// </summary>
        /// <param name="response"></param>
        private static string GetErrors(ANetApiResponseType response)
        {
            var sb = new StringBuilder();
            sb.AppendLine("The API request failed with the following errors:");
            for (int i = 0; i < response.messages.Length; i++)
            {
                sb.AppendLine("[" + response.messages[i].code + "] " + response.messages[i].text);
            }
            return sb.ToString();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public override ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);
            var store = _services.StoreService.GetStoreById(processPaymentRequest.StoreId);

            var webClient = new WebClient();
            var form = new NameValueCollection();
            form.Add("x_login", _authorizeNetPaymentSettings.LoginId);
            form.Add("x_tran_key", _authorizeNetPaymentSettings.TransactionKey);
            if (_authorizeNetPaymentSettings.UseSandbox)
                form.Add("x_test_request", "TRUE");
            else
                form.Add("x_test_request", "FALSE");

            form.Add("x_delim_data", "TRUE");
            form.Add("x_delim_char", "|");
            form.Add("x_encap_char", "");
            form.Add("x_version", GetApiVersion());
            form.Add("x_relay_response", "FALSE");
            form.Add("x_method", "CC");
            form.Add("x_currency_code", store.PrimaryStoreCurrency.CurrencyCode);
            if (_authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize)
                form.Add("x_type", "AUTH_ONLY");
            else if (_authorizeNetPaymentSettings.TransactMode == TransactMode.AuthorizeAndCapture)
                form.Add("x_type", "AUTH_CAPTURE");
            else
                throw new SmartException("Not supported transaction mode");

            var orderTotal = Math.Round(processPaymentRequest.OrderTotal, 2);
            form.Add("x_amount", orderTotal.ToString("0.00", CultureInfo.InvariantCulture));
            form.Add("x_card_num", processPaymentRequest.CreditCardNumber);
            form.Add("x_exp_date", processPaymentRequest.CreditCardExpireMonth.ToString("D2") + processPaymentRequest.CreditCardExpireYear.ToString());
            form.Add("x_card_code", processPaymentRequest.CreditCardCvv2);
            form.Add("x_first_name", customer.BillingAddress.FirstName);
            form.Add("x_last_name", customer.BillingAddress.LastName);
            if (!string.IsNullOrEmpty(customer.BillingAddress.Company))
                form.Add("x_company", customer.BillingAddress.Company);
            form.Add("x_address", customer.BillingAddress.Address1);
            form.Add("x_city", customer.BillingAddress.City);
            if (customer.BillingAddress.StateProvince != null)
                form.Add("x_state", customer.BillingAddress.StateProvince.Abbreviation);
            form.Add("x_zip", customer.BillingAddress.ZipPostalCode);
            if (customer.BillingAddress.Country != null)
                form.Add("x_country", customer.BillingAddress.Country.TwoLetterIsoCode);
            //20 chars maximum
            form.Add("x_invoice_num", processPaymentRequest.OrderGuid.ToString().Substring(0, 20));
            form.Add("x_customer_ip", _services.WebHelper.GetCurrentIpAddress());

            string reply = null;

            Byte[] responseData = webClient.UploadValues(GetAuthorizeNETUrl(), form);
            reply = Encoding.ASCII.GetString(responseData);

            if (!String.IsNullOrEmpty(reply))
            {
                string[] responseFields = reply.Split('|');
                switch (responseFields[0])
                {
                    case "1":
                        result.AuthorizationTransactionCode = string.Format("{0},{1}", responseFields[6], responseFields[4]);
                        result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", responseFields[2], responseFields[3]);
                        result.AvsResult = responseFields[5];
                        //responseFields[38];
                        if (_authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize)
                        {
                            result.NewPaymentStatus = PaymentStatus.Authorized;
                        }
                        else
                        {
                            result.NewPaymentStatus = PaymentStatus.Paid;
                        }
                        break;
                    case "2":
                        result.AddError(string.Format("Declined ({0}: {1})", responseFields[2], responseFields[3]));
                        break;
                    case "3":
                        result.AddError(string.Format("Error: {0}", reply));
                        break;

                }
            }
            else
            {
                result.AddError("Authorize.NET unknown error");
            }

            return result;
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shoping cart</param>
        /// <returns>Additional handling fee</returns>
		public override decimal GetAdditionalHandlingFee(IList<OrganizedShoppingCartItem> cart)
        {
            return _authorizeNetPaymentSettings.AdditionalFee;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public override CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult
            {
                NewPaymentStatus = capturePaymentRequest.Order.PaymentStatus
            };

            var webClient = new WebClient();
            var form = new NameValueCollection();
            var store = _services.StoreService.GetStoreById(capturePaymentRequest.Order.StoreId);

            form.Add("x_login", _authorizeNetPaymentSettings.LoginId);
            form.Add("x_tran_key", _authorizeNetPaymentSettings.TransactionKey);
            if (_authorizeNetPaymentSettings.UseSandbox)
                form.Add("x_test_request", "TRUE");
            else
                form.Add("x_test_request", "FALSE");

            form.Add("x_delim_data", "TRUE");
            form.Add("x_delim_char", "|");
            form.Add("x_encap_char", "");
            form.Add("x_version", GetApiVersion());
            form.Add("x_relay_response", "FALSE");
            form.Add("x_method", "CC");
            form.Add("x_currency_code", store.PrimaryStoreCurrency.CurrencyCode);
            form.Add("x_type", "PRIOR_AUTH_CAPTURE");

            var orderTotal = Math.Round(capturePaymentRequest.Order.OrderTotal, 2);
            form.Add("x_amount", orderTotal.ToString("0.00", CultureInfo.InvariantCulture));
            string[] codes = capturePaymentRequest.Order.AuthorizationTransactionCode.Split(',');
            //x_trans_id. When x_test_request (sandbox) is set to a positive response, 
            //or when Test mode is enabled on the payment gateway, this value will be "0".
            form.Add("x_trans_id", codes[0]);

            string reply = null;

            Byte[] responseData = webClient.UploadValues(GetAuthorizeNETUrl(), form);
            reply = Encoding.ASCII.GetString(responseData);

            if (!String.IsNullOrEmpty(reply))
            {
                string[] responseFields = reply.Split('|');
                switch (responseFields[0])
                {
                    case "1":
                        result.CaptureTransactionId = string.Format("{0},{1}", responseFields[6], responseFields[4]);
                        result.CaptureTransactionResult = string.Format("Approved ({0}: {1})", responseFields[2], responseFields[3]);
                        //result.AVSResult = responseFields[5];
                        //responseFields[38];
                        result.NewPaymentStatus = PaymentStatus.Paid;
                        break;
                    case "2":
                        result.AddError(string.Format("Declined ({0}: {1})", responseFields[2], responseFields[3]));
                        break;
                    case "3":
                        result.AddError(string.Format("Error: {0}", reply));
                        break;
                }
            }
            else
            {
                result.AddError("Authorize.NET unknown error");
            }

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public override ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();

            var authentication = PopulateMerchantAuthentication();
            if (!processPaymentRequest.IsRecurringPayment)
            {
                var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

                var subscription = new ARBSubscriptionType();
                var creditCard = new net.authorize.api.CreditCardType();

                subscription.name = processPaymentRequest.OrderGuid.ToString();

                creditCard.cardNumber = processPaymentRequest.CreditCardNumber;
                creditCard.expirationDate = processPaymentRequest.CreditCardExpireYear + "-" + processPaymentRequest.CreditCardExpireMonth; // required format for API is YYYY-MM
                creditCard.cardCode = processPaymentRequest.CreditCardCvv2;

                subscription.payment = new PaymentType();
                subscription.payment.Item = creditCard;

                subscription.billTo = new NameAndAddressType();
                subscription.billTo.firstName = customer.BillingAddress.FirstName;
                subscription.billTo.lastName = customer.BillingAddress.LastName;
                subscription.billTo.address = customer.BillingAddress.Address1 + " " + customer.BillingAddress.Address2;
                subscription.billTo.city = customer.BillingAddress.City;
                if (customer.BillingAddress.StateProvince != null)
                {
                    subscription.billTo.state = customer.BillingAddress.StateProvince.Abbreviation;
                }
                subscription.billTo.zip = customer.BillingAddress.ZipPostalCode;

                if (customer.ShippingAddress != null)
                {
                    subscription.shipTo = new NameAndAddressType();
                    subscription.shipTo.firstName = customer.ShippingAddress.FirstName;
                    subscription.shipTo.lastName = customer.ShippingAddress.LastName;
                    subscription.shipTo.address = customer.ShippingAddress.Address1 + " " + customer.ShippingAddress.Address2;
                    subscription.shipTo.city = customer.ShippingAddress.City;
                    if (customer.ShippingAddress.StateProvince != null)
                    {
                        subscription.shipTo.state = customer.ShippingAddress.StateProvince.Abbreviation;
                    }
                    subscription.shipTo.zip = customer.ShippingAddress.ZipPostalCode;

                }

                subscription.customer = new CustomerType();
                subscription.customer.email = customer.BillingAddress.Email;
                subscription.customer.phoneNumber = customer.BillingAddress.PhoneNumber;

                subscription.order = new OrderType();
                subscription.order.description = string.Format("{0} {1}", "Authorize.NET", "Recurring payment");

                // Create a subscription that is leng of specified occurrences and interval is amount of days ad runs

                subscription.paymentSchedule = new PaymentScheduleType();
                DateTime dtNow = DateTime.UtcNow;
                subscription.paymentSchedule.startDate = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day);
                subscription.paymentSchedule.startDateSpecified = true;

                subscription.paymentSchedule.totalOccurrences = Convert.ToInt16(processPaymentRequest.RecurringTotalCycles);
                subscription.paymentSchedule.totalOccurrencesSpecified = true;

                var orderTotal = Math.Round(processPaymentRequest.OrderTotal, 2);
                subscription.amount = orderTotal;
                subscription.amountSpecified = true;

                // Interval can't be updated once a subscription is created.
                subscription.paymentSchedule.interval = new PaymentScheduleTypeInterval();
                switch (processPaymentRequest.RecurringCyclePeriod)
                {
                    case RecurringProductCyclePeriod.Days:
                        subscription.paymentSchedule.interval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength);
                        subscription.paymentSchedule.interval.unit = ARBSubscriptionUnitEnum.days;
                        break;
                    case RecurringProductCyclePeriod.Weeks:
                        subscription.paymentSchedule.interval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength * 7);
                        subscription.paymentSchedule.interval.unit = ARBSubscriptionUnitEnum.days;
                        break;
                    case RecurringProductCyclePeriod.Months:
                        subscription.paymentSchedule.interval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength);
                        subscription.paymentSchedule.interval.unit = ARBSubscriptionUnitEnum.months;
                        break;
                    case RecurringProductCyclePeriod.Years:
                        subscription.paymentSchedule.interval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength * 12);
                        subscription.paymentSchedule.interval.unit = ARBSubscriptionUnitEnum.months;
                        break;
                    default:
                        throw new SmartException("Not supported cycle period");
                }

                using (var webService = new net.authorize.api.Service())
                {
                    if (_authorizeNetPaymentSettings.UseSandbox)
                        webService.Url = "https://apitest.authorize.net/soap/v1/Service.asmx";
                    else
                        webService.Url = "https://api.authorize.net/soap/v1/Service.asmx";

                    var response = webService.ARBCreateSubscription(authentication, subscription);

                    if (response.resultCode == MessageTypeEnum.Ok)
                    {
                        result.SubscriptionTransactionId = response.subscriptionId.ToString();
                        result.AuthorizationTransactionCode = response.resultCode.ToString();
                        result.AuthorizationTransactionResult = string.Format("Approved ({0}: {1})", response.resultCode.ToString(), response.subscriptionId.ToString());

                        if (_authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize)
                        {
                            result.NewPaymentStatus = PaymentStatus.Authorized;
                        }
                        else
                        {
                            result.NewPaymentStatus = PaymentStatus.Paid;
                        }
                    }
                    else
                    {
                        result.AddError(string.Format("Error processing recurring payment. {0}", GetErrors(response)));
                    }
                }
            }

            return result;
        }

        public override RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Manual;

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public override CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            var authentication = PopulateMerchantAuthentication();
            long subscriptionId = 0;
            long.TryParse(cancelPaymentRequest.Order.SubscriptionTransactionId, out subscriptionId);


            using (var webService = new net.authorize.api.Service())
            {
                if (_authorizeNetPaymentSettings.UseSandbox)
                    webService.Url = "https://apitest.authorize.net/soap/v1/Service.asmx";
                else
                    webService.Url = "https://api.authorize.net/soap/v1/Service.asmx";

                var response = webService.ARBCancelSubscription(authentication, subscriptionId);

                if (response.resultCode == MessageTypeEnum.Ok)
                {
                    //ok
                }
                else
                {
                    result.AddError("Error cancelling subscription, please contact customer support. " + GetErrors(response));
                }
            }
            return result;
        }

        /// <summary>
        /// Gets a route for provider configuration
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetConfigurationRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "Configure";
            controllerName = "AuthorizeNet";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.AuthorizeNet" } };
        }

        /// <summary>
        /// Gets a route for payment info
        /// </summary>
        /// <param name="actionName">Action name</param>
        /// <param name="controllerName">Controller name</param>
        /// <param name="routeValues">Route values</param>
        public override void GetPaymentInfoRoute(out string actionName, out string controllerName, out RouteValueDictionary routeValues)
        {
            actionName = "PaymentInfo";
            controllerName = "AuthorizeNet";
            routeValues = new RouteValueDictionary() { { "area", "SmartStore.AuthorizeNet" } };
        }

        public override Type GetControllerType()
        {
            return typeof(AuthorizeNetController);
        }

        public override void Install()
        {
            //settings
            var settings = new AuthorizeNetSettings()
            {
                UseSandbox = true,
                TransactMode = TransactMode.Authorize,
                TransactionKey = "123",
                LoginId = "456"
            };
            _services.Settings.SaveSetting(settings);

            _services.Localization.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _services.Settings.DeleteSetting<AuthorizeNetSettings>();

            _services.Localization.ImportPluginResourcesFromXml(this.PluginDescriptor);

            base.Uninstall();
        }

        #endregion

        #region Properties


        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public override PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        #endregion
    }
}
