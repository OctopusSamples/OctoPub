using System.Collections.Generic;
using System.Linq;

namespace Audit.Service.Services.Lambda
{
    /// <summary>
    /// Extract tenant information from a AWS Lambda HTTP proxy request.
    /// </summary>
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