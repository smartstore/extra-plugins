using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.UPS.Models
{
    public class UPSModel
    {
        public UPSModel()
        {
            CarrierServicesOffered = new List<string>();
            AvailableCarrierServices = new List<string>();
            AvailableCustomerClassifications = new List<SelectListItem>();
            AvailablePickupTypes = new List<SelectListItem>();
            AvailablePackagingTypes = new List<SelectListItem>();
            AvailableCountries = new List<SelectListItem>();
        }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.Url")]
        public string Url { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.AccessKey")]
        public string AccessKey { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.Username")]
        public string Username { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.AdditionalHandlingCharge")]
        public decimal AdditionalHandlingCharge { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.InsurePackage")]
        public bool InsurePackage { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.CustomerClassification")]
        public string CustomerClassification { get; set; }
        public IList<SelectListItem> AvailableCustomerClassifications { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.PickupType")]
        public string PickupType { get; set; }
        public IList<SelectListItem> AvailablePickupTypes { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.PackagingType")]
        public string PackagingType { get; set; }
        public IList<SelectListItem> AvailablePackagingTypes { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.DefaultShippedFromCountry")]
        public int DefaultShippedFromCountryId { get; set; }
        public IList<SelectListItem> AvailableCountries { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.DefaultShippedFromZipPostalCode")]
        public string DefaultShippedFromZipPostalCode { get; set; }


        public IList<string> CarrierServicesOffered { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.UPS.Fields.AvailableCarrierServices")]
        public IList<string> AvailableCarrierServices { get; set; }
        public string[] CheckedCarrierServices { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }
    }
}