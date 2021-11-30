using System.Collections.Generic;
using Amazon.Lambda.APIGatewayEvents;
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
        public void RequestWrapperFromHttpGetAllMultiHeader()
        {
            var wrapper = RequestWrapperFactory.CreateFromHttpRequest(new APIGatewayProxyRequest()
            {
                Path = "/api/audits",
                HttpMethod = "GET",
                MultiValueHeaders = new Dictionary<string, IList<string>>()
                {
                    { "Accept", new List<string>() {"application/vnd.api+json", "application/vnd.api+json; dataPartition=test"} }
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
    }
}