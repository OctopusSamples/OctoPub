using System.Collections.Generic;

namespace audit_service.Services
{
    public interface ITenantParser
    {
        string GetTenant(IEnumerable<string> acceptHeader);
    }
}