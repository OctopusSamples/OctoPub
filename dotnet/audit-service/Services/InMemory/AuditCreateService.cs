using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using audit_service.Services.Web;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Http;

namespace audit_service.Services.InMemory
{
    public class AuditCreateService : ICreateService<Audit, int>
    {
        private readonly Db _context;
        private readonly ITenantParser _tenantParser;

        public AuditCreateService(Db context, ITenantParser tenantParser)
        {
            _context = context;
            _tenantParser = tenantParser;
        }

        public Task<Audit> CreateAsync(Audit resource, CancellationToken cancellationToken)
        {
            resource.Id = FindUniqueId();
            resource.Tenant = _tenantParser.GetTenant();
            _context.Audits.Add(resource);
            _context.SaveChanges();
            return Task.FromResult(resource);
        }

        private int FindUniqueId()
        {
            var id = _context.Audits.Count();
            while (_context.Audits.Find(id) != null) ++id;

            return id;
        }
    }
}