using System;
using System.Text;
using System.Web.Mvc;
using SmartStore.Fedex.Domain;
using SmartStore.Fedex.Models;
using SmartStore.Services.Configuration;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Fedex.Controllers
{
    [AdminAuthorize]
    public class FedexController : PluginControllerBase
    {
        private readonly FedexSettings _fedexSettings;
        private readonly ISettingService _settingService;

        public FedexController(FedexSettings fedexSettings, ISettingService settingService)
        {
            this._fedexSettings = fedexSettings;
            this._settingService = settingService;
        }

        public ActionResult Configure()
        {
            var model = new FedexShippingModel();
            model.Url = _fedexSettings.Url;
            model.Key = _fedexSettings.Key;
            model.Password = _fedexSettings.Password;
            model.AccountNumber = _fedexSettings.AccountNumber;
            model.MeterNumber = _fedexSettings.MeterNumber;
            model.DropoffType = Convert.ToInt32(_fedexSettings.DropoffType);
            model.AvailableDropOffTypes = _fedexSettings.DropoffType.ToSelectList();
            model.UseResidentialRates = _fedexSettings.UseResidentialRates;
            model.ApplyDiscounts = _fedexSettings.ApplyDiscounts;
            model.AdditionalHandlingCharge = _fedexSettings.AdditionalHandlingCharge;
            model.Street = _fedexSettings.Street;
            model.City = _fedexSettings.City;
            model.StateOrProvinceCode = _fedexSettings.StateOrProvinceCode;
            model.PostalCode = _fedexSettings.PostalCode;
            model.CountryCode = _fedexSettings.CountryCode;
            model.PackingPackageVolume = _fedexSettings.PackingPackageVolume;
            model.PackingType = Convert.ToInt32(_fedexSettings.PackingType);
            model.PackingTypeValues = _fedexSettings.PackingType.ToSelectList();
            model.PassDimensions = _fedexSettings.PassDimensions;
            model.PrimaryStoreCurrencyCode = Services.StoreContext.CurrentStore.PrimaryStoreCurrency.CurrencyCode;

            // Load service names.
            var services = new FedexServices();
            var carrierServicesOfferedDomestic = _fedexSettings.CarrierServicesOffered;
            foreach (string service in services.Services)
            {
                model.AvailableCarrierServices.Add(service);
            }

            if (!String.IsNullOrEmpty(carrierServicesOfferedDomestic))
            {
                foreach (string service in services.Services)
                {
                    string serviceId = FedexServices.GetServiceId(service);
                    if (!String.IsNullOrEmpty(serviceId) && !String.IsNullOrEmpty(carrierServicesOfferedDomestic))
                    {
                        if (carrierServicesOfferedDomestic.Contains(serviceId))
                            model.CarrierServicesOffered.Add(service);
                    }
                }
            }

            return View(model);
        }

        [HttpPost]
        public ActionResult Configure(FedexShippingModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            _fedexSettings.Url = model.Url;
            _fedexSettings.Key = model.Key.TrimSafe();
            _fedexSettings.Password = model.Password.TrimSafe();
            _fedexSettings.AccountNumber = model.AccountNumber.TrimSafe();
            _fedexSettings.MeterNumber = model.MeterNumber;
            _fedexSettings.DropoffType = (DropoffType)model.DropoffType;
            _fedexSettings.UseResidentialRates = model.UseResidentialRates;
            _fedexSettings.ApplyDiscounts = model.ApplyDiscounts;
            _fedexSettings.AdditionalHandlingCharge = model.AdditionalHandlingCharge;
            _fedexSettings.Street = model.Street;
            _fedexSettings.City = model.City;
            _fedexSettings.StateOrProvinceCode = model.StateOrProvinceCode.Truncate(2);
            _fedexSettings.PostalCode = model.PostalCode;
            _fedexSettings.CountryCode = model.CountryCode;
            _fedexSettings.PackingPackageVolume = model.PackingPackageVolume;
            _fedexSettings.PackingType = (PackingType)model.PackingType;
            _fedexSettings.PassDimensions = model.PassDimensions;

            // Save selected services
            var carrierServicesOfferedDomestic = new StringBuilder();
            var carrierServicesDomesticSelectedCount = 0;
            if (model.CheckedCarrierServices != null)
            {
                foreach (var cs in model.CheckedCarrierServices)
                {
                    carrierServicesDomesticSelectedCount++;
                    var serviceId = FedexServices.GetServiceId(cs);
                    if (!String.IsNullOrEmpty(serviceId))
                    {
                        carrierServicesOfferedDomestic.AppendFormat("{0}:", serviceId);
                    }
                }
            }

            // Add default options if no services were selected (Priority Mail International, First-Class Mail International Package, and Express Mail International)
            if (carrierServicesDomesticSelectedCount == 0)
            {
                _fedexSettings.CarrierServicesOffered = "FEDEX_2_DAY:PRIORITY_OVERNIGHT:FEDEX_GROUND:GROUND_HOME_DELIVERY:INTERNATIONAL_ECONOMY";
            }
            else
            {
                _fedexSettings.CarrierServicesOffered = carrierServicesOfferedDomestic.ToString();
            }

            _settingService.SaveSetting(_fedexSettings);

            return RedirectToConfiguration("SmartStore.FedEx", false);
        }
    }
}
