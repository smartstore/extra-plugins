using System.Data.Entity.Migrations;
using SmartStore.Core.Plugins;
using SmartStore.MailChimp.Data;
using SmartStore.MailChimp.Data.Migrations;
using SmartStore.Services.Localization;
using SmartStore.Services.Tasks;

namespace SmartStore.MailChimp.Services
{
    public class MailChimpInstallationService
    {
        private readonly MailChimpObjectContext _mailChimpObjectContext;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ILocalizationService _localizationService;

        public MailChimpInstallationService(MailChimpObjectContext mailChimpObjectContext,
            IScheduleTaskService scheduleTaskService,
            ILocalizationService localizationService)
        {
            _mailChimpObjectContext = mailChimpObjectContext;
            _scheduleTaskService = scheduleTaskService;
            _localizationService = localizationService;
        }

        /// <summary>
        /// Installs the sync task.
        /// </summary>
        private void InstallSyncTask()
        {
            _scheduleTaskService.GetOrAddTask<MailChimpSynchronizationTask>(x =>
            {
                x.Name = "MailChimp sync";
                x.CronExpression = "0 */1 * * *"; // Every hour
                x.Enabled = false;
            });

        }

        /// <summary>
        /// Installs this instance.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        public virtual void Install(BasePlugin plugin)
        {
            //locales
            _localizationService.ImportPluginResourcesFromXml(plugin.PluginDescriptor);

            //Install sync task
            InstallSyncTask();
        }

        /// <summary>
        /// Uninstalls this instance.
        /// </summary>
        /// <param name="plugin">The plugin.</param>
        public virtual void Uninstall(BasePlugin plugin)
        {
            //locales
            _localizationService.DeleteLocaleStringResources(plugin.PluginDescriptor.ResourceRootKey);

            _scheduleTaskService.TryDeleteTask<MailChimpSynchronizationTask>();

            var migrator = new DbMigrator(new Configuration());
            migrator.Update(DbMigrator.InitialDatabase);
        }
    }
}