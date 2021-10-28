using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using JsonApiDotNetCore.Services;

namespace audit_service.Services.InMemory
{
    public class AuditGetAllService : IGetAllService<Audit, string>
    {
        private readonly Db context;

        public AuditGetAllService(Db context)
        {
            this.context = context;
        }

        public Task<IReadOnlyCollection<Audit>> GetAsync(CancellationToken cancellationToken)
        {
            IReadOnlyCollection<Audit> list = context.Audits.ToList();
            return Task.FromResult(list);
        }
    }
}