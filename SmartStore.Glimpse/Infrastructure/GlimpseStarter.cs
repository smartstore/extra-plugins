using System.Diagnostics;
//using Glimpse.Ado;
//using Glimpse.EF;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using SmartStore.Core.Infrastructure;
using G = Glimpse;

namespace SmartStore.Glimpse.Infrastructure
{
	public class GlimpseStarter : IPreApplicationStart, IApplicationStart
    {
        // Ensure this runs before EF initializes
        int IApplicationStart.Order => -2000;

        void IPreApplicationStart.Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(GlimpseHttpModule));

            //G.Ado.Initialize.Start();
            //G.EF.Initialize.Start();
        }

        void IApplicationStart.Start()
        {
            // register Glimpse trace listener
            var listener = new G.Core.TraceListener();
            Trace.Listeners.Add(listener);
        }
    }
}
