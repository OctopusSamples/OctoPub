﻿using Amazon.Lambda.APIGatewayEvents;
using Audit.Service.Repositories.InMemory;
using Audit.Service.Services;
using Audit.Service.Services.InMemory;
using Audit.Service.Services.Lambda;
using Microsoft.EntityFrameworkCore;
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
            var services = new ServiceCollection();

            // create an in memory database
            services.AddSingleton(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<Db>();
                optionsBuilder.UseInMemoryDatabase("audit");
                var context = new Db(optionsBuilder.Options);

                /*
                 * The in memory database lives as long as the Lambda is hot. But it will eventually be reset
                 * back to a blank state.
                 * To be able to test queries, we add a sample record so requests are not always empty.
                 */
                if (!_initializedDatabase)
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

            services.AddSingleton<IRequestWrapperAccessor, RequestWrapperAccessor>();
            services.AddSingleton<AuditCreateService>();
            services.AddSingleton<AuditGetAllService>();
            services.AddSingleton<AuditGetByIdService>();
            services.AddSingleton<AuditHandler>();

            return services.BuildServiceProvider();
        }
    }
}