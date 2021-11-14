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
        private readonly Db context;
        private readonly IWebTenantParser _webTenantParser;

        public AuditGetAllService(Db context, IWebTenantParser webTenantParser)
        {
            this.context = context;
            this._webTenantParser = webTenantParser;
        }

        public Task<IReadOnlyCollection<Audit>> GetAsync(CancellationToken cancellationToken)
        {
            var tenant = _webTenantParser.GetTenant();
            IReadOnlyCollection<Audit> list = context.Audits
                .Where(a => a.Tenant == Constants.DefaultTenant || a.Tenant == tenant)
                .ToList();
            return Task.FromResult(list);
        }
    }
}