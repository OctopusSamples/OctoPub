using System.Data.Entity.Migrations;

namespace Audit.Service.Migrations
{
    public class AddAuditsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable("audit", c => new
            {
                Id = c.Int(identity: true, nullable: false),
                Branch = c.String(),
                Tenant = c.String(),
                Action = c.String(),
                Subject = c.String(),
                Object = c.String()
            });
        }

        public override void Down()
        {
            DropTable("audit");
        }
    }
}