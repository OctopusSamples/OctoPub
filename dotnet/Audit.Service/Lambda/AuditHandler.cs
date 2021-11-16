using System;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Audit.Service.Services.InMemory;
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

        public AuditHandler(AuditCreateService auditCreateService, AuditGetAllService auditGetAllService,
            AuditGetByIdService auditGetByIdService)
        {
            _auditCreateService = auditCreateService;
            _auditGetAllService = auditGetAllService;
            _auditGetByIdService = auditGetByIdService;
        }

        /// <summary>
        /// Returns the health check details
        /// </summary>
        /// <returns>The health check details if the path and method are a match, or null otherwise</returns>
        public APIGatewayProxyResponse GetHealth(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Read &&
                  wrapper.EntityType != EntityType.Health))
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
        public async Task<APIGatewayProxyResponse> GetAll(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Read &&
                  wrapper.EntityType == EntityType.Collection))
            {
                return null;
            }

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(await _auditGetAllService.GetAsync(wrapper),
                    new JsonApiSerializerSettings()),
                StatusCode = 200
            };
        }

        /// <summary>
        /// Returns one audit record
        /// </summary>
        /// <returns>The audit record if the path and method are a match, or null otherwise</returns>
        public async Task<APIGatewayProxyResponse> GetOne(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Read &&
                  wrapper.EntityType == EntityType.Individual))
            {
                return null;
            }

            var result = await _auditGetByIdService.GetAsync(wrapper.Id, wrapper);

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
        public async Task<APIGatewayProxyResponse> CreateOne(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Create &&
                  wrapper.EntityType == EntityType.Individual))
            {
                return null;
            }

            try
            {
                var entity = JsonConvert.DeserializeObject<Models.Audit>(wrapper.Entity,
                    new JsonApiSerializerSettings());
                var newEntity = await _auditCreateService.CreateAsync(entity, wrapper);
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
                        $"{{\"message\": \"{ex}\", \"input\": \"{wrapper.Entity}\"}}",
                    StatusCode = 500
                };
            }
        }
    }
}