using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Web;
using Glimpse.AspNet;
using Glimpse.AspNet.Policy;
using Glimpse.Core.Extensibility;
using Glimpse.Core.Framework;
using Glimpse.Mvc.Inspector;
using SmartStore.Core.Infrastructure;
using G = Glimpse;

namespace SmartStore.Glimpse.Infrastructure
{

    public class GlimpseHttpModule : IHttpModule
    {
        private const string RuntimeKey = "__GlimpseRuntime";
        private const string LoggerKey = "__GlimpseLogger";
        private static readonly object _lock = new object();
        private static readonly Factory Factory;

        static GlimpseHttpModule()
        {
            var providerLocator = new AspNetServiceLocator();
            var userLocator = new GlimpseServiceLocator();
            Factory = new Factory(providerLocator/*, userLocator >> CRASHES */);

            AppDomain.CurrentDomain.SetData(LoggerKey, Factory.InstantiateLogger());
            AppDomain.CurrentDomain.DomainUnload += OnAppDomainUnload;
        }

        public void Init(HttpApplication httpApplication)
        {
            Init(WithTestable(httpApplication));
        }

        public void Dispose()
        {
            // Nothing to dispose
        }

        internal void Init(HttpApplicationBase httpApplication)
        {
            var runtime = GetRuntime(httpApplication.Application);

            if (runtime.IsInitialized || runtime.Initialize())
            {
                httpApplication.BeginRequest += (context, e) => BeginRequest(WithTestable(context));
                httpApplication.PostAcquireRequestState += (context, e) => BeginSessionAccess(WithTestable(context));
                httpApplication.PostRequestHandlerExecute += (context, e) => EndSessionAccess(WithTestable(context));
                httpApplication.PostReleaseRequestState += (context, e) => EndRequest(WithTestable(context));
                httpApplication.PreSendRequestHeaders += (context, e) => SendHeaders(WithTestable(context));
            }
        }

        internal static void OnAppDomainUnload(object sender, EventArgs e)
        {
            var appDomain = sender as AppDomain;
            var logger = appDomain.GetData(LoggerKey) as ILogger;

            if (logger == null)
                return;

            string shutDownMessage = "Reason for shutdown: ";
            var httpRuntimeType = typeof(HttpRuntime);

            // Get shutdown message from HttpRuntime via ScottGu: http://weblogs.asp.net/scottgu/archive/2005/12/14/433194.aspx
            var httpRuntime = httpRuntimeType.InvokeMember("_theRuntime", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField, null, null, null) as HttpRuntime;
            if (httpRuntime != null)
            {
                shutDownMessage += httpRuntimeType.InvokeMember("_shutDownMessage", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField, null, httpRuntime, null) as string;
            }
            else
            {
                shutDownMessage += "unknown.";
            }

            logger.Fatal("App domain with Id: '{0}' and BaseDirectory: '{1}' has been unloaded. Any in memory data stores have been lost.{2}", appDomain.Id, appDomain.BaseDirectory, shutDownMessage);

            // NLog writes its logs asynchronously, which means that if we don't wait, chances are the log will not be written 
            // before the appdomain is actually shut down, so we sleep for 100ms and hopefully that is enough for NLog to do its thing
            Thread.Sleep(100);
        }

        internal IGlimpseRuntime GetRuntime(HttpApplicationStateBase applicationState)
        {
            var runtime = applicationState[RuntimeKey] as IGlimpseRuntime;

            if (runtime == null)
            {
                lock (_lock)
                {
                    runtime = applicationState[RuntimeKey] as IGlimpseRuntime;

                    if (runtime == null)
                    {
                        var config = Factory.InstantiateConfiguration() as GlimpseConfiguration;
                        config.EndpointBaseUri = "~/Glimpse";
                        config.DefaultRuntimePolicy = RuntimePolicy.On;

                        IRuntimePolicy policy;
                        var policies = config.RuntimePolicies;
                        if (policies != null)
                        {
                            policy = policies.Where(x => x is LocalPolicy).FirstOrDefault();
                            if (policy != null)
                            {
                                policies.Remove(policy);
                            }
                        }

                        var crashingInspector = config.Inspectors.Where(x => x is DependencyInjectionInspector).FirstOrDefault();
                        if (crashingInspector != null)
                        {
                            // get rid of this shit! (https://github.com/Glimpse/Glimpse/issues/513)
                            config.Inspectors.Remove(crashingInspector);
                        }

                        var settings = EngineContext.Current.Resolve<GlimpseSettings>();

                        var tabs = config.Tabs;
                        if (tabs != null)
                        {
                            if (!settings.ShowConfigurationTab)
                                RemoveTabFromCollection<G.AspNet.Tab.Configuration>(tabs);

                            if (!settings.ShowSessionTab)
                                RemoveTabFromCollection<G.AspNet.Tab.Session>(tabs);

                            if (!settings.ShowServerTab)
                                RemoveTabFromCollection<G.AspNet.Tab.Server>(tabs);

                            if (!settings.ShowEnvironmentTab)
                                RemoveTabFromCollection<G.AspNet.Tab.Environment>(tabs);

                            if (!settings.ShowRequestTab)
                                RemoveTabFromCollection<G.AspNet.Tab.Request>(tabs);

                            if (!settings.ShowRoutesTab)
                                RemoveTabFromCollection<G.AspNet.Tab.Routes>(tabs);

                            if (!settings.ShowTimelineTab)
                                RemoveTabFromCollection<G.Core.Tab.Timeline>(tabs);

                            if (!settings.ShowTraceTab)
                                RemoveTabFromCollection<G.Core.Tab.Trace>(tabs);

                            if (!settings.ShowExecutionTab)
                                RemoveTabFromCollection<G.Mvc.Tab.Execution>(tabs);

                            if (!settings.ShowMetadataTab)
                                RemoveTabFromCollection<G.Mvc.Tab.Metadata>(tabs);

                            if (!settings.ShowModelBindingTab)
                                RemoveTabFromCollection<G.Mvc.Tab.ModelBinding>(tabs);

                            if (!settings.ShowViewsTab)
                                RemoveTabFromCollection<G.Mvc.Tab.Views>(tabs);

                            //if (!settings.ShowSqlTab)
                            //	RemoveTabFromCollection<G.Ado.Tab.SQL>(tabs);

                        }

                        runtime = Factory.InstantiateRuntime(); // new GlimpseRuntime(config);
                        ((GlimpseRuntime)runtime).Configuration = config;

                        applicationState.Add(RuntimeKey, runtime);
                    }
                }
            }

            return runtime;
        }

        private void RemoveTabFromCollection<T>(ICollection<ITab> collection) where T : ITab
        {
            var type = typeof(T);
            var item = collection.Where(x => type.IsInstanceOfType(x)).FirstOrDefault();
            if (item != null)
            {
                collection.Remove(item);
            }
        }

        internal void BeginRequest(HttpContextBase httpContext)
        {
            // TODO: Add Logging to either methods here or in Runtime
            var runtime = GetRuntime(httpContext.Application);

            runtime.BeginRequest();
        }

        internal void EndRequest(HttpContextBase httpContext)
        {
            var runtime = GetRuntime(httpContext.Application);

            runtime.EndRequest();
        }

        internal void SendHeaders(HttpContextBase httpContext)
        {
            httpContext.Items["_GlimpseHttpHeadersSent"] = true;
        }

        private static HttpContextBase WithTestable(object sender)
        {
            var httpApplication = sender as HttpApplication;

            return new HttpContextWrapper(httpApplication.Context);
        }

        private static HttpApplicationBase WithTestable(HttpApplication httpApplication)
        {
            return new HttpApplicationWrapper(httpApplication);
        }

        private void BeginSessionAccess(HttpContextBase httpContext)
        {
            var runtime = GetRuntime(httpContext.Application);

            runtime.BeginSessionAccess();
        }

        private void EndSessionAccess(HttpContextBase httpContext)
        {
            var runtime = GetRuntime(httpContext.Application);

            runtime.EndSessionAccess();
        }
    }

}
