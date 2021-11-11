using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using JsonApiDotNetCore.Services;

namespace audit_service.Services.InMemory
{
    public class AuditGetByIdService : IGetByIdService<Audit, int>
    {
        private readonly Db context;
        private readonly ITenantParser tenantParser;

        public AuditGetByIdService(Db context, ITenantParser tenantParser)
        {
            this.context = context;
            this.tenantParser = tenantParser;
        }

        public Task<Audit> GetAsync(int id, CancellationToken cancellationToken)
        {
            var audit = context.Audits.Find(id);
            return audit.Tenant == "main" || audit.Tenant == tenantParser.GetTenant()
                ? Task.FromResult(audit)
                : Task.FromResult<Audit>(null);
        }
    }
}