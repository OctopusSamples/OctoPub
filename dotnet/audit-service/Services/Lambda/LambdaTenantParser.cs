using System.Collections.Generic;
using System.Linq;
using Amazon.Lambda.APIGatewayEvents;

namespace audit_service.Services.Lambda
{
    public class LambdaTenantParser : ITenantParser
    {
        private readonly ITenantExtractor _tenantExtractor;
        private readonly IApiGatewayProxyRequestAccessor _apiGatewayProxyRequestAccessor;

        public LambdaTenantParser(ITenantExtractor tenantExtractor, IApiGatewayProxyRequestAccessor apiGatewayProxyRequestAccessor)
        {
            _tenantExtractor = tenantExtractor;
            _apiGatewayProxyRequestAccessor = apiGatewayProxyRequestAccessor;
        }

        public string GetTenant()
        {
            return _tenantExtractor.GetTenant(
                (_apiGatewayProxyRequestAccessor.ApiGatewayProxyRequest.Headers ?? new Dictionary<string,string>())
                    .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                    .Select(h => h.Value));
        }
    }
}