using System.Linq;
using Amazon.Lambda.APIGatewayEvents;

namespace audit_service.Services.Lambda
{
    public class LambdaTenantParser : ILambdaTenantParser
    {
        private readonly ITenantParser _tenantParser;

        public LambdaTenantParser(ITenantParser tenantParser)
        {
            _tenantParser = tenantParser;
        }

        public string GetTenant(APIGatewayProxyRequest request)
        {
            return _tenantParser.GetTenant(
                request.Headers
                    .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                    .Select(h => h.Value));
        }
    }
}