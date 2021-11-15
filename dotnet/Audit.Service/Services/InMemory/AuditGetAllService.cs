using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audit.Service.Repositories.InMemory;

namespace Audit.Service.Services.InMemory
{
    public class AuditGetAllService
    {
        private readonly Db _context;
        private readonly ITenantParser _tenantParser;

        public AuditGetAllService(Db context, ITenantParser tenantParser)
        {
            _context = context;
            _tenantParser = tenantParser;
        }

        public Task<IReadOnlyCollection<Models.Audit>> GetAsync(CancellationToken cancellationToken)
        {
            var tenant = _tenantParser.GetTenant();
            IReadOnlyCollection<Models.Audit> list = _context.Audits
                .Where(a => a.Tenant == Constants.DefaultTenant || a.Tenant == tenant)
                .ToList();
            return Task.FromResult(list);
        }
    }
}