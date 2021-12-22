using System;
using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.SQSEvents;
using Audit.Service.Application.Lambda;
using JsonApiSerializer;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Audit.Service.Tests
{
    public class Tests
    {
        private static readonly Audits Audits = new Audits();

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
                        HttpMethod = "POST", Path = "/api/audits", Body = JsonConvert.SerializeObject(new Domain.Entities.Audit
                        {
                            Action = "test1",
                            Object = "test2",
                            Subject = "test3"
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Domain.Entities.Audit>(response.Body,
                new JsonApiSerializerSettings());

            var getResponse =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "get", Path = "/api/audits",
                        QueryStringParameters = new Dictionary<string, string> { { "filter", "id==" + entity.Id } }
                    }, null);

            var list = JsonConvert.DeserializeObject<List<Domain.Entities.Audit>>(getResponse.Body,
                new JsonApiSerializerSettings());

            Assert.IsTrue(list.Any(p => p.Id == entity.Id));

            var getResponse2 =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "get", Path = "/api/audits",
                        QueryStringParameters = new Dictionary<string, string> { { "filter", "subject==doesnotexist" } }
                    }, null);

            var list2 = JsonConvert.DeserializeObject<List<Domain.Entities.Audit>>(getResponse2.Body,
                new JsonApiSerializerSettings());

            Assert.IsTrue(list2.Count == 0);
        }

        [Test]
        public void GetAuditsFilteringCross()
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits", Body = JsonConvert.SerializeObject(new Domain.Entities.Audit
                        {
                            Action = "test1",
                            Object = "test2",
                            Subject = "test3"
                        }, new JsonApiSerializerSettings()),
                        MultiValueHeaders = new Dictionary<string, IList<string>>
                        {
                            {
                                "Accept",
                                new List<string>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=main" }
                            }
                        }
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Domain.Entities.Audit>(response.Body,
                new JsonApiSerializerSettings());

            // Attempt to get the record in the context of another partition
            var getResponse =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "get", Path = "/api/audits",
                        QueryStringParameters = new Dictionary<string, string> { { "filter", "id==" + entity.Id } },
                        MultiValueHeaders = new Dictionary<string, IList<string>>
                        {
                            {
                                "Accept",
                                new List<string>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=testing2" }
                            }
                        }
                    }, null);

            var list = JsonConvert.DeserializeObject<List<Domain.Entities.Audit>>(getResponse.Body,
                new JsonApiSerializerSettings());

            Assert.IsTrue(list.Count == 1);
        }

        [Test]
        public void FailGetAuditsFilteringCross()
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits", Body = JsonConvert.SerializeObject(new Domain.Entities.Audit
                        {
                            Action = "test1",
                            Object = "test2",
                            Subject = "test3"
                        }, new JsonApiSerializerSettings()),
                        MultiValueHeaders = new Dictionary<string, IList<string>>
                        {
                            {
                                "Accept",
                                new List<string>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=testing1" }
                            }
                        }
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Domain.Entities.Audit>(response.Body,
                new JsonApiSerializerSettings());

            // Attempt to get the record in the context of another partition
            var getResponse =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "get", Path = "/api/audits",
                        QueryStringParameters = new Dictionary<string, string> { { "filter", "id==" + entity.Id } },
                        MultiValueHeaders = new Dictionary<string, IList<string>>
                        {
                            {
                                "Accept",
                                new List<string>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=testing2" }
                            }
                        }
                    }, null);

            var list = JsonConvert.DeserializeObject<List<Domain.Entities.Audit>>(getResponse.Body,
                new JsonApiSerializerSettings());

            Assert.IsTrue(list.Count == 0);
        }

        [Test]
        public void CreateAudit()
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits", Body = JsonConvert.SerializeObject(new Domain.Entities.Audit
                        {
                            Action = "test1",
                            Object = "test2",
                            Subject = "test3"
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Domain.Entities.Audit>(response.Body,
                new JsonApiSerializerSettings());

            Assert.AreEqual("test1", entity.Action);
            Assert.AreEqual("test2", entity.Object);
            Assert.AreEqual("test3", entity.Subject);
        }

        [Test]
        public void CreateAuditFromMessage()
        {
            var subject = Guid.NewGuid();

            Audits.HandleSqsEvent(
                new SQSEvent
                {
                    Records = new List<SQSEvent.SQSMessage>
                    {
                        new SQSEvent.SQSMessage
                        {
                            Body = JsonConvert.SerializeObject(new Domain.Entities.Audit
                            {
                                Action = "test1",
                                Object = "test2",
                                Subject = subject.ToString()
                            }, new JsonApiSerializerSettings()),
                            MessageAttributes = new Dictionary<string, SQSEvent.MessageAttribute>
                            {
                                { "action", new SQSEvent.MessageAttribute { StringValue = "Create" } },
                                { "entity", new SQSEvent.MessageAttribute { StringValue = "Audit" } }
                            }
                        }
                    }
                }, null);

            var getResponse =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "get", Path = "/api/audits",
                        QueryStringParameters = new Dictionary<string, string>
                            { { "filter", "subject=='" + subject + "'" } }
                    }, null);

            var list = JsonConvert.DeserializeObject<List<Domain.Entities.Audit>>(getResponse.Body,
                new JsonApiSerializerSettings());

            Assert.IsTrue(list.Count == 1);
        }

        /// <summary>
        ///     Test to ensure that you can not request an audit resource from another tenant
        /// </summary>
        [Test]
        public void FailGetAudit()
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits",
                        MultiValueHeaders = new Dictionary<string, IList<string>>
                        {
                            {
                                "Accept",
                                new List<string>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=testing" }
                            }
                        },
                        Body = JsonConvert.SerializeObject(new Domain.Entities.Audit
                        {
                            Action = "test",
                            Object = "test",
                            Subject = "test"
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Domain.Entities.Audit>(response.Body,
                new JsonApiSerializerSettings());

            var getResponse =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "GET", Path = "/api/audits/" + entity.Id,
                        MultiValueHeaders = new Dictionary<string, IList<string>>
                        {
                            {
                                "Accept",
                                new List<string>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=main" }
                            }
                        }
                    }, null);

            Assert.AreEqual(404, getResponse.StatusCode);
        }

        /// <summary>
        ///     Test to ensure that you can not request an audit resource from another tenant
        /// </summary>
        [Test]
        public void FailGetAudits()
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits",
                        MultiValueHeaders = new Dictionary<string, IList<string>>
                        {
                            {
                                "Accept",
                                new List<string>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=testing" }
                            }
                        },
                        Body = JsonConvert.SerializeObject(new Domain.Entities.Audit
                        {
                            Action = "test",
                            Object = "test",
                            Subject = "test"
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);

            var entity = JsonConvert.DeserializeObject<Domain.Entities.Audit>(response.Body,
                new JsonApiSerializerSettings());

            var getResponse =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "GET", Path = "/api/audits",
                        MultiValueHeaders = new Dictionary<string, IList<string>>
                        {
                            {
                                "Accept",
                                new List<string>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=main" }
                            }
                        }
                    }, null);

            var list = JsonConvert.DeserializeObject<List<Domain.Entities.Audit>>(getResponse.Body,
                new JsonApiSerializerSettings());

            Assert.False(list.Any(a => a.Id == entity.Id));
        }

        [Test]
        [TestCase("", "test", "test")]
        [TestCase(" ", "test", "test")]
        [TestCase("test", "", "test")]
        [TestCase("test", " ", "test")]
        [TestCase("test", "test", "")]
        [TestCase("test", "test", " ")]
        public void FailWithMissingFields(string action, string objectField, string subject)
        {
            var response =
                Audits.AuditsApi(
                    new APIGatewayProxyRequest
                    {
                        HttpMethod = "POST", Path = "/api/audits",
                        MultiValueHeaders = new Dictionary<string, IList<string>>
                        {
                            {
                                "Accept",
                                new List<string>
                                    { "application/vnd.api+json", "application/vnd.api+json; dataPartition=testing" }
                            }
                        },
                        Body = JsonConvert.SerializeObject(new Domain.Entities.Audit
                        {
                            Action = action,
                            Object = objectField,
                            Subject = subject
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);
            Assert.AreEqual(401, response.StatusCode);
        }
    }
}