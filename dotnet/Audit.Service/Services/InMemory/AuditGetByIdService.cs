using System.Threading.Tasks;
using Audit.Service.Lambda;
using Audit.Service.Repositories;

namespace Audit.Service.Services.InMemory
{
    public class AuditGetByIdService
    {
        private readonly Db _context;

        public AuditGetByIdService(Db context)
        {
            _context = context;
        }

        public Models.Audit Get(int id, RequestWrapper wrapper)
        {
            var audit = _context.Audits.Find(id);
            return audit.DataPartition == Constants.DefaultPartition || audit.DataPartition == wrapper.DataPartition
                ? audit
                : null;
        }
    }
}