using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using audit_service.Services;
using audit_service.Services.InMemory;
using audit_service.Services.Lambda;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace audit_service.Lambda
{
    public class Audits
    {
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<APIGatewayProxyResponse> AuditsApi(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var serviceProvider = ConfigureServices(request);

                var auditGetAllService = serviceProvider.GetService<AuditGetAllService>();
                var token = new CancellationTokenSource().Token;

                return new APIGatewayProxyResponse
                {
                    Body = JsonSerializer.Serialize(await auditGetAllService.GetAsync(token)),
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new APIGatewayProxyResponse
                {
                    Body = JsonSerializer.Serialize(ex.ToString()),
                    StatusCode = 500
                };
            }
        }

        private ServiceProvider ConfigureServices(APIGatewayProxyRequest request)
        {
            var services = new ServiceCollection();

            // create an in memory database
            services.AddSingleton(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<Db>();
                optionsBuilder.UseInMemoryDatabase("audit");
                var context = new Db(optionsBuilder.Options);

                context.Audits.Add(new Audit
                {
                    Id = 0, Action = "Created a sample audit record for the ephemeral inmemory database",
                    Object = "Test", Subject = "Test", Tenant = Constants.DefaultTenant
                });
                context.SaveChanges();

                return context;
            });

            services.AddSingleton<IApiGatewayProxyRequestAccessor>(provider =>
                new ApiGatewayProxyRequestAccessor(request));
            services.AddSingleton<ITenantExtractor, TenantExtractor>();
            services.AddSingleton<ITenantParser, LambdaTenantParser>();
            services.AddSingleton<AuditCreateService>();
            services.AddSingleton<AuditGetAllService>();
            services.AddSingleton<AuditGetByIdService>();

            return services.BuildServiceProvider();
        }
    }
}