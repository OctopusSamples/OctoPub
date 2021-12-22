using Audit.Service.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Audit.Service
{
    /// <summary>
    /// A host extension class that performs a database migration.
    /// </summary>
    public static class MigrationManager
    {
        /// <summary>
        /// Perform a database migration.
        /// </summary>
        /// <param name="host">Access to the hosting environment.</param>
        /// <returns>The same host for fluent chaining.</returns>
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using (var appContext = scope.ServiceProvider.GetRequiredService<Db>())
                {
                    if (appContext.Database.IsMySql())
                        appContext.Database.Migrate();
                    else if (appContext.Database.IsSqlite()) appContext.Database.EnsureCreated();
                }
            }

            return host;
        }
    }
}