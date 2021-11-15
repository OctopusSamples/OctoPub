using System;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Lambda.APIGatewayEvents;
using audit_service.Lambda;
using NUnit.Framework;

namespace Audit.Service.Tests
{
    public class Tests
    {
        private static readonly Audits Audits = new Audits();

        [Test]
        public async Task Test1()
        {
            var response = await Audits.AuditsApi(new APIGatewayProxyRequest(), null);
            Assert.IsNotNull(response);

            var serializeOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            Console.Out.WriteLine(JsonSerializer.Serialize(response, serializeOptions));
        }
    }
}