using Autofac;
using Autofac.Core;
using SmartStore.Core.Data;
using SmartStore.Core.Infrastructure;
using SmartStore.Core.Infrastructure.DependencyManagement;
using SmartStore.Data;
using SmartStore.MailChimp.Data;
using SmartStore.MailChimp.Services;

namespace SmartStore.MailChimp
{
    public class DependencyRegistrar : IDependencyRegistrar
    {

        /// <summary>
        /// Registers the specified builder.
        /// </summary>
        /// <param name="builder">The builder.</param>
        /// <param name="typeFinder">The type finder.</param>
		public void Register(ContainerBuilder builder, ITypeFinder typeFinder, bool isActiveModule)
        {
            builder.RegisterType<SubscriptionEventQueueingService>().As<ISubscriptionEventQueueingService>().InstancePerRequest();
            builder.RegisterType<MailChimpInstallationService>().AsSelf().InstancePerRequest();
            builder.RegisterType<MailChimpApiService>().As<IMailChimpApiService>().InstancePerRequest();

            // Register named context
            builder.Register<IDbContext>(c => new MailChimpObjectContext(DataSettings.Current.DataConnectionString))
                .Named<IDbContext>(MailChimpObjectContext.ALIASKEY)
                .InstancePerRequest();

            // Register the type
            builder.Register<MailChimpObjectContext>(c => new MailChimpObjectContext(DataSettings.Current.DataConnectionString))
                .InstancePerRequest();

            //Register repository
            builder.RegisterType<EfRepository<MailChimpEventQueueRecord>>()
                .As<IRepository<MailChimpEventQueueRecord>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>(MailChimpObjectContext.ALIASKEY))
                .InstancePerRequest();
        }

        /// <summary>
        /// Gets the order.
        /// </summary>
        public int Order => 1;
    }
}