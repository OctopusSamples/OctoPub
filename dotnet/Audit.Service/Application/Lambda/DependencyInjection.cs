using System;
using System.IO;
using System.Reflection;
using Audit.Service.Domain.Handler;
using Audit.Service.Infrastructure.Repositories;
using Audit.Service.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Audit.Service.Application.Lambda
{
    /// <summary>
    ///     Manages the dependency injection context.
    /// </summary>
    public class DependencyInjection
    {
        /// <summary>
        ///     Builds a dependency injection context.
        /// </summary>
        /// <returns>The DI service provider.</returns>
        public ServiceProvider ConfigureServices()
        {
            var configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddJsonFile(
                    $"appsettings.{Environment.GetEnvironmentVariable("LAMBDA_ENVIRONMENT")}.json",
                    true)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();

            var useInMemoryDb =
                bool.Parse(configuration.GetSection("Database:UseInMemory").Value);

            // create an in memory database
            services.AddSingleton(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<Db>();
                if (useInMemoryDb)
                {
                    var dbPath = $"{Path.GetTempPath()}{Path.DirectorySeparatorChar}audits.db";
                    optionsBuilder.UseSqlite(
                        $"Data Source={dbPath}",
                        x => x.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name));
                }
                else
                {
                    optionsBuilder.UseMySql(
                        configuration.GetConnectionString("MySqlDatabase"),
                        new MySqlServerVersion(Constants.MySqlVersion),
                        x =>
                        {
                            x.EnableRetryOnFailure(3, TimeSpan.FromSeconds(10), null);
                            x.MigrationsAssembly(Assembly.GetExecutingAssembly().GetName().Name);
                        });
                }

                var context = new Db(optionsBuilder.Options);

                InitializeDatabase(context, configuration);

                return context;
            });

            services.AddSingleton<IResponseBuilder, ResponseBuilder>();
            services.AddSingleton<AuditCreateService>();
            services.AddSingleton<AuditGetAllService>();
            services.AddSingleton<AuditGetByIdService>();
            services.AddSingleton<AuditHandler>();

            return services.BuildServiceProvider();
        }

        private void InitializeDatabase(Db context, IConfigurationRoot configuration)
        {
            if (context.Database.IsSqlite())
                context.Database.EnsureCreated();
        }
    }
}