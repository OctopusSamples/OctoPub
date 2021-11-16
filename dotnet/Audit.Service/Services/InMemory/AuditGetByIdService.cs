using System.Threading.Tasks;
using Audit.Service.Lambda;
using Audit.Service.Repositories.InMemory;

namespace Audit.Service.Services.InMemory
{
    public class AuditGetByIdService
    {
        private readonly Db _context;

        public AuditGetByIdService(Db context)
        {
            _context = context;
        }

        public Task<Models.Audit> GetAsync(int id, RequestWrapper wrapper)
        {
            var audit = _context.Audits.Find(id);
            return audit.Tenant == Constants.DefaultTenant || audit.Tenant == wrapper.Tenant
                ? Task.FromResult(audit)
                : Task.FromResult<Models.Audit>(null);
        }
    }
}