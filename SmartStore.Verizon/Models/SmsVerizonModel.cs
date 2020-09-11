using SmartStore.Web.Framework;

namespace SmartStore.Plugin.Sms.Verizon.Models
{
    public class SmsVerizonModel
    {
        [SmartResourceDisplayName("Plugins.Sms.Verizon.Enabled")]
        public bool Enabled { get; set; }

        [SmartResourceDisplayName("Plugins.Sms.Verizon.Email")]
        public string Email { get; set; }

        [SmartResourceDisplayName("Plugins.Sms.Verizon.TestMessage")]
        public string TestMessage { get; set; }
        public string TestSmsResult { get; set; }
        public bool ErrorResult { get; set; }
    }
}