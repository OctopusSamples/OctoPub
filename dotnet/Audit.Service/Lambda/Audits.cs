﻿using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Serialization.Json;
using Amazon.Lambda.SQSEvents;
using Microsoft.Extensions.DependencyInjection;

[assembly: LambdaSerializer(typeof(JsonSerializer))]

namespace Audit.Service.Lambda
{
    public class Audits
    {
        private static readonly DependencyInjection DependencyInjection = new DependencyInjection();

        /// <summary>
        /// This is the HTTP entry point to the Lambda.
        /// </summary>
        /// <param name="request">The request details in proxy format</param>
        /// <param name="context">The lambda context</param>
        /// <returns>The API content</returns>
        public APIGatewayProxyResponse AuditsApi(APIGatewayProxyRequest request, ILambdaContext context)
        {
            try
            {
                var serviceProvider = DependencyInjection.ConfigureServices();
                var requestWrapper = RequestWrapperFactory.CreateFromHttpRequest(request);
                var handler = serviceProvider.GetService<AuditHandler>();
                return ProcessRequest(handler, requestWrapper);
            }
            catch (Exception ex)
            {
                return new APIGatewayProxyResponse
                {
                    Body = System.Text.Json.JsonSerializer.Serialize(ex.ToString()),
                    StatusCode = 500
                };
            }
        }

        /// <summary>
        /// The Message bus entry point to the Lambda.
        /// </summary>
        /// <param name="sqsEvent">The SQS event details</param>
        /// <param name="context">The SQS context</param>
        public void HandleSqsEvent(SQSEvent sqsEvent, ILambdaContext context)
        {
            Console.Out.WriteLine("Audits.HandleSqsEvent(SQSEvent, ILambdaContext)");
            Console.Out.WriteLine(sqsEvent.Records.Count + " records to process");

            var serviceProvider = DependencyInjection.ConfigureServices();
            sqsEvent.Records
                .Select(m => new Thread(() =>
                {
                    try
                    {
                        Console.Out.WriteLine(System.Text.Json.JsonSerializer.Serialize(m));

                        var requestWrapper = RequestWrapperFactory.CreateFromSqsMessage(m);

                        Console.Out.WriteLine(System.Text.Json.JsonSerializer.Serialize(requestWrapper));
                        Console.Out.WriteLine(requestWrapper.Entity);

                        var handler = serviceProvider.GetService<AuditHandler>();
                        var audit = ProcessRequest(handler, requestWrapper);

                        Console.Out.WriteLine(System.Text.Json.JsonSerializer.Serialize(audit));
                    }
                    catch (Exception ex)
                    {
                        // need to do something here to allow sagas to roll themselves back
                        Console.Out.WriteLine(ex);
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
                   ?? new APIGatewayProxyResponse
                   {
                       Body = "{\"message\": \"path not found\"}",
                       StatusCode = 404
                   };
        }
    }
}