using System;
using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Audit.Service.Application.Lambda;
using Audit.Service.Domain.Interceptor;
using Audit.Service.Infrastructure.Services;
using JsonApiSerializer;
using Newtonsoft.Json;
using NLog;

namespace Audit.Service.Domain.Handler
{
    /// <summary>
    ///     This class is created by the DI provider, and does the work of mapping requests to responses.
    /// </summary>
    [LogMethod]
    public class AuditHandler
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private readonly AuditCreateService auditCreateService;
        private readonly AuditGetAllService auditGetAllService;
        private readonly AuditGetByIdService auditGetByIdService;
        private readonly IResponseBuilder responseBuilder;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditHandler"/> class.
        /// </summary>
        /// <param name="auditCreateService">The service used to create new resources.</param>
        /// <param name="auditGetAllService">The service used to return resource collections.</param>
        /// <param name="auditGetByIdService">The service used to return single resources.</param>
        /// <param name="responseBuilder">The response builder.</param>
        public AuditHandler(
            AuditCreateService auditCreateService,
            AuditGetAllService auditGetAllService,
            AuditGetByIdService auditGetByIdService,
            IResponseBuilder responseBuilder)
        {
            this.auditCreateService = auditCreateService;
            this.auditGetAllService = auditGetAllService;
            this.auditGetByIdService = auditGetByIdService;
            this.responseBuilder = responseBuilder;
        }

        /// <summary>
        /// Returns the health check details.
        /// </summary>
        /// <param name="wrapper">The details of the request.</param>
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
        /// Returns all the matching audit records.
        /// </summary>
        /// <param name="wrapper">The details of the request.</param>
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
        /// Returns one audit record.
        /// </summary>
        /// <param name="wrapper">The details of the request.</param>
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

            return responseBuilder.BuildNotFound();
        }

        /// <summary>
        /// Creates an audit record.
        /// </summary>
        /// <param name="wrapper">The details of the request.</param>
        /// <returns>The audit records if the path and method are a match, or null otherwise.</returns>
        public APIGatewayProxyResponse? CreateOne(RequestWrapper wrapper)
        {
            if (!(wrapper.ActionType == ActionType.Create &&
                  wrapper.EntityType == EntityType.Audit))
                return null;

            try
            {
                var entity = JsonConvert.DeserializeObject<Entities.Audit>(
                    wrapper.Entity,
                    new JsonApiSerializerSettings());

                if (string.IsNullOrWhiteSpace(entity.Action) ||
                    string.IsNullOrWhiteSpace(entity.Object) ||
                    string.IsNullOrWhiteSpace(entity.Subject))
                    return responseBuilder.BuildClientError("One or more required fields were not supplied");

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
                Logger.Error(Constants.ServiceName + "-Handler-CreateFailure", ex);
                return responseBuilder.BuildError(ex);
            }
        }
    }
}