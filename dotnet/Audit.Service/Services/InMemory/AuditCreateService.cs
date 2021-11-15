using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audit.Service.Repositories.InMemory;

namespace Audit.Service.Services.InMemory
{
    public class AuditCreateService
    {
        private readonly Db _context;
        private readonly ITenantParser _tenantParser;

        public AuditCreateService(Db context, ITenantParser tenantParser)
        {
            _context = context;
            _tenantParser = tenantParser;
        }

        public Task<Models.Audit> CreateAsync(Models.Audit resource, CancellationToken cancellationToken)
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