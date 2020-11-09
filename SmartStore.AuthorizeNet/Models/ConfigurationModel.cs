using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.AuthorizeNet.Models
{
    public class ConfigurationModel : ModelBase
    {
        [SmartResourceDisplayName("Plugins.Payments.AuthorizeNet.Fields.UseSandbox")]
        public bool UseSandbox { get; set; }

        public int TransactModeId { get; set; }
        [SmartResourceDisplayName("Plugins.Payments.AuthorizeNet.Fields.TransactModeValues")]
        public SelectList TransactModeValues { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.AuthorizeNet.Fields.TransactionKey")]
        public string TransactionKey { get; set; }

        [SmartResourceDisplayName("Plugins.Payments.AuthorizeNet.Fields.LoginId")]
        public string LoginId { get; set; }

        [SmartResourceDisplayName("Admin.Configuration.Payment.Methods.AdditionalFee")]
        public decimal AdditionalFee { get; set; }

        public string PrimaryStoreCurrencyCode { get; set; }
    }
}