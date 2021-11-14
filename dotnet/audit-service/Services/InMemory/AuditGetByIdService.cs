using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using audit_service.Services.Web;
using JsonApiDotNetCore.Services;

namespace audit_service.Services.InMemory
{
    public class AuditGetByIdService : IGetByIdService<Audit, int>
    {
        private readonly Db context;
        private readonly IWebTenantParser _webTenantParser;

        public AuditGetByIdService(Db context, IWebTenantParser webTenantParser)
        {
            this.context = context;
            this._webTenantParser = webTenantParser;
        }

        public Task<Audit> GetAsync(int id, CancellationToken cancellationToken)
        {
            var audit = context.Audits.Find(id);
            return audit.Tenant == Constants.DefaultTenant || audit.Tenant == _webTenantParser.GetTenant()
                ? Task.FromResult(audit)
                : Task.FromResult<Audit>(null);
        }
    }
}