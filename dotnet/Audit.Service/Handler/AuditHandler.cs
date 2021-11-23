using System;
using Amazon.Lambda.APIGatewayEvents;
using Audit.Service.Lambda;
using Audit.Service.Services;
using JsonApiSerializer;
using Newtonsoft.Json;

namespace Audit.Service.Handler
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
        public APIGatewayProxyResponse GetAll(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.ReadAll &&
                  wrapper.EntityType == EntityType.Audit))
            {
                return null;
            }

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(_auditGetAllService.Get(wrapper),
                    new JsonApiSerializerSettings()),
                StatusCode = 200
            };
        }

        /// <summary>
        /// Returns one audit record
        /// </summary>
        /// <returns>The audit record if the path and method are a match, or null otherwise</returns>
        public APIGatewayProxyResponse GetOne(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Read &&
                  wrapper.EntityType == EntityType.Audit))
            {
                return null;
            }

            var result = _auditGetByIdService.Get(wrapper.Id, wrapper);

            if (result != null)
            {
                return new APIGatewayProxyResponse
                {
                    Body = JsonConvert.SerializeObject(result, new JsonApiSerializerSettings()),
                    StatusCode = 200
                };
            }

            return BuildNotFound();
        }

        /// <summary>
        /// Returns all the audit records
        /// </summary>
        /// <returns>The audit records if the path and method are a match, or null otherwise</returns>
        public APIGatewayProxyResponse CreateOne(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Create &&
                  wrapper.EntityType == EntityType.Audit))
            {
                return null;
            }

            try
            {
                var entity = JsonConvert.DeserializeObject<Models.Audit>(wrapper.Entity,
                    new JsonApiSerializerSettings());
                var newEntity = _auditCreateService.Create(entity, wrapper);
                return new APIGatewayProxyResponse
                {
                    Body = JsonConvert.SerializeObject(newEntity, new JsonApiSerializerSettings()),
                    StatusCode = 200
                };
            }
            catch (Exception ex)
            {
                return BuildServerError(ex);
            }
        }

        private APIGatewayProxyResponse BuildServerError(Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                Body =
                    $"{{\"errors\": [{{\"code\": \"{ex.GetType().Name}\"}}]}}",
                StatusCode = 500
            };
        }

        private APIGatewayProxyResponse BuildNotFound()
        {
            return new APIGatewayProxyResponse
            {
                Body = "{\"errors\": [{\"title\": \"Resource not found\"}]}",
                StatusCode = 404
            };
        }
    }
}