using System.Collections.Generic;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Audit.Service.Migrations
{
    public class AddAuditsTable : DbMigration
    {
        public override void Up()
        {
            CreateTable("audit", c => new
            {
                Id = c.Int(identity: true, nullable: false,
                    annotations: new Dictionary<string, AnnotationValues>
                        {{ "MySql:ValueGenerationStrategy", new AnnotationValues(null, MySqlValueGenerationStrategy.IdentityColumn) }}),
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