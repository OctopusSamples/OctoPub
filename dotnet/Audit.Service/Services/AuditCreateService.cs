using Audit.Service.Lambda;
using Audit.Service.Repositories;

namespace Audit.Service.Services
{
    public class AuditCreateService
    {
        private readonly Db _context;

        public AuditCreateService(Db context)
        {
            _context = context;
        }

        public Models.Audit Create(Models.Audit resource)
        {
            resource.Id = null;
            _context.Audits.Add(resource);
            _context.SaveChanges();
            return resource;
        }
    }
}