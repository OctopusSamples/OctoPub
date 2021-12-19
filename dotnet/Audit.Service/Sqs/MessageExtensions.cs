using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.SQSEvents;
using Amazon.SQS.Model;

namespace Audit.Service.Sqs
{
    /// <summary>
    /// Provides extension methods to converting message classes.
    /// </summary>
    public static class MessageExtensions
    {
        /// <summary>
        /// Convert a Message to an SQSMessage.
        /// </summary>
        /// <param name="message">The Message to convert.</param>
        /// <returns>The SQSMessage converted from the Message.</returns>
        public static SQSEvent.SQSMessage ConvertToSqsMessage(this Message message)
        {
            return new SQSEvent.SQSMessage()
            {
                Attributes = message.Attributes,
                Body = message.Body,
                MessageId = message.MessageId,
                MessageAttributes = message.MessageAttributes
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
                ReceiptHandle = message.ReceiptHandle,
                Md5OfBody = message.MD5OfBody,
                Md5OfMessageAttributes = message.MD5OfMessageAttributes
            };
        }
    }
}