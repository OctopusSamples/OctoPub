using System;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Audit.Service.Repositories.InMemory;
using Audit.Service.Services;
using Audit.Service.Services.InMemory;
using Audit.Service.Services.Lambda;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.CompilerServices;

namespace Audit.Service.Lambda
{
    public class Audits
    {
        private static bool _initializedDatabase = false;

        /// <summary>
        /// This is the entry point to the Lambda.
        /// </summary>
        /// <param name="request">The request details in proxy format</param>
        /// <param name="context">The lambda context</param>
        /// <returns>The API content</returns>
        [LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
        public async Task<APIGatewayProxyResponse> AuditsApi(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var serviceProvider = ConfigureServices(request);
                var handler = serviceProvider.GetService<AuditHandler>();

                return await handler.GetAll()
                       ?? await handler.GetOne()
                       ?? handler.GetHealth()
                       ?? new APIGatewayProxyResponse
                       {
                           Body = "{\"message\": \"path not found\"}",
                           StatusCode = 404
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

        /// <summary>
        /// Builds a dependency injection context.
        /// </summary>
        /// <param name="request">The details of the Lambda request</param>
        /// <returns>The DI service provider</returns>
        private ServiceProvider ConfigureServices(APIGatewayProxyRequest request)
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

            services.AddSingleton<IApiGatewayProxyRequestAccessor>(provider =>
                new ApiGatewayProxyRequestAccessor(request));
            services.AddSingleton<ITenantExtractor, TenantExtractor>();
            services.AddSingleton<ITenantParser, LambdaTenantParser>();
            services.AddSingleton<AuditCreateService>();
            services.AddSingleton<AuditGetAllService>();
            services.AddSingleton<AuditGetByIdService>();
            services.AddSingleton<AuditHandler>();

            return services.BuildServiceProvider();
        }
    }

    /// <summary>
    /// This class is created by the DI provider, and does the work of mapping requests to responses.
    /// </summary>
    public class AuditHandler
    {
        private static readonly Regex HealthRegex = new Regex(@"^/health/.*$");
        private static readonly Regex GetAllRegex = new Regex(@"^/api/audits/?$");
        private static readonly Regex GetOneRegex = new Regex(@"^/api/audits/(?<id>\d+)$");

        private readonly AuditCreateService _auditCreateService;
        private readonly AuditGetAllService _auditGetAllService;
        private readonly AuditGetByIdService _auditGetByIdService;
        private readonly IApiGatewayProxyRequestAccessor _apiGatewayProxyRequestAccessor;

        public AuditHandler(AuditCreateService auditCreateService, AuditGetAllService auditGetAllService,
            AuditGetByIdService auditGetByIdService, IApiGatewayProxyRequestAccessor apiGatewayProxyRequestAccessor)
        {
            _auditCreateService = auditCreateService;
            _auditGetAllService = auditGetAllService;
            _auditGetByIdService = auditGetByIdService;
            _apiGatewayProxyRequestAccessor = apiGatewayProxyRequestAccessor;
        }

        /// <summary>
        /// Returns the health check details
        /// </summary>
        /// <returns>The health check details if the path and method are a match, or null otherwise</returns>
        public APIGatewayProxyResponse GetHealth()
        {
            if (!HealthRegex.IsMatch(_apiGatewayProxyRequestAccessor.ApiGatewayProxyRequest.Path ?? string.Empty) ||
                _apiGatewayProxyRequestAccessor.ApiGatewayProxyRequest.HttpMethod?.ToLower() != "get")
            {
                return null;
            }

            return new APIGatewayProxyResponse
            {
                Body = "{\"message\": \"OK\"}",
                StatusCode = 200
            };
        }

        /// <summary>
        /// Returns all the audit records
        /// </summary>
        /// <returns>The audit records if the path and method are a match, or null otherwise</returns>
        public async Task<APIGatewayProxyResponse> GetAll()
        {
            if (!GetAllRegex.IsMatch(_apiGatewayProxyRequestAccessor.ApiGatewayProxyRequest.Path ?? string.Empty)||
                _apiGatewayProxyRequestAccessor.ApiGatewayProxyRequest.HttpMethod?.ToLower() != "get")
            {
                return null;
            }

            var token = new CancellationTokenSource().Token;
            return new APIGatewayProxyResponse
            {
                Body = JsonSerializer.Serialize(await _auditGetAllService.GetAsync(token)),
                StatusCode = 200
            };
        }

        /// <summary>
        /// Returns one audit record
        /// </summary>
        /// <returns>The audit record if the path and method are a match, or null otherwise</returns>
        public async Task<APIGatewayProxyResponse> GetOne()
        {
            var match = GetOneRegex.Match(_apiGatewayProxyRequestAccessor.ApiGatewayProxyRequest.Path ?? string.Empty);

            if (!match.Success ||
                _apiGatewayProxyRequestAccessor.ApiGatewayProxyRequest.HttpMethod?.ToLower() != "get")
            {
                return null;
            }

            var token = new CancellationTokenSource().Token;
            var result = await _auditGetByIdService.GetAsync(Int32.Parse(match.Groups["id"].Value), token);

            if (result != null)
            {
                return new APIGatewayProxyResponse
                {
                    Body = JsonSerializer.Serialize(result),
                    StatusCode = 200
                };
            }

            return new APIGatewayProxyResponse
            {
                Body = "{\"message\": \"Entity not found\"}",
                StatusCode = 404
            };
        }
    }
}