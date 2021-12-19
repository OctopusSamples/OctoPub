using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.SQSEvents;
using Audit.Service.Handler;
using Audit.Service.Interceptor;
using Audit.Service.Repositories;
using Audit.Service.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace Audit.Service.Lambda
{
    /// <summary>
    /// The entrypoint to the Audits Lambda.
    /// </summary>
    [LogMethod]
    public class Audits
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static readonly DependencyInjection DependencyInjection = new DependencyInjection();

        /// <summary>
        ///     This is the entry point to the Lambda to run database migrations.
        /// </summary>
        /// <param name="request">The request details in proxy format.</param>
        /// <param name="context">The lambda context.</param>
        /// <returns>201 if the migration succeeded, or 500 if it failed.</returns>
        public APIGatewayProxyResponse AuditsDbMigration(APIGatewayProxyRequest request, ILambdaContext? context)
        {
            try
            {
                var serviceProvider = DependencyInjection.ConfigureServices();
                var responseBuilder = serviceProvider.GetRequiredService<IResponseBuilder>();

                try
                {
                    var db = serviceProvider.GetRequiredService<Db>();
                    db.Database.Migrate();
                    return new APIGatewayProxyResponse
                    {
                        StatusCode = 201
                    };
                }
                catch (Exception ex)
                {
                    Logger.Error(Constants.ServiceName + "-Migration-GeneralFailure:" + ex);
                    return responseBuilder.BuildError(ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Constants.ServiceName + "-Migration-DIFailure:" + ex);
                return BuildGenericError();
            }
        }

        /// <summary>
        ///     This is the HTTP entry point to the Lambda.
        /// </summary>
        /// <param name="request">The request details in proxy format.</param>
        /// <param name="context">The lambda context.</param>
        /// <returns>The API content.</returns>
        public APIGatewayProxyResponse AuditsApi(APIGatewayProxyRequest request, ILambdaContext? context)
        {
            try
            {
                var serviceProvider = DependencyInjection.ConfigureServices();
                var responseBuilder = serviceProvider.GetRequiredService<IResponseBuilder>();

                try
                {
                    var requestWrapper = RequestWrapperFactory.CreateFromHttpRequest(request);
                    var handler = serviceProvider.GetRequiredService<AuditHandler>();
                    return AddCors(ProcessRequest(handler, requestWrapper, responseBuilder));
                }
                catch (Exception ex)
                {
                    Logger.Error(Constants.ServiceName + "-Lambda-GeneralFailure:" + ex);
                    return responseBuilder.BuildError(ex);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(Constants.ServiceName + "-Lambda-DIFailure:" + ex);
                return BuildGenericError();
            }
        }

        /// <summary>
        ///     The Message bus entry point to the Lambda.
        /// </summary>
        /// <param name="sqsEvent">The SQS event details.</param>
        /// <param name="context">The SQS context.</param>
        public void HandleSqsEvent(SQSEvent sqsEvent, ILambdaContext? context)
        {
            Logger.Debug("Audits.HandleSqsEvent(SQSEvent, ILambdaContext)");
            Logger.Debug(sqsEvent.Records.Count + " records to process");

            var serviceProvider = DependencyInjection.ConfigureServices();

            sqsEvent.Records
                .Select(m => new Thread(() => ProcessMessage(m, serviceProvider)))
                .Select(t =>
                {
                    t.Start();
                    return t;
                })
                .ToList()
                .ForEach(t => t.Join());
        }

        /// <summary>
        /// Process an individual SQS message.
        /// </summary>
        /// <param name="message">The SQS message.</param>
        /// <param name="serviceProvider">The DI service provider.</param>
        public void ProcessMessage(SQSEvent.SQSMessage message, ServiceProvider serviceProvider)
        {
            try
            {
                var responseBuilder = serviceProvider.GetRequiredService<IResponseBuilder>();

                Logger.Debug(System.Text.Json.JsonSerializer.Serialize(message));

                var requestWrapper = RequestWrapperFactory.CreateFromSqsMessage(message);

                Logger.Debug(System.Text.Json.JsonSerializer.Serialize(requestWrapper));
                Logger.Debug(requestWrapper.Entity);

                var handler = serviceProvider.GetRequiredService<AuditHandler>();
                var audit = ProcessRequest(handler, requestWrapper, responseBuilder);

                Logger.Debug(System.Text.Json.JsonSerializer.Serialize(audit));
            }
            catch (Exception ex)
            {
                // need to do something here to allow sagas to roll themselves back
                Logger.Error(Constants.ServiceName + "-SQS-GeneralFailure", ex);
            }
        }

        private APIGatewayProxyResponse ProcessRequest(AuditHandler handler, RequestWrapper wrapper,
            IResponseBuilder responseBuilder)
        {
            return handler.GetAll(wrapper)
                   ?? handler.GetOne(wrapper)
                   ?? handler.CreateOne(wrapper)
                   ?? handler.GetHealth(wrapper)
                   ?? responseBuilder.BuildNotFound();
        }

        private APIGatewayProxyResponse AddCors(APIGatewayProxyResponse response)
        {
            if (response.Headers == null) response.Headers = new Dictionary<string, string>();

            response.Headers.Add("Access-Control-Allow-Origin", "*");
            return response;
        }

        private APIGatewayProxyResponse BuildGenericError()
        {
            return new APIGatewayProxyResponse
            {
                Body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    errors = new[]
                    {
                        new { code = "A unspecified error occured" }
                    }
                }),
                StatusCode = 500
            };
        }
    }
}