using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
        private ServiceProvider ServiceProvider { get; set; }

        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<APIGatewayProxyResponse> AuditsApi(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                ConfigureServices(request);

                var auditGetAllService = ServiceProvider.GetService<AuditGetAllService>();
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

        private void ConfigureServices(APIGatewayProxyRequest request)
        {
            var services = new ServiceCollection();

            // create an in memory database
            services.AddTransient(provider =>
            {
                var optionsBuilder = new DbContextOptionsBuilder<Db>();
                optionsBuilder.UseInMemoryDatabase("audit");
                return new Db(optionsBuilder.Options);
            });

            services.AddTransient<IApiGatewayProxyRequestAccessor>(provider => new ApiGatewayProxyRequestAccessor(request));
            services.AddTransient<ITenantExtractor, TenantExtractor>();
            services.AddTransient<ITenantParser, LambdaTenantParser>();
            services.AddTransient<AuditCreateService>();
            services.AddTransient<AuditGetAllService>();
            services.AddTransient<AuditGetByIdService>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}