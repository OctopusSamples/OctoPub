using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Audit.Service.Lambda;
using JsonApiSerializer;
using Newtonsoft.Json;
using NUnit.Framework;
using JsonSerializer = System.Text.Json.JsonSerializer;

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

            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            Console.Out.WriteLineAsync(JsonSerializer.Serialize(response, serializeOptions));
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
                            Action = "test",
                            Object = "test",
                            Subject = "test"
                        }, new JsonApiSerializerSettings())
                    }, null);
            Assert.IsNotNull(response);

            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            Console.Out.WriteLineAsync(JsonSerializer.Serialize(response, serializeOptions));
        }
    }
}