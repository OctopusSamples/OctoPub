using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Audit.Service.Lambda;
using Audit.Service.Repositories.InMemory;
using PostSharp.Patterns.Contracts;

namespace Audit.Service.Services.InMemory
{
    public class AuditCreateService
    {
        private readonly Db _context;

        public AuditCreateService([Required] Db context)
        {
            _context = context;
        }

        public Task<Models.Audit> CreateAsync([Required] Models.Audit resource, [Required] RequestWrapper wrapper)
        {
            resource.Id = FindUniqueId();
            resource.Tenant = wrapper.Tenant;
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