using System.Linq;
using Audit.Service.Lambda;
using Audit.Service.Repositories.InMemory;

namespace Audit.Service.Services.InMemory
{
    public class AuditCreateService
    {
        private readonly Db _context;

        public AuditCreateService(Db context)
        {
            _context = context;
        }

        public Models.Audit CreateAsync(Models.Audit resource, RequestWrapper wrapper)
        {
            resource.Id = FindUniqueId();
            resource.Tenant = wrapper.Tenant;
            _context.Audits.Add(resource);
            _context.SaveChanges();
            return resource;
        }

        private int FindUniqueId()
        {
            var id = _context.Audits.Count();
            while (_context.Audits.Find(id) != null) ++id;

            return id;
        }
    }
}