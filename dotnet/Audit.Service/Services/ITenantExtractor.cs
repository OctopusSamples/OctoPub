using System.Collections.Generic;

namespace Audit.Service.Services
{
    public interface ITenantExtractor
    {
        string GetTenant(IEnumerable<string> acceptHeader);
    }
}