using System.Threading;
using System.Threading.Tasks;
using Audit.Service.Repositories.InMemory;
using Audit.Service.Services.Lambda;

namespace Audit.Service.Services.InMemory
{
    public class AuditGetByIdService
    {
        private readonly Db _context;
        private readonly IRequestWrapperAccessor _requestWrapper;

        public AuditGetByIdService(Db context, IRequestWrapperAccessor requestWrapper)
        {
            _context = context;
            _requestWrapper = requestWrapper;
        }

        public Task<Models.Audit> GetAsync(int id, CancellationToken cancellationToken)
        {
            var audit = _context.Audits.Find(id);
            return audit.Tenant == Constants.DefaultTenant || audit.Tenant == _requestWrapper.RequestWrapper.Tenant
                ? Task.FromResult(audit)
                : Task.FromResult<Models.Audit>(null);
        }
    }
}