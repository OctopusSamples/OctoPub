using Microsoft.EntityFrameworkCore;

namespace Audit.Service.Repositories
{
    public class Db : DbContext
    {
        public Db(DbContextOptions<Db> options)
            : base(options)
        {
        }

        public DbSet<Models.Audit> Audits => Set<Models.Audit>();
    }
}