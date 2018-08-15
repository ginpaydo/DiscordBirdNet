namespace DiscordBirdNet.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddParameterMigration : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Parameters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 2147483647),
                        Value = c.String(nullable: false, maxLength: 2147483647),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.Parameters");
        }
    }
}
