using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using SmartStore.Web.Framework;

namespace SmartStore.Fedex.Models
{
    public class FedexShippingModel
    {
        public FedexShippingModel()
        {
            CarrierServicesOffered = new List<string>();
            AvailableCarrierServices = new List<string>();
        }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.Url")]
        public string Url { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.Key")]
        public string Key { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.AccountNumber")]
        public string AccountNumber { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.MeterNumber")]
        public string MeterNumber { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.UseResidentialRates")]
        public bool UseResidentialRates { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.ApplyDiscounts")]
        public bool ApplyDiscounts { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.AdditionalHandlingCharge")]
        public decimal AdditionalHandlingCharge { get; set; }

        public IList<string> CarrierServicesOffered { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.CarrierServices")]
        public IList<string> AvailableCarrierServices { get; set; }
        public string[] CheckedCarrierServices { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.Street")]
        public string Street { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.City")]
        public string City { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.StateOrProvinceCode")]
        public string StateOrProvinceCode { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.PostalCode")]
        public string PostalCode { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.CountryCode")]
        public string CountryCode { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.PassDimensions")]
        public bool PassDimensions { get; set; }

        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.PackingPackageVolume")]
        public int PackingPackageVolume { get; set; }

        public int PackingType { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.PackingType")]
        public SelectList PackingTypeValues { get; set; }

        public int DropoffType { get; set; }
        [SmartResourceDisplayName("Plugins.Shipping.Fedex.Fields.DropoffType")]
        public SelectList AvailableDropOffTypes { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
    }
}