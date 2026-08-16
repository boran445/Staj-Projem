namespace DevExtremeMvcApp1.Migrations
{
    using System.Data.Entity.Migrations;

    internal sealed class Configuration : DbMigrationsConfiguration<DevExtremeMvcApp1.Data.ApplicationDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            AutomaticMigrationDataLossAllowed = false;
        }

        protected override void Seed(DevExtremeMvcApp1.Data.ApplicationDbContext context)
        {
        }
    }
}
