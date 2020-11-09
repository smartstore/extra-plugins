using SmartStore.Core;
using SmartStore.Core.Plugins;
using SmartStore.MailChimp.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Tasks;

namespace SmartStore.MailChimp
{
    public class MailChimpSynchronizationTask : ITask
    {
        private readonly IPluginFinder _pluginFinder;
        private readonly IMailChimpApiService _mailChimpApiService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        public MailChimpSynchronizationTask(IPluginFinder pluginFinder,
            IMailChimpApiService mailChimpApiService,
            IStoreContext storeContext,
            ISettingService settingService)
        {
            this._pluginFinder = pluginFinder;
            this._mailChimpApiService = mailChimpApiService;
            this._storeContext = storeContext;
            this._settingService = settingService;
        }

        /// <summary>
        /// Execute task
        /// </summary>
		public void Execute(TaskExecutionContext ctx)
        {
            //is plugin installed?
            var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.MailChimp");
            if (pluginDescriptor == null)
                return;

            if (!(_storeContext.CurrentStore.Id == 0 ||
                _settingService.GetSettingByKey<string>(pluginDescriptor.GetSettingKey("LimitedToStores")).ToIntArrayContains(_storeContext.CurrentStore.Id, true)))
                return;

            //is plugin configured?
            var plugin = pluginDescriptor.Instance() as MailChimpPlugin;
            if (plugin == null || !plugin.IsConfigured())
                return;

            _mailChimpApiService.Synchronize();
        }
    }
}