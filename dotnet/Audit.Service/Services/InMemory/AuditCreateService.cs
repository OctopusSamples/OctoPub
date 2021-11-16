using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Audit.Service.Repositories.InMemory;
using Audit.Service.Services.Lambda;

namespace Audit.Service.Services.InMemory
{
    public class AuditCreateService
    {
        private readonly Db _context;
        private readonly IRequestWrapperAccessor _requestWrapper;

        public AuditCreateService(Db context, IRequestWrapperAccessor requestWrapper)
        {
            _context = context;
            _requestWrapper = requestWrapper;
        }

        public Task<Models.Audit> CreateAsync(Models.Audit resource, CancellationToken cancellationToken)
        {
            resource.Id = FindUniqueId();
            resource.Tenant = _requestWrapper.RequestWrapper.Tenant;
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