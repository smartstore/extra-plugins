using System;
using System.Text;
using System.Web.Mvc;
using SmartStore.Services.Configuration;
using SmartStore.USPS.Domain;
using SmartStore.USPS.Models;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.USPS.Controllers
{
    [AdminAuthorize]
    public class USPSController : PluginControllerBase
    {
        private readonly USPSSettings _uspsSettings;
        private readonly ISettingService _settingService;

        public USPSController(USPSSettings uspsSettings, ISettingService settingService)
        {
            _uspsSettings = uspsSettings;
            _settingService = settingService;
        }

        public ActionResult Configure()
        {
            var model = new USPSModel();
            model.UseSandbox = _uspsSettings.UseSandbox;
            model.Username = _uspsSettings.Username;
            model.Password = _uspsSettings.Password;
            model.AdditionalHandlingCharge = _uspsSettings.AdditionalHandlingCharge;
            model.ZipPostalCodeFrom = _uspsSettings.ZipPostalCodeFrom;
            model.PrimaryStoreCurrencyCode = Services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            // Load Domestic service names.
            var services = new USPSServices();
            var carrierServicesOfferedDomestic = _uspsSettings.CarrierServicesOfferedDomestic;
            foreach (string service in services.DomesticServices)
            {
                model.AvailableCarrierServicesDomestic.Add(service);
            }

            if (!String.IsNullOrEmpty(carrierServicesOfferedDomestic))
            {
                foreach (string service in services.DomesticServices)
                {
                    var serviceId = USPSServices.GetServiceIdDomestic(service);
                    if (!String.IsNullOrEmpty(serviceId) && !String.IsNullOrEmpty(carrierServicesOfferedDomestic))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                        if (carrierServicesOfferedDomestic.Contains(String.Format("[{0}]", serviceId)))
                        {
                            model.CarrierServicesOfferedDomestic.Add(service);
                        }
                    }
                }
            }

            // Load Internation service names.
            var carrierServicesOfferedInternational = _uspsSettings.CarrierServicesOfferedInternational;
            foreach (string service in services.InternationalServices)
            {
                model.AvailableCarrierServicesInternational.Add(service);
            }

            if (!String.IsNullOrEmpty(carrierServicesOfferedInternational))
            {
                foreach (string service in services.InternationalServices)
                {
                    var serviceId = USPSServices.GetServiceIdInternational(service);
                    if (!String.IsNullOrEmpty(serviceId) && !String.IsNullOrEmpty(carrierServicesOfferedInternational))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs.
                        if (carrierServicesOfferedInternational.Contains(String.Format("[{0}]", serviceId)))
                        {
                            model.CarrierServicesOfferedInternational.Add(service);
                        }
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Configure(USPSModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            _uspsSettings.UseSandbox = model.UseSandbox;
            _uspsSettings.Username = model.Username.TrimSafe();
            _uspsSettings.Password = model.Password.TrimSafe();
            _uspsSettings.AdditionalHandlingCharge = model.AdditionalHandlingCharge;
            _uspsSettings.ZipPostalCodeFrom = model.ZipPostalCodeFrom;

            // Save selected Domestic services.
            var carrierServicesOfferedDomestic = new StringBuilder();
            int carrierServicesDomesticSelectedCount = 0;
            if (model.CheckedCarrierServicesDomestic != null)
            {
                foreach (var cs in model.CheckedCarrierServicesDomestic)
                {
                    carrierServicesDomesticSelectedCount++;

                    string serviceId = USPSServices.GetServiceIdDomestic(cs);
                    //unselect any other services if NONE is selected
                    if (serviceId.Equals("NONE"))
                    {
                        carrierServicesOfferedDomestic.Clear();
                        carrierServicesOfferedDomestic.AppendFormat("[{0}]:", serviceId);
                        break;
                    }

                    if (!String.IsNullOrEmpty(serviceId))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                        carrierServicesOfferedDomestic.AppendFormat("[{0}]:", serviceId);
                    }
                }
            }
            // Add default options if no services were selected (Priority, Express, and Parcel Post)
            if (carrierServicesDomesticSelectedCount == 0)
            {
                _uspsSettings.CarrierServicesOfferedDomestic = "[1]:[3]:[4]:";
            }
            else
            {
                _uspsSettings.CarrierServicesOfferedDomestic = carrierServicesOfferedDomestic.ToString();
            }

            // Save selected International services
            var carrierServicesOfferedInternational = new StringBuilder();
            int carrierServicesInternationalSelectedCount = 0;
            if (model.CheckedCarrierServicesInternational != null)
            {
                foreach (var cs in model.CheckedCarrierServicesInternational)
                {
                    carrierServicesInternationalSelectedCount++;
                    string serviceId = USPSServices.GetServiceIdInternational(cs);
                    // unselect other services if NONE is selected
                    if (serviceId.Equals("NONE"))
                    {
                        carrierServicesOfferedInternational.Clear();
                        carrierServicesOfferedInternational.AppendFormat("[{0}]:", serviceId);
                        break;
                    }
                    if (!String.IsNullOrEmpty(serviceId))
                    {
                        // Add delimiters [] so that single digit IDs aren't found in multi-digit IDs
                        carrierServicesOfferedInternational.AppendFormat("[{0}]:", serviceId);
                    }
                }
            }
            // Add default options if no services were selected (Priority Mail International, First-Class Mail International Package, and Express Mail International)
            if (carrierServicesInternationalSelectedCount == 0)
            {
                _uspsSettings.CarrierServicesOfferedInternational = "[2]:[15]:[1]:";
            }
            else
            {
                _uspsSettings.CarrierServicesOfferedInternational = carrierServicesOfferedInternational.ToString();
            }

            _settingService.SaveSetting(_uspsSettings);

            return RedirectToConfiguration("SmartStore.USPS");
        }
    }
}
