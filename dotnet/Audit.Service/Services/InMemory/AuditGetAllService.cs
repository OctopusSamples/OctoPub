using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audit.Service.Lambda;
using Audit.Service.Repositories;

namespace Audit.Service.Services.InMemory
{
    public class AuditGetAllService
    {
        private readonly Db _context;

        public AuditGetAllService(Db context)
        {
            _context = context;
        }

        public IReadOnlyCollection<Models.Audit> Get(RequestWrapper wrapper)
        {
            var tenant = wrapper.DataPartition;
            IReadOnlyCollection<Models.Audit> list = _context.Audits
                .Where(a => a.DataPartition == Constants.DefaultPartition || a.DataPartition == tenant)
                .ToList();
            return list;
        }
    }
}