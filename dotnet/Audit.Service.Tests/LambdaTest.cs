using System;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using audit_service.Lambda;
using NUnit.Framework;

namespace Audit.Service.Tests
{
    public class Tests
    {
        private static readonly Audits Audits = new Audits();

        [Test]
        public void Test1()
        {
            var response = Audits.AuditsApi(new APIGatewayProxyRequest(), null);
            Assert.IsNotNull(response);

            Console.Out.WriteLine(JsonSerializer.Serialize(response));
        }
    }
}