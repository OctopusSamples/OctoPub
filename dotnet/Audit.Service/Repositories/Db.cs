using Microsoft.EntityFrameworkCore;

namespace Audit.Service.Repositories
{
    /// <summary>
    /// The entity framework context.
    /// </summary>
    public class Db : DbContext
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Db"/> class.
        /// </summary>
        /// <param name="options">The database context options.</param>
        public Db(DbContextOptions<Db> options)
            : base(options)
        {
        }

        /// <summary>
        /// Gets the collection of audit entities.
        /// </summary>
        public DbSet<Models.Audit> Audits => Set<Models.Audit>();
    }
}