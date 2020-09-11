using System;
using System.Web.Mvc;
using SmartStore.Core.Plugins;
using SmartStore.Plugin.Sms.Verizon.Models;
using SmartStore.Services.Configuration;
using SmartStore.Verizon;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Filters;
using SmartStore.Web.Framework.Security;

namespace SmartStore.Plugin.Sms.Verizon.Controllers
{
    [AdminAuthorize]
    public class SmsVerizonController : PluginControllerBase
    {
        private readonly VerizonSettings _verizonSettings;
        private readonly ISettingService _settingService;
        private readonly IPluginFinder _pluginFinder;

        public SmsVerizonController(
            VerizonSettings verizonSettings,
            ISettingService settingService,
            IPluginFinder pluginFinder)
        {
            _verizonSettings = verizonSettings;
            _settingService = settingService;
            _pluginFinder = pluginFinder;
        }

        public ActionResult Configure()
        {
            var model = new SmsVerizonModel
            {
                Enabled = _verizonSettings.Enabled,
                Email = _verizonSettings.Email
            };

            return View(model);
        }

        [HttpPost, FormValueRequired("save")]
        public ActionResult Configure(SmsVerizonModel model)
        {
            if (!ModelState.IsValid)
            {
                return Configure();
            }

            _verizonSettings.Enabled = model.Enabled;
            _verizonSettings.Email = model.Email;

            _settingService.SaveSetting(_verizonSettings);

            return View(model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("test-sms")]
        public ActionResult TestSms(SmsVerizonModel model)
        {
            try
            {
                if (model.TestMessage.IsEmpty())
                {
                    model.TestSmsResult = T("Plugins.Sms.Verizon.EnterTestMessage");
                    model.ErrorResult = true;
                }
                else
                {
                    var pluginDescriptor = _pluginFinder.GetPluginDescriptorBySystemName("SmartStore.Verizon");
                    if (pluginDescriptor == null)
                        throw new SmartException(T("Admin.Common.ResourceNotFound"));

                    var plugin = pluginDescriptor.Instance() as VerizonSmsProvider;
                    if (plugin == null)
                        throw new SmartException(T("Admin.Common.ResourceNotFound"));

                    if (plugin.SendSms(model.TestMessage))
                    {
                        model.TestSmsResult = T("Plugins.Sms.Verizon.TestSuccess");
                    }
                    else
                    {
                        model.TestSmsResult = T("Plugins.Sms.Verizon.TestFailed");
                        model.ErrorResult = true;
                    }
                }
            }
            catch (Exception exception)
            {
                model.TestSmsResult = exception.ToString();
                model.ErrorResult = true;
            }

            return View("Configure", model);
        }
    }
}