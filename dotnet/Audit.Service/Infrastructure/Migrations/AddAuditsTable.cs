using Audit.Service.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Audit.Service.Infrastructure.Migrations
{
    /// <summary>
    /// A database migration that creates the initial Audits table.
    /// </summary>
    [DbContext(typeof(Db))]
    [Migration("00001-AddInitialTable")]
    public class AddAuditsTable : Migration
    {
        /// <summary>
        /// Create the audits table.
        /// </summary>
        /// <param name="migrationBuilder">The migration builder interface.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                "audits",
                table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Branch = table.Column<string>(nullable: true),
                    DataPartition = table.Column<string>(nullable: true),
                    Action = table.Column<string>(nullable: true),
                    Subject = table.Column<string>(nullable: true),
                    Object = table.Column<string>(nullable: true)
                },
                constraints: table => { table.PrimaryKey("PK_Authors", x => x.Id); });
        }
    }
}