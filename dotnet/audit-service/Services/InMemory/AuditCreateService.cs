using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;

namespace audit_service.Services.InMemory
{
    public class AuditCreateService : ICreateService<Audit, string>
    {
        private const string AUDIT_URN_PREFIX = "urn:audits:local_development";
        private readonly Db context;
        private readonly ITenantParser tenantParser;

        public AuditCreateService(Db context, ITenantParser tenantParser)
        {
            this.context = context;
            this.tenantParser = tenantParser;
        }

        public Task<Audit> CreateAsync(Audit resource, CancellationToken cancellationToken)
        {
            resource.Id = AUDIT_URN_PREFIX + ":" + FindUniqueId();
            resource.Tenant = tenantParser.GetTenant();
            context.Audits.Add(resource);
            return Task.FromResult(resource);
        }

        private int FindUniqueId()
        {
            var id = context.Audits.Count();
            while (context.Audits.Find(AUDIT_URN_PREFIX + ":" + id) != null) ++id;

            return id;
        }
    }
}