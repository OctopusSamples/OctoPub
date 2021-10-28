using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using JsonApiDotNetCore.Services;

namespace audit_service.Services.InMemory
{
    public class AuditGetAllService : IGetAllService<Audit, string>
    {
        private readonly Db context;
        private readonly ITenantParser tenantParser;

        public AuditGetAllService(Db context, ITenantParser tenantParser)
        {
            this.context = context;
            this.tenantParser = tenantParser;
        }

        public Task<IReadOnlyCollection<Audit>> GetAsync(CancellationToken cancellationToken)
        {
            var tenant = tenantParser.GetTenant();
            IReadOnlyCollection<Audit> list = context.Audits
                .Where(a => a.Tenant == "main" || a.Tenant == tenant)
                .ToList();
            return Task.FromResult(list);
        }
    }
}