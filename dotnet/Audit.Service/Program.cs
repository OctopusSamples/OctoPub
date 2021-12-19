using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS;
using Amazon.SQS.Model;
using Audit.Service.Lambda;
using CommandLine;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Audit.Service
{
    /// <summary>
    /// The entry point for the self hosted web application.
    /// </summary>
    public static class Program
    {
        /// <summary>
        /// The application entry point.
        /// </summary>
        /// <param name="args">The arguments passed from the command line.</param>
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    AppMode fixedMode = Enum.TryParse<AppMode>(o.Mode, out var mode) ? mode : AppMode.Web;
                    switch (fixedMode)
                    {
                        case AppMode.Sqs:
                            ListenSqs(o).GetAwaiter().GetResult();
                            break;
                        default:
                            CreateHostBuilder(args)
                                .Build()
                                .MigrateDatabase()
                                .Run();
                            break;
                    }
                });
        }

        private static async Task ListenSqs(Options o)
        {
            var sqsClient = new AmazonSQSClient();
            var audits = new Audits();
            var serviceProvider = new DependencyInjection().ConfigureServices();

            do
            {
                var msg = await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    QueueUrl = o.SqsQueue,
                    MaxNumberOfMessages = 10,
                    WaitTimeSeconds = 10
                });

                foreach (var msgMessage in msg.Messages)
                {
                    // We have to do some conversions between the different representations of SQS messages and values
                    var sqsMessage = new SQSEvent.SQSMessage()
                    {
                        Attributes = msgMessage.Attributes,
                        Body = msgMessage.Body,
                        MessageId = msgMessage.MessageId,
                        MessageAttributes = msgMessage.MessageAttributes
                            .Select(p => new KeyValuePair<string, SQSEvent.MessageAttribute>(
                                p.Key,
                                new SQSEvent.MessageAttribute
                                {
                                    BinaryValue = p.Value.BinaryValue,
                                    DataType = p.Value.DataType,
                                    StringValue = p.Value.StringValue,
                                    BinaryListValues = p.Value.BinaryListValues,
                                    StringListValues = p.Value.StringListValues
                                }))
                            .ToDictionary(p => p.Key, p => p.Value),
                        ReceiptHandle = msgMessage.ReceiptHandle,
                        Md5OfBody = msgMessage.MD5OfBody,
                        Md5OfMessageAttributes = msgMessage.MD5OfMessageAttributes
                    };
                    audits.ProcessMessage(sqsMessage, serviceProvider);
                }
            }
            while (true);
        }

        /// <summary>
        /// Creates the host builder, exposing features like DI, logging, configuration etc.
        /// </summary>
        /// <param name="args">The main program arguments.</param>
        /// <returns>The host builder.</returns>
        private static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
        }
    }
}