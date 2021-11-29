using Audit.Service.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Audit.Service
{
    public static class MigrationManager
    {
        public static IHost MigrateDatabase(this IHost host)
        {
            using (var scope = host.Services.CreateScope())
            {
                using (var appContext = scope.ServiceProvider.GetRequiredService<Db>())
                {
                    if (appContext.Database.IsMySql())
                    {
                        appContext.Database.Migrate();
                    }
                    else if (appContext.Database.IsSqlite())
                    {
                        appContext.Database.EnsureCreated();
                    }
                }
            }

            return host;
        }
    }
}