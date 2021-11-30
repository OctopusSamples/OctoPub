using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Audit.Service.Lambda;
using Audit.Service.Services;
using JsonApiSerializer;
using Newtonsoft.Json;

namespace Audit.Service.Handler
{
    /// <summary>
    ///     This class is created by the DI provider, and does the work of mapping requests to responses.
    /// </summary>
    public class AuditHandler
    {
        private readonly AuditCreateService auditCreateService;
        private readonly AuditGetAllService auditGetAllService;
        private readonly AuditGetByIdService auditGetByIdService;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditHandler"/> class.
        /// </summary>
        /// <param name="auditCreateService">The service used to create new resources.</param>
        /// <param name="auditGetAllService">The service used to return resource collections.</param>
        /// <param name="auditGetByIdService">The service used to return single resources.</param>
        public AuditHandler(
            AuditCreateService auditCreateService,
            AuditGetAllService auditGetAllService,
            AuditGetByIdService auditGetByIdService)
        {
            this.auditCreateService = auditCreateService;
            this.auditGetAllService = auditGetAllService;
            this.auditGetByIdService = auditGetByIdService;
        }

        /// <summary>
        ///     Returns the health check details.
        /// </summary>
        /// <returns>The health check details if the path and method are a match, or null otherwise.</returns>
        public APIGatewayProxyResponse? GetHealth(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Read &&
                  wrapper.EntityType == EntityType.Health))
                return null;

            return new APIGatewayProxyResponse
            {
                Body = "{\"message\": \"OK\"}",
                StatusCode = 200
            };
        }

        /// <summary>
        ///     Returns all the audit records.
        /// </summary>
        /// <returns>The audit records if the path and method are a match, or null otherwise.</returns>
        public APIGatewayProxyResponse? GetAll(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.ReadAll &&
                  wrapper.EntityType == EntityType.Audit))
                return null;

            return new APIGatewayProxyResponse
            {
                Body = JsonConvert.SerializeObject(
                    auditGetAllService.Get(
                        new List<string> { wrapper.DataPartition, Constants.DefaultPartition },
                        wrapper.Filter),
                    new JsonApiSerializerSettings()),
                StatusCode = 200
            };
        }

        /// <summary>
        ///     Returns one audit record.
        /// </summary>
        /// <returns>The audit record if the path and method are a match, or null otherwise.</returns>
        public APIGatewayProxyResponse? GetOne(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Read &&
                  wrapper.EntityType == EntityType.Audit))
                return null;

            var result = auditGetByIdService.Get(wrapper.Id, wrapper);

            if (result != null && (result.DataPartition == wrapper.DataPartition ||
                                   result.DataPartition == Constants.DefaultPartition))
                return new APIGatewayProxyResponse
                {
                    Body = JsonConvert.SerializeObject(result, new JsonApiSerializerSettings()),
                    StatusCode = 200
                };

            return BuildNotFound();
        }

        /// <summary>
        ///     Returns all the audit records.
        /// </summary>
        /// <returns>The audit records if the path and method are a match, or null otherwise.</returns>
        public APIGatewayProxyResponse? CreateOne(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Create &&
                  wrapper.EntityType == EntityType.Audit))
                return null;

            try
            {
                var entity = JsonConvert.DeserializeObject<Models.Audit>(
                    wrapper.Entity,
                    new JsonApiSerializerSettings());

                if (string.IsNullOrWhiteSpace(entity.Action) ||
                    string.IsNullOrWhiteSpace(entity.Object) ||
                    string.IsNullOrWhiteSpace(entity.Subject))
                    return BuildRequestError("One or more required fields were not supplied");

                entity.Id = null;
                entity.DataPartition = wrapper.DataPartition;
                var newEntity = auditCreateService.Create(entity);
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

        private APIGatewayProxyResponse BuildRequestError(string message)
        {
            return new APIGatewayProxyResponse
            {
                Body =
                    $"{{\"errors\": [{{\"title\": \"{message}\"}}]}}",
                StatusCode = 401
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