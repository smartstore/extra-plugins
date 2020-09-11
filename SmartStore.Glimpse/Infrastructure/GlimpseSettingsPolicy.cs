using Glimpse.AspNet.Extensions;
using Glimpse.Core.Extensibility;
using SmartStore.Core;
using SmartStore.Core.Infrastructure;
using SmartStore.Services.Customers;

namespace SmartStore.Glimpse.Infrastructure
{
    public class GlimpseSettingsPolicy : IRuntimePolicy
    {
        public RuntimePolicy Execute(IRuntimePolicyContext policyContext)
        {
            // You can perform a check like the one below to control Glimpse's permissions within your application.
            // More information about RuntimePolicies can be found at http://getglimpse.com/Help/Custom-Runtime-Policy
            // var httpContext = policyContext.GetHttpContext();
            // if (!httpContext.User.IsInRole("Administrator"))
            // {
            //     return RuntimePolicy.Off;
            // }

            var settings = EngineContext.Current.Resolve<GlimpseSettings>();

            if (!settings.IsEnabled)
                return RuntimePolicy.Off;

            var workContext = EngineContext.Current.Resolve<IWorkContext>();

            if (!settings.ShowConsoleInAdminArea)
            {
                if (workContext.IsAdmin)
                    return RuntimePolicy.Off;
            }

            if (settings.AllowAdministratorsOnly)
            {
                if (!workContext.CurrentCustomer.IsAdmin())
                {
                    return RuntimePolicy.Off;
                }
            }

            if (!settings.EnableOnRemoteServer)
            {
                var httpContext = policyContext.GetHttpContext();
                if (!httpContext.Request.IsLocal)
                {
                    return RuntimePolicy.Off;
                }
            }

            return RuntimePolicy.On;
        }

        public RuntimeEvent ExecuteOn => RuntimeEvent.EndRequest;
    }
}