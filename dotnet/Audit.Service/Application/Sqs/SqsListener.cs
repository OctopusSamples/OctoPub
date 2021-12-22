using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon.SQS;
using Amazon.SQS.Model;
using Audit.Service.Application.Lambda;
using Microsoft.Extensions.DependencyInjection;
using NLog;

namespace Audit.Service.Application.Sqs
{
    /// <summary>
    /// Listens to new SQS messages and passes them to the Lambda handler.
    /// </summary>
    public class SqsListener
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// When operating as a local development branch, we listen to the specified SQS branch for new message
        /// and process them like we would if the messages were received by a Lambda.
        /// </summary>
        /// <param name="o">The command line options.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task ListenSqs(Options o)
        {
            var keepRunning = true;

            Console.CancelKeyPress += (sender, e) => { keepRunning = false; };

            var sqsClient = new AmazonSQSClient();
            var audits = new Audits();
            var serviceProvider = new DependencyInjection().ConfigureServices();

            do
            {
                var msg = await ReceiveMessages(sqsClient, o.SqsQueue);

                foreach (var msgMessage in msg.Messages)
                {
                    await ProcessMessage(audits, sqsClient, msgMessage, serviceProvider, o.SqsQueue);
                }
            }
            while (keepRunning);
        }

        private async Task ProcessMessage(Audits audits, AmazonSQSClient sqsClient, Message msgMessage,
            ServiceProvider serviceProvider, string queueUrl)
        {
            try
            {
                // We have to do some conversions between the different representations of SQS messages and values
                var sqsMessage = msgMessage.ConvertToSqsMessage();
                audits.ProcessMessage(sqsMessage, serviceProvider);
                await sqsClient.DeleteMessageAsync(queueUrl, msgMessage.ReceiptHandle);
            }
            catch (Exception ex)
            {
                Logger.Error(Constants.ServiceName + "-LocalSQS-GeneralFailure", ex);
            }
        }

        private async Task<ReceiveMessageResponse> ReceiveMessages(AmazonSQSClient sqsClient, string queueUrl)
        {
            var attributes = new List<string>
            {
                "action",
                "dataPartition",
                "entity"
            };

            return await sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
            {
                QueueUrl = queueUrl,
                MaxNumberOfMessages = 10,
                WaitTimeSeconds = 10,
                MessageAttributeNames = attributes
            });
        }
    }
}