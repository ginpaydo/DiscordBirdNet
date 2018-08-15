namespace DiscordBirdNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddFacilityMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Facility",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 2147483647),
                        BasePrice = c.Int(nullable: false),
                        BaseIncome = c.Int(nullable: false),
                        Level = c.Int(nullable: false),
                        CurrentPrice = c.Int(nullable: false),
                        CurrentIncome = c.Int(nullable: false),
                        Comment = c.String(maxLength: 2147483647),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Facility");
        }
    }
}
