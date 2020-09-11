using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using SmartStore.Web.Framework;

namespace SmartStore.USPS.Models
{
    public class USPSModel
    {
        public USPSModel()
        {
            CarrierServicesOfferedDomestic = new List<string>();
            AvailableCarrierServicesDomestic = new List<string>();
            CarrierServicesOfferedInternational = new List<string>();
            AvailableCarrierServicesInternational = new List<string>();
        }

        [SmartResourceDisplayName("Plugins.Shipping.USPS.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.USPS.Fields.Username")]
        public string Username { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.USPS.Fields.Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.USPS.Fields.AdditionalHandlingCharge")]
        public decimal AdditionalHandlingCharge { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.USPS.Fields.ZipPostalCodeFrom")]
        public string ZipPostalCodeFrom { get; set; }

        public IList<string> CarrierServicesOfferedDomestic { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.USPS.Fields.AvailableCarrierServicesDomestic")]
        public IList<string> AvailableCarrierServicesDomestic { get; set; }
        public string[] CheckedCarrierServicesDomestic { get; set; }

        public IList<string> CarrierServicesOfferedInternational { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.USPS.Fields.AvailableCarrierServicesInternational")]
        public IList<string> AvailableCarrierServicesInternational { get; set; }
        public string[] CheckedCarrierServicesInternational { get; set; }
        public string PrimaryStoreCurrencyCode { get; set; }
    }
}