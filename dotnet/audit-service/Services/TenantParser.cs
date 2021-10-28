using System.Linq;
using Microsoft.AspNetCore.Http;

namespace audit_service.Services
{
    public class TenantParser : ITenantParser
    {
        private readonly IHttpContextAccessor httpContextAccessor;

        public TenantParser(IHttpContextAccessor httpContextAccessor)
        {
            this.httpContextAccessor = httpContextAccessor;
        }

        public string GetTenant()
        {
            return httpContextAccessor.HttpContext.Request.Headers
                // Get the accept headers
                .Where(h => h.Key.ToLower() == "accept")
                // Get the accept header values
                .SelectMany(h => h.Value)
                // Split the values on a semicolon
                .SelectMany(v => v.Split(";"))
                // trim the results and make them lowercase
                .Select(v => v.Trim().ToLower())
                // find any header value segments that indicate the feature branch or global tenant
                .Where(v => v.StartsWith("tenant=") || v.StartsWith("audit_tenant="))
                // split those values on the equals
                .Select(v => v.Split("="))
                // validate that the results have 2 elements
                .Where(v => v.Length == 2)
                // get the second element
                .Select(v => v[1].Trim())
                // if nothing was found, we assume we are the main tenant
                .FirstOrDefault() ?? "main";
        }
    }
}