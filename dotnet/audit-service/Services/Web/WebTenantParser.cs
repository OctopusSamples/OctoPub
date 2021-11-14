using System.Linq;
using Microsoft.AspNetCore.Http;

namespace audit_service.Services.Web
{
    public class WebWebTenantParser : IWebTenantParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantParser _tenantParser;

        public WebWebTenantParser(IHttpContextAccessor httpContextAccessor, ITenantParser tenantParser)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantParser = tenantParser;
        }

        public string GetTenant()
        {
            return _tenantParser.GetTenant(
                _httpContextAccessor.HttpContext.Request.Headers
                    // Get the accept headers
                    .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                    // Get the accept header values
                    .SelectMany(h => h.Value));
        }
    }
}