using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using audit_service.Services.Web;
using JsonApiDotNetCore.Services;

namespace audit_service.Services.InMemory
{
    public class AuditGetAllService : IGetAllService<Audit, int>
    {
        private readonly Db _context;
        private readonly ITenantParser _tenantParser;

        public AuditGetAllService(Db context, ITenantParser tenantParser)
        {
            _context = context;
            _tenantParser = tenantParser;
        }

        public Task<IReadOnlyCollection<Audit>> GetAsync(CancellationToken cancellationToken)
        {
            var tenant = _tenantParser.GetTenant();
            IReadOnlyCollection<Audit> list = _context.Audits
                .Where(a => a.Tenant == Constants.DefaultTenant || a.Tenant == tenant)
                .ToList();
            return Task.FromResult(list);
        }
    }
}