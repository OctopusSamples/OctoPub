using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.SQSEvents;
using Audit.Service.Handler;
using Audit.Service.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLog;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace Audit.Service.Lambda
{
    /// <summary>
    /// The entrypoint to the Audits Lambda.
    /// </summary>
    public class Audits
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

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
                var serviceProvider = new DependencyInjection().ConfigureServices();
                var db = serviceProvider.GetRequiredService<Db>();
                db.Database.Migrate();
                return new APIGatewayProxyResponse
                {
                    StatusCode = 201
                };
            }
            catch (Exception ex)
            {
                return BuildError(ex);
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
                var serviceProvider = new DependencyInjection().ConfigureServices();
                var requestWrapper = RequestWrapperFactory.CreateFromHttpRequest(request);
                var handler = serviceProvider.GetRequiredService<AuditHandler>();
                return AddCors(ProcessRequest(handler, requestWrapper));
            }
            catch (Exception ex)
            {
                return BuildError(ex);
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

            var serviceProvider = new DependencyInjection().ConfigureServices();
            sqsEvent.Records
                .Select(m => new Thread(() =>
                {
                    try
                    {
                        Logger.Debug(System.Text.Json.JsonSerializer.Serialize(m));

                        var requestWrapper = RequestWrapperFactory.CreateFromSqsMessage(m);

                        Logger.Debug(System.Text.Json.JsonSerializer.Serialize(requestWrapper));
                        Logger.Debug(requestWrapper.Entity);

                        var handler = serviceProvider.GetRequiredService<AuditHandler>();
                        var audit = ProcessRequest(handler, requestWrapper);

                        Logger.Debug(System.Text.Json.JsonSerializer.Serialize(audit));
                    }
                    catch (Exception ex)
                    {
                        // need to do something here to allow sagas to roll themselves back
                        Logger.Error(Constants.ServiceName + "-SQS-GeneralFailure", ex);
                    }
                }))
                .Select(t =>
                {
                    t.Start();
                    return t;
                })
                .ToList()
                .ForEach(t => t.Join());
        }

        private APIGatewayProxyResponse ProcessRequest(AuditHandler handler, RequestWrapper wrapper)
        {
            return handler.GetAll(wrapper)
                   ?? handler.GetOne(wrapper)
                   ?? handler.CreateOne(wrapper)
                   ?? handler.GetHealth(wrapper)
                   ?? BuildNotFound();
        }

        private APIGatewayProxyResponse AddCors(APIGatewayProxyResponse response)
        {
            if (response.Headers == null) response.Headers = new Dictionary<string, string>();

            response.Headers.Add("Access-Control-Allow-Origin", "*");
            return response;
        }

        private APIGatewayProxyResponse BuildError(Exception ex)
        {
            return new APIGatewayProxyResponse
            {
                Body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    errors = new[]
                    {
                        new { code = ex.GetType().Name }
                    }
                }),
                StatusCode = 500
            };
        }

        private APIGatewayProxyResponse BuildNotFound()
        {
            return new APIGatewayProxyResponse
            {
                Body = System.Text.Json.JsonSerializer.Serialize(new
                {
                    errors = new[]
                    {
                        new { title = "Resource was not found" }
                    }
                }),
                StatusCode = 404
            };
        }
    }
}