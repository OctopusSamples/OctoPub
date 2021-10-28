using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using JsonApiDotNetCore.Services;

namespace audit_service.Services.InMemory
{
    public class AuditGetByIdService : IGetByIdService<Audit, string>
    {
        private readonly Db context;

        public AuditGetByIdService(Db context)
        {
            this.context = context;
        }

        public Task<Audit> GetAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(context.Audits.Find(id));
        }
    }
}