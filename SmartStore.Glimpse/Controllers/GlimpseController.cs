using System.Web;
using System.Web.Mvc;
using Glimpse.Core.Framework;
using SmartStore.ComponentModel;
using SmartStore.Glimpse.Models;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;

namespace SmartStore.Glimpse.Controllers
{
    public class GlimpseController : PluginControllerBase
    {
        public ActionResult Index([Bind(Prefix = "n")] string resourceName)
        {
            var httpContext = this.HttpContext;

            // HACK: This is a hack for the POC. We will update Glimpse to allow for calling GlimpseRuntime.Instance instead.
            var runtime = httpContext.Application["__GlimpseRuntime"] as IGlimpseRuntime;
            if (runtime == null)
            {
                throw new HttpException(404, "Glimpse runtime is missing.");
            }

            if (resourceName.IsEmpty())
            {
                runtime.ExecuteDefaultResource();
            }
            else
            {
                runtime.ExecuteResource(resourceName, new ResourceParameters(httpContext.Request.QueryString.ToDictionary()));
            }

            return new EmptyResult();
        }

        [ChildActionOnly, AdminAuthorize, LoadSetting]
        public ActionResult Configure(GlimpseSettings settings)
        {
            var model = new ConfigurationModel();
            MiniMapper.Map(settings, model);

            return View(model);
        }

        [HttpPost, AdminAuthorize, ChildActionOnly, SaveSetting]
        public ActionResult Configure(ConfigurationModel model, GlimpseSettings settings)
        {
            if (!ModelState.IsValid)
            {
                return Configure(settings);
            }

            MiniMapper.Map(model, settings);
            NotifySuccess(T("Admin.Common.DataSuccessfullySaved"));

            return RedirectToConfiguration("SmartStore.Glimpse");
        }
    }
}