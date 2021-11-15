using System.Collections.Generic;
using System.Linq;

namespace Audit.Service.Services
{
    /// <summary>
    /// The common implementation that extracts the tenant information from a collection of accept headers.
    /// </summary>
    public class TenantExtractor : ITenantExtractor
    {
        public string GetTenant(IEnumerable<string> acceptHeader)
        {
            var versions = (acceptHeader ?? Enumerable.Empty<string>())
                .SelectMany(v => v.Split(";"))
                // trim the results and make them lowercase
                .Select(v => v.Trim().ToLower())
                // find any header value segments that indicate the tenant
                .Where(v => v.StartsWith("tenant="))
                // split those values on the equals
                .Select(v => v.Split("="))
                // validate that the results have 2 elements
                .Where(v => v.Length == 2)
                .ToList();

             var appVersion =  versions
                 // find any header value segments that indicate the tenant
                 .Where(v => v[0].Trim().StartsWith(Constants.AcceptVersionInfo + "="))
                // get the second element
                .Select(v => v[1].Trim())
                // if nothing was found, we assume we are the default tenant
                .FirstOrDefault();

             var tenantVersion = versions
                 // find any header value segments that indicate the tenant
                 .Where(v => v[0].Trim().StartsWith(Constants.AcceptTenantInfo + "="))
                 // get the second element
                 .Select(v => v[1].Trim())
                 // if nothing was found, we assume we are the default tenant
                 .FirstOrDefault();

             return tenantVersion ?? appVersion ?? Constants.DefaultTenant;
        }
    }
}