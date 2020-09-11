using SmartStore.Web.Framework.Modelling;

namespace SmartStore.Glimpse.Models
{
    public class ConfigurationModel : ModelBase
    {
        public bool IsEnabled { get; set; }
        public bool ShowConsoleInAdminArea { get; set; }
        public bool AllowAdministratorsOnly { get; set; }
        public bool EnableOnRemoteServer { get; set; }

        public bool ShowConfigurationTab { get; set; }
        public bool ShowEnvironmentTab { get; set; }
        public bool ShowRequestTab { get; set; }
        public bool ShowRoutesTab { get; set; }
        public bool ShowServerTab { get; set; }
        public bool ShowSessionTab { get; set; }
        public bool ShowExecutionTab { get; set; }
        public bool ShowMetadataTab { get; set; }
        public bool ShowModelBindingTab { get; set; }
        public bool ShowViewsTab { get; set; }
        public bool ShowTimelineTab { get; set; }
        public bool ShowTraceTab { get; set; }
        public bool ShowSqlTab { get; set; }
    }
}