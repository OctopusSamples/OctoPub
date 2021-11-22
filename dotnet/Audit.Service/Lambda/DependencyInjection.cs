using System;
using System.Configuration;
using System.Reflection;
using Audit.Service.Repositories;
using Audit.Service.Services.InMemory;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Service.Lambda
{
    /// <summary>
    /// Manages the dependency injection context.
    /// </summary>
    public class DependencyInjection
    {
        private static bool _initializedDatabase;

        /// <summary>
        /// Builds a dependency injection context.
        /// </summary>
        /// <returns>The DI service provider</returns>
        public ServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("LAMBDA_ENVIRONMENT")}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            var useInMemoryDb =
                Boolean.Parse((ReadOnlySpan<char>)configuration.GetSection("Database:UseInMemory").Value);

            // create an in memory database
            services.AddSingleton(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<Db>();
                if (useInMemoryDb)
                {
                    optionsBuilder.UseInMemoryDatabase("audit");
                }
                else
                {
                    optionsBuilder.UseMySql(
                        configuration.GetConnectionString("MySqlDatabase"),
                        new MySqlServerVersion("8.0"),
                        x => x.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));
                }
                var context = new Db(optionsBuilder.Options);

                if (useInMemoryDb)
                {
                    context.Database.EnsureCreated();
                }
                else
                {
                    context.Database.SetCommandTimeout(Int32.TryParse(configuration.GetSection("Database:MySqlTimeout").Value, out var timeout) ? timeout : 180 );
                    context.Database.GetMigrations();
                    context.Database.Migrate();
                }

                /*
                 * The in memory database lives as long as the Lambda is hot. But it will eventually be reset
                 * back to a blank state.
                 * To be able to test queries, we add a sample record so requests are not always empty.
                 */
                if (useInMemoryDb && !_initializedDatabase)
                {
                    context.Audits.Add(new Models.Audit
                    {
                        Id = 0, Action = "Created a sample audit record for the ephemeral inmemory database",
                        Object = "Test", Subject = "Test", Tenant = Constants.DefaultTenant
                    });
                    context.SaveChanges();
                    _initializedDatabase = true;
                }

                return context;
            });

            services.AddSingleton<AuditCreateService>();
            services.AddSingleton<AuditGetAllService>();
            services.AddSingleton<AuditGetByIdService>();
            services.AddSingleton<AuditHandler>();

            return services.BuildServiceProvider();
        }
    }
}