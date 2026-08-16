namespace DevExtremeMvcApp1.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CalculationResults",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ShapeType = c.String(nullable: false),
                        Param1 = c.Double(nullable: false),
                        Area = c.Double(nullable: false),
                        Volume = c.Double(nullable: false),
                        CalculationDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CalculationResults");
        }
    }
}
