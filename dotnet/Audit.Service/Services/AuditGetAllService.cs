using System.Collections.Generic;
using System.Linq;
using Audit.Service.Lambda;
using Audit.Service.Repositories;

namespace Audit.Service.Services
{
    public class AuditGetAllService
    {
        private readonly Db _context;

        public AuditGetAllService(Db context)
        {
            _context = context;
        }

        public IReadOnlyCollection<Models.Audit> Get(IList<string> partitions)
        {
            IReadOnlyCollection<Models.Audit> list = _context.Audits
                .Where(a => partitions.Contains(a.DataPartition))
                .ToList();
            return list;
        }
    }
}