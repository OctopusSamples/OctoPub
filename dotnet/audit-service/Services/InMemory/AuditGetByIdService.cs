using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using JsonApiDotNetCore.Services;

namespace audit_service.Services.InMemory
{
    public class AuditGetByIdService : IGetByIdService<Audit, int>
    {
        private readonly Db _context;
        private readonly ITenantParser _tenantParser;

        public AuditGetByIdService(Db context, ITenantParser tenantParser)
        {
            _context = context;
            _tenantParser = tenantParser;
        }

        public Task<Audit> GetAsync(int id, CancellationToken cancellationToken)
        {
            var audit = _context.Audits.Find(id);
            return audit.Tenant == Constants.DefaultTenant || audit.Tenant == _tenantParser.GetTenant()
                ? Task.FromResult(audit)
                : Task.FromResult<Audit>(null);
        }
    }
}