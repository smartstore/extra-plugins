using System.Web.Mvc;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;

namespace SmartStore.LivePersonChat.Models
{
    public class ConfigurationModel : ModelBase
    {
        [SmartResourceDisplayName("Plugins.Widgets.LivePersonChat.MonitoringCode")]
        [AllowHtml]
        public string MonitoringCode { get; set; }
    }
}