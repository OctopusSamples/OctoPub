using Microsoft.EntityFrameworkCore;

namespace Audit.Service.Repositories.InMemory
{
    public class Db : DbContext
    {
        public Db(DbContextOptions<Db> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Models.Audit> Audits { get; set; }
    }
}