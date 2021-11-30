using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SQSEvents;
using Audit.Service.Lambda;
using NUnit.Framework;

namespace Audit.Service.Tests
{
    public class RequestWrapperTests
    {
        [Test]
        public void RequestWrapperFromHttpGetAll()
        {
            var wrapper = RequestWrapperFactory.CreateFromHttpRequest(new APIGatewayProxyRequest()
            {
                Path = "/api/audits",
                HttpMethod = "GET",
                Headers = new Dictionary<string, string>()
                {
                    { "Accept", "application/vnd.api+json" }
                }
            });

            Assert.AreEqual(EntityType.Audit, wrapper.EntityType);
            Assert.AreEqual(ActionType.ReadAll, wrapper.ActionType);
            Assert.AreEqual("main", wrapper.DataPartition);
        }

        [Test]
        public void RequestWrapperFromHttpHealthGet()
        {
            var wrapper = RequestWrapperFactory.CreateFromHttpRequest(new APIGatewayProxyRequest()
            {
                Path = "/health/audits/GET",
                HttpMethod = "GET",
                Headers = new Dictionary<string, string>()
                {
                    { "Accept", "application/vnd.api+json" }
                }
            });

            Assert.AreEqual(EntityType.Health, wrapper.EntityType);
            Assert.AreEqual(ActionType.Read, wrapper.ActionType);
            Assert.AreEqual("main", wrapper.DataPartition);
        }

        [Test]
        public void RequestWrapperFromHttpHealthCreate()
        {
            var wrapper = RequestWrapperFactory.CreateFromHttpRequest(new APIGatewayProxyRequest()
            {
                Path = "/health/audits/POST",
                HttpMethod = "GET",
                Headers = new Dictionary<string, string>()
                {
                    { "Accept", "application/vnd.api+json" }
                }
            });

            Assert.AreEqual(EntityType.Health, wrapper.EntityType);
            Assert.AreEqual(ActionType.Read, wrapper.ActionType);
            Assert.AreEqual("main", wrapper.DataPartition);
        }

        [Test]
        public void RequestWrapperFromHttpHealthGetOne()
        {
            var wrapper = RequestWrapperFactory.CreateFromHttpRequest(new APIGatewayProxyRequest()
            {
                Path = "/health/audits/1/GET",
                HttpMethod = "GET",
                Headers = new Dictionary<string, string>()
                {
                    { "Accept", "application/vnd.api+json" }
                }
            });

            Assert.AreEqual(EntityType.Health, wrapper.EntityType);
            Assert.AreEqual(ActionType.Read, wrapper.ActionType);
            Assert.AreEqual("main", wrapper.DataPartition);
        }

        [Test]
        public void RequestWrapperFromHttpGetAllMultiHeader()
        {
            var wrapper = RequestWrapperFactory.CreateFromHttpRequest(new APIGatewayProxyRequest()
            {
                Path = "/api/audits",
                HttpMethod = "GET",
                MultiValueHeaders = new Dictionary<string, IList<string>>()
                {
                    {
                        "Accept",
                        new List<string>()
                            { "application/vnd.api+json", "application/vnd.api+json; dataPartition=test" }
                    }
                }
            });

            Assert.AreEqual(EntityType.Audit, wrapper.EntityType);
            Assert.AreEqual(ActionType.ReadAll, wrapper.ActionType);
            Assert.AreEqual("test", wrapper.DataPartition);
        }

        [Test]
        public void RequestWrapperFromHttpCreate()
        {
            var wrapper = RequestWrapperFactory.CreateFromHttpRequest(new APIGatewayProxyRequest()
            {
                Path = "/api/audits",
                HttpMethod = "POST",
                Headers = new Dictionary<string, string>()
                {
                    { "Accept", "application/vnd.api+json" }
                }
            });

            Assert.AreEqual(EntityType.Audit, wrapper.EntityType);
            Assert.AreEqual(ActionType.Create, wrapper.ActionType);
            Assert.AreEqual("main", wrapper.DataPartition);
        }

        [Test]
        public void RequestWrapperFromHttpGet()
        {
            var wrapper = RequestWrapperFactory.CreateFromHttpRequest(new APIGatewayProxyRequest()
            {
                Path = "/api/audits/1",
                HttpMethod = "GET",
                Headers = new Dictionary<string, string>()
                {
                    { "Accept", "application/vnd.api+json" }
                }
            });

            Assert.AreEqual(EntityType.Audit, wrapper.EntityType);
            Assert.AreEqual(ActionType.Read, wrapper.ActionType);
            Assert.AreEqual("main", wrapper.DataPartition);
        }

        [Test]
        public void RequestWrapperFromHttpUpdate()
        {
            var wrapper = RequestWrapperFactory.CreateFromHttpRequest(new APIGatewayProxyRequest()
            {
                Path = "/api/audits/1",
                HttpMethod = "PATCH",
                Headers = new Dictionary<string, string>()
                {
                    { "Accept", "application/vnd.api+json" }
                }
            });

            Assert.AreEqual(EntityType.Audit, wrapper.EntityType);
            Assert.AreEqual(ActionType.Update, wrapper.ActionType);
            Assert.AreEqual("main", wrapper.DataPartition);
        }

        [Test]
        public void RequestWrapperFromSqsGetAll()
        {
            var wrapper = RequestWrapperFactory.CreateFromSqsMessage(new SQSEvent.SQSMessage()
            {
                MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>()
                {
                    { "action", new SQSEvent.MessageAttribute() { StringValue = "Create" } },
                    { "entity", new SQSEvent.MessageAttribute() { StringValue = "Audit" } },
                    {
                        "dataPartition",
                        new SQSEvent.MessageAttribute()
                        {
                            StringValue = "application/vnd.api+json, application/vnd.api+json; dataPartition=testing"
                        }
                    }
                }
            });

            Assert.AreEqual(EntityType.Audit, wrapper.EntityType);
            Assert.AreEqual(ActionType.Create, wrapper.ActionType);
            Assert.AreEqual("testing", wrapper.DataPartition);
        }
    }
}