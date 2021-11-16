using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audit.Service.Repositories.InMemory;
using Audit.Service.Services.Lambda;

namespace Audit.Service.Services.InMemory
{
    public class AuditGetAllService
    {
        private readonly Db _context;
        private readonly IRequestWrapperAccessor _requestWrapper;

        public AuditGetAllService(Db context, IRequestWrapperAccessor requestWrapper)
        {
            _context = context;
            _requestWrapper = requestWrapper;
        }

        public Task<IReadOnlyCollection<Models.Audit>> GetAsync(CancellationToken cancellationToken)
        {
            var tenant = _requestWrapper.RequestWrapper.Tenant;
            IReadOnlyCollection<Models.Audit> list = _context.Audits
                .Where(a => a.Tenant == Constants.DefaultTenant || a.Tenant == tenant)
                .ToList();
            return Task.FromResult(list);
        }
    }
}