using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
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
        private static ServiceProvider ServiceProvider { get; set; }

        public Audits()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();
        }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public APIGatewayProxyResponse AuditsApi(APIGatewayProxyRequest request, ILambdaContext context)
        {
            return new APIGatewayProxyResponse
            {
                Body = "OK",
                StatusCode = 200
            };
        }

        private static void ConfigureServices(IServiceCollection services)
        {
            // create an in memory database
            services.AddTransient(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<Db>();
                optionsBuilder.UseInMemoryDatabase("audit");
                return new Db(optionsBuilder.Options);
            });

            services.AddTransient<ITenantParser, TenantParser>();
            services.AddTransient<ILambdaTenantParser, LambdaTenantParser>();
            services.AddTransient<AuditCreateService>();
            services.AddTransient<AuditGetAllService>();
            services.AddTransient<AuditGetByIdService>();
        }
    }
}