using Audit.Service.Lambda;
using Audit.Service.Repositories;

namespace Audit.Service.Services
{
    public class AuditGetByIdService
    {
        private readonly Db context;

        public AuditGetByIdService(Db context)
        {
            this.context = context;
        }

        public Models.Audit? Get(int id, RequestWrapper wrapper)
        {
            var audit = context.Audits.Find(id);
            return audit != null && (audit.DataPartition == Constants.DefaultPartition ||
                                     audit.DataPartition == wrapper.DataPartition)
                ? audit
                : null;
        }
    }
}