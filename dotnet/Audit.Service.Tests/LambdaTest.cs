using System;
using System.Text.Json;
using System.Threading.Tasks;
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
        public async Task GetAudits()
        {
            var response =
                await Audits.AuditsApi(new APIGatewayProxyRequest { HttpMethod = "get", Path = "/api/audits" }, null);
            Assert.IsNotNull(response);

            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            await Console.Out.WriteLineAsync(JsonSerializer.Serialize(response, serializeOptions));
        }

        [Test]
        public async Task CreateAudit()
        {
            var response =
                await Audits.AuditsApi(
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
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            await Console.Out.WriteLineAsync(JsonSerializer.Serialize(response, serializeOptions));
        }
    }
}