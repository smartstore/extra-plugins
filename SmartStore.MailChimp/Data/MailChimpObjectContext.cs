using System.Data.Entity;
using SmartStore.Data;
using SmartStore.Data.Setup;
using SmartStore.MailChimp.Data.Migrations;

namespace SmartStore.MailChimp.Data
{
    public class MailChimpObjectContext : ObjectContextBase
    {
        internal const string ALIASKEY = "SmartStore.MailChimp-ObjectContext";

        static MailChimpObjectContext()
        {
            var initializer = new MigrateDatabaseInitializer<MailChimpObjectContext, Configuration>
            {
                TablesToCheck = new[] { "MailChimpEventQueueRecord" }
            };
            Database.SetInitializer(initializer);
        }

        /// <summary>
        /// For tooling support, e.g. EF Migrations
        /// </summary>
        public MailChimpObjectContext()
            : base()
        {
        }

        public MailChimpObjectContext(string nameOrConnectionString)
            : base(nameOrConnectionString, ALIASKEY)
        {
        }

        /// <summary>
        /// This method is called when the model for a derived context has been initialized, but
        /// before the model has been locked down and used to initialize the context.  The default
        /// implementation of this method does nothing, but it can be overridden in a derived class
        /// such that the model can be further configured before it is locked down.
        /// </summary>
        /// <param name="modelBuilder">The builder that defines the model for the context being created.</param>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new MailChimpEventQueueRecordMap());

            //disable EdmMetadata generation
            //modelBuilder.Conventions.Remove<IncludeMetadataConvention>();
            base.OnModelCreating(modelBuilder);
        }

    }
}