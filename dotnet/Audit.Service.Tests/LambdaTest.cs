using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Audit.Service.Lambda;
using JsonApiSerializer;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Audit.Service.Tests
{
    public class Tests
    {
        private static readonly Audits Audits = new Audits();

        [SetUp]
        public void SetUp()
        {
            Audits.AuditsDbMigration(new APIGatewayProxyRequest(), null);
        }

        [Test]
        public void GetAudits()
        {
            var response =
                Audits.AuditsApi(new APIGatewayProxyRequest { HttpMethod = "get", Path = "/api/audits" }, null);
            Assert.IsNotNull(response);
        }

        [Test]
        public void GetAuditsFiltering()
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits", Body = JsonConvert.SerializeObject(new Models.Audit
                        {
                            Action = "test1",
                            Object = "test2",
                            Subject = "test3"
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Models.Audit>(response.Body,
                new JsonApiSerializerSettings());

            var getResponse =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "get", Path = "/api/audits",
                        QueryStringParameters = new Dictionary<string, string>() { { "filter", "id==" + entity.Id } }
                    }, null);

            var list = JsonConvert.DeserializeObject<List<Models.Audit>>(getResponse.Body,
                new JsonApiSerializerSettings());

            Assert.IsTrue(list.Any(p => p.Id == entity.Id));

            var getResponse2 =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "get", Path = "/api/audits",
                        QueryStringParameters = new Dictionary<string, string>() { { "filter", "subject==doesnotexist" } }
                    }, null);

            var list2 = JsonConvert.DeserializeObject<List<Models.Audit>>(getResponse2.Body,
                new JsonApiSerializerSettings());

            Assert.IsTrue(list2.Count == 0);
        }

        [Test]
        public void CreateAudit()
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits", Body = JsonConvert.SerializeObject(new Models.Audit
                        {
                            Action = "test1",
                            Object = "test2",
                            Subject = "test3"
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Models.Audit>(response.Body,
                new JsonApiSerializerSettings());

            Assert.AreEqual("test1", entity.Action);
            Assert.AreEqual("test2", entity.Object);
            Assert.AreEqual("test3", entity.Subject);
        }

        /// <summary>
        /// Test to ensure that you can not request an audit resource from another tenant
        /// </summary>
        [Test]
        public void FailGetAudit()
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits",
                        MultiValueHeaders = new Dictionary<string, IList<string>>()
                        {
                            {
                                "Accept",
                                new List<String>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=testing" }
                            },
                        },
                        Body = JsonConvert.SerializeObject(new Models.Audit
                        {
                            Action = "test",
                            Object = "test",
                            Subject = "test"
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Models.Audit>(response.Body,
                new JsonApiSerializerSettings());

            var getResponse =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "GET", Path = "/api/audits/" + entity.Id,
                        MultiValueHeaders = new Dictionary<string, IList<string>>()
                        {
                            {
                                "Accept",
                                new List<String>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=main" }
                            }
                        },
                    }, null);

            Assert.AreEqual(404, getResponse.StatusCode);
        }

        /// <summary>
        /// Test to ensure that you can not request an audit resource from another tenant
        /// </summary>
        [Test]
        public void FailGetAudits()
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits",
                        MultiValueHeaders = new Dictionary<string, IList<string>>()
                        {
                            {
                                "Accept",
                                new List<String>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=testing" }
                            },
                        },
                        Body = JsonConvert.SerializeObject(new Models.Audit
                        {
                            Action = "test",
                            Object = "test",
                            Subject = "test"
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Models.Audit>(response.Body,
                new JsonApiSerializerSettings());

            var getResponse =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "GET", Path = "/api/audits",
                        MultiValueHeaders = new Dictionary<string, IList<string>>()
                        {
                            {
                                "Accept",
                                new List<String>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=main" }
                            }
                        },
                    }, null);

            var list = JsonConvert.DeserializeObject<List<Models.Audit>>(getResponse.Body,
                new JsonApiSerializerSettings());

            Assert.False(list.Any(a => a.Id == entity.Id));
        }
    }
}