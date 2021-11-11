using audit_service.Models;
using Microsoft.EntityFrameworkCore;

namespace audit_service.Repositories.InMemory
{
    public class Db : DbContext
    {
        public Db(DbContextOptions<Db> options)
            : base(options)
        {
        }

        public DbSet<Audit> Audits { get; set; }
    }
}