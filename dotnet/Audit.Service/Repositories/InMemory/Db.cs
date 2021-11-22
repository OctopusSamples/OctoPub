using System.Data.Entity.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Audit.Service.Repositories.InMemory
{
    public class Db : DbContext
    {
        public Db(DbContextOptions<Db> options)
            : base(options)
        {
            Database.SetCommandTimeout(180);
        }

        public DbSet<Models.Audit> Audits { get; set; }
    }
}