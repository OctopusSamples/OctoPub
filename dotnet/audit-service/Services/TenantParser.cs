using System.Collections.Generic;
using System.Linq;

namespace audit_service.Services
{
    public class TenantParser : ITenantParser
    {
        public string GetTenant(IEnumerable<string> acceptHeader)
        {
            return acceptHeader
                .SelectMany(v => v.Split(";"))
                // trim the results and make them lowercase
                .Select(v => v.Trim().ToLower())
                // find any header value segments that indicate the tenant
                .Where(v => v.StartsWith("tenant="))
                // split those values on the equals
                .Select(v => v.Split("="))
                // validate that the results have 2 elements
                .Where(v => v.Length == 2)
                // get the second element
                .Select(v => v[1].Trim())
                // if nothing was found, we assume we are the default tenant
                .FirstOrDefault() ?? Constants.DefaultTenant;
        }
    }
}