namespace SmartStore.MailChimp.Data.Migrations
{
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            //if (DbMigrationContext.Current.SuppressInitialCreate<MailChimpObjectContext>())
            //	return;

            CreateTable(
                "dbo.MailChimpEventQueueRecord",
                c => new
                {
                    Id = c.Int(nullable: false, identity: true),
                    Email = c.String(nullable: false, maxLength: 255),
                    IsSubscribe = c.Boolean(nullable: false),
                    CreatedOnUtc = c.DateTime(nullable: false),
                })
                .PrimaryKey(t => t.Id);

        }

        public override void Down()
        {
            DropTable("dbo.MailChimpEventQueueRecord");
        }
    }
}
