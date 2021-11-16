using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Audit.Service.Services.InMemory;
using Audit.Service.Services.Lambda;
using JsonApiSerializer;
using Newtonsoft.Json;

namespace Audit.Service.Lambda
{
    /// <summary>
    /// This class is created by the DI provider, and does the work of mapping requests to responses.
    /// </summary>
    public class AuditHandler
    {
        private readonly AuditCreateService _auditCreateService;
        private readonly AuditGetAllService _auditGetAllService;
        private readonly AuditGetByIdService _auditGetByIdService;
        private readonly IRequestWrapperAccessor _requestWrapperAccessor;

        public AuditHandler(AuditCreateService auditCreateService, AuditGetAllService auditGetAllService,
            AuditGetByIdService auditGetByIdService, IRequestWrapperAccessor requestWrapperAccessor)
        {
            _auditCreateService = auditCreateService;
            _auditGetAllService = auditGetAllService;
            _auditGetByIdService = auditGetByIdService;
            _requestWrapperAccessor = requestWrapperAccessor;
        }

        /// <summary>
        /// Returns the health check details
        /// </summary>
        /// <returns>The health check details if the path and method are a match, or null otherwise</returns>
        public APIGatewayProxyResponse GetHealth()
        {
            if (_requestWrapperAccessor.RequestWrapper.ActionType != ActionType.Read ||
                _requestWrapperAccessor.RequestWrapper.EntityType != EntityType.Health )
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
            if (!(_requestWrapperAccessor.RequestWrapper.ActionType == ActionType.Read &&
                _requestWrapperAccessor.RequestWrapper.EntityType == EntityType.Collection))
            {
                return null;
            }

            var token = new CancellationTokenSource().Token;
            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(await _auditGetAllService.GetAsync(token),
                    new JsonApiSerializerSettings()),
                StatusCode = 200
            };
        }

        /// <summary>
        /// Returns one audit record
        /// </summary>
        /// <returns>The audit record if the path and method are a match, or null otherwise</returns>
        public async Task<APIGatewayProxyResponse> GetOne()
        {
            if (!(_requestWrapperAccessor.RequestWrapper.ActionType == ActionType.Read &&
                _requestWrapperAccessor.RequestWrapper.EntityType == EntityType.Individual))
            {
                return null;
            }

            var token = new CancellationTokenSource().Token;
            var result = await _auditGetByIdService.GetAsync(_requestWrapperAccessor.RequestWrapper.Id, token);

            if (result != null)
            {
                return new APIGatewayProxyResponse
                {
                    Body = JsonConvert.SerializeObject(result, new JsonApiSerializerSettings()),
                    StatusCode = 200
                };
            }

            return new APIGatewayProxyResponse
            {
                Body = "{\"message\": \"Entity not found\"}",
                StatusCode = 404
            };
        }

        /// <summary>
        /// Returns all the audit records
        /// </summary>
        /// <returns>The audit records if the path and method are a match, or null otherwise</returns>
        public async Task<APIGatewayProxyResponse> CreateOne()
        {
            if (!(_requestWrapperAccessor.RequestWrapper.ActionType == ActionType.Create &&
                _requestWrapperAccessor.RequestWrapper.EntityType == EntityType.Individual))
            {
                return null;
            }

            try
            {
                var token = new CancellationTokenSource().Token;
                var entity = JsonConvert.DeserializeObject<Models.Audit>(_requestWrapperAccessor.RequestWrapper.Entity,
                    new JsonApiSerializerSettings());
                var newEntity = await _auditCreateService.CreateAsync(entity, token);
                return new APIGatewayProxyResponse
                {
                    Body = JsonConvert.SerializeObject(newEntity, new JsonApiSerializerSettings()),
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return new APIGatewayProxyResponse
                {
                    Body =
                        $"{{\"message\": \"{ex}\", \"input\": \"{_requestWrapperAccessor.RequestWrapper.Entity}\"}}",
                    StatusCode = 500
                };
            }
        }
    }
}