using System.Linq;
using Microsoft.AspNetCore.Http;

namespace audit_service.Services.Web
{
    public class WebWebTenantParser : ITenantParser
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITenantExtractor _tenantExtractor;

        public WebWebTenantParser(IHttpContextAccessor httpContextAccessor, ITenantExtractor tenantExtractor)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenantExtractor = tenantExtractor;
        }

        public string GetTenant()
        {
            return _tenantExtractor.GetTenant(
                _httpContextAccessor.HttpContext.Request.Headers
                    // Get the accept headers
                    .Where(h => h.Key.ToLower() == Constants.AcceptHeader)
                    // Get the accept header values
                    .SelectMany(h => h.Value));
        }
    }
}