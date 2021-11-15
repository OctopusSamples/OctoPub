using System.Collections.Generic;

namespace audit_service.Services
{
    public interface ITenantExtractor
    {
        string GetTenant(IEnumerable<string> acceptHeader);
    }
}