using System.Threading;
using System.Threading.Tasks;
using Audit.Service.Repositories.InMemory;

namespace Audit.Service.Services.InMemory
{
    public class AuditGetByIdService
    {
        private readonly Db _context;
        private readonly ITenantParser _tenantParser;

        public AuditGetByIdService(Db context, ITenantParser tenantParser)
        {
            _context = context;
            _tenantParser = tenantParser;
        }

        public Task<Models.Audit> GetAsync(int id, CancellationToken cancellationToken)
        {
            var audit = _context.Audits.Find(id);
            return audit.Tenant == Constants.DefaultTenant || audit.Tenant == _tenantParser.GetTenant()
                ? Task.FromResult(audit)
                : Task.FromResult<Models.Audit>(null);
        }
    }
}