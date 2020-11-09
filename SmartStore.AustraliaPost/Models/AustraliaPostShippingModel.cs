using System.ComponentModel;
using SmartStore.Web.Framework;

namespace SmartStore.AustraliaPost.Models
{
    public class AustraliaPostModel
    {
        [SmartResourceDisplayName("Plugins.Shipping.AustraliaPost.Fields.GatewayUrl")]
        public string GatewayUrl { get; set; }

        [DisplayName("Additional handling charge")]
        [SmartResourceDisplayName("Plugins.Shipping.AustraliaPost.Fields.AdditionalHandlingCharge")]
        public decimal AdditionalHandlingCharge { get; set; }

        [DisplayNameAttribute("Shipped from zip")]
        [SmartResourceDisplayName("Plugins.Shipping.AustraliaPost.Fields.ShippedFromZipPostalCode")]
        public string ShippedFromZipPostalCode { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
    }
}