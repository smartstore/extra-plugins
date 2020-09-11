using System;
using System.Collections.Specialized;
using System.Text;
using System.Web.Mvc;
using SmartStore.Core.Domain.Tasks;
using SmartStore.MailChimp.Models;
using SmartStore.MailChimp.Services;
using SmartStore.Services.Configuration;
using SmartStore.Services.Localization;
using SmartStore.Services.Tasks;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;

namespace SmartStore.MailChimp.Controllers
{

    [AdminAuthorize]
    public class MailChimpController : PluginControllerBase
    {
        private readonly IMailChimpApiService _mailChimpApiService;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly MailChimpSettings _settings;
        private readonly ISubscriptionEventQueueingService _subscriptionEventQueueingService;

        public MailChimpController(ISettingService settingService, IScheduleTaskService scheduleTaskService,
            IMailChimpApiService mailChimpApiService, ISubscriptionEventQueueingService subscriptionEventQueueingService,
            ILocalizationService localizationService, MailChimpSettings settings)
        {
            this._settingService = settingService;
            this._scheduleTaskService = scheduleTaskService;
            this._mailChimpApiService = mailChimpApiService;
            this._subscriptionEventQueueingService = subscriptionEventQueueingService;
            this._localizationService = localizationService;
            this._settings = settings;
        }

        [NonAction]
        private void MapListOptions(MailChimpSettingsModel model)
        {
            NameValueCollection listOptions = _mailChimpApiService.RetrieveLists();

            //Ensure there will not be duplicates
            model.ListOptions.Clear();

            foreach (string key in listOptions.AllKeys)
            {
                model.ListOptions.Add(new SelectListItem { Text = key, Value = listOptions[key] });
            }
        }

        [NonAction]
        private MailChimpSettingsModel PrepareModel()
        {
            var model = new MailChimpSettingsModel();

            //Set the properties
            model.ApiKey = _settings.ApiKey;
            model.DefaultListId = _settings.DefaultListId;
            model.WebHookKey = _settings.WebHookKey;
            ScheduleTask task = FindScheduledTask();
            if (task != null)
            {
                //model.AutoSyncEachMinutes = task.Seconds / 60;
                model.AutoSync = task.Enabled;
            }

            //Maps the list options
            MapListOptions(model);

            return model;
        }

        [NonAction]
        private ScheduleTask FindScheduledTask()
        {
            return _scheduleTaskService.GetTaskByType("SmartStore.MailChimp.MailChimpSynchronizationTask, SmartStore.MailChimp");
        }

        public ActionResult Configure()
        {
            var model = PrepareModel();
            return View(model);
        }

        [HttpPost, FormValueRequired("save"), ActionName("Configure")]
        public ActionResult ConfigurePost(MailChimpSettingsModel model)
        {
            string saveResult = "";
            if (ModelState.IsValid)
            {
                _settings.DefaultListId = model.DefaultListId;
                _settings.ApiKey = model.ApiKey.TrimSafe();
                _settings.WebHookKey = model.WebHookKey.TrimSafe();

                _settingService.SaveSetting(_settings);
            }

            // Update the task
            var task = FindScheduledTask();
            if (task != null)
            {
                task.Enabled = model.AutoSync;
                //task.Seconds = model.AutoSyncEachMinutes*60;
                _scheduleTaskService.UpdateTask(task);
                saveResult = _localizationService.GetResource("Plugins.Misc.MailChimp.AutoSyncRestart");
            }

            model = PrepareModel();
            //set result text
            model.SaveResult = saveResult;

            return RedirectToConfiguration("SmartStore.MailChimp");
        }

        [HttpPost, FormValueRequired("queueall"), ActionName("Configure")]
        public ActionResult QueueAll(FormCollection formCollection)
        {
            _subscriptionEventQueueingService.QueueAll();

            return Configure();
        }

        [HttpPost, FormValueRequired("sync"), ActionName("Configure")]
        public ActionResult Sync(FormCollection formCollection)
        {
            var model = PrepareModel();
            try
            {
                var sb = new StringBuilder();

                var result = _mailChimpApiService.Synchronize();
                //subscribe
                sb.Append("Subscribe results: ");
                sb.Append(result.SubscribeResult);
                sb.Append("<br />");
                for (int i = 0; i < result.SubscribeErrors.Count; i++)
                {
                    sb.Append(result.SubscribeErrors[i]);
                    if (i != result.SubscribeErrors.Count - 1)
                        sb.Append("<br />");
                }

                //unsubscribe
                sb.Append("<br />");
                sb.Append("Unsubscribe results: ");
                sb.Append(result.UnsubscribeResult);
                sb.Append("<br />");
                for (int i = 0; i < result.UnsubscribeErrors.Count; i++)
                {
                    sb.Append(result.UnsubscribeErrors[i]);
                    if (i != result.UnsubscribeErrors.Count - 1)
                        sb.Append("<br />");
                }
                //set result text
                model.SyncResult = sb.ToString();
            }
            catch (Exception exc)
            {
                //set result text
                model.SyncResult = exc.ToString();
            }

            return View("Configure", model);
        }
    }
}