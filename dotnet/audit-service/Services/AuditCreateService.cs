using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using audit_service.Models;
using audit_service.Repositories.InMemory;
using JsonApiDotNetCore.Services;

namespace audit_service.Services
{
    public class AuditCreateService : ICreateService<Audit, string>
    {
        private const string AUDIT_URN_PREFIX = "urn:audits:local_development";
        private readonly Db context;

        public AuditCreateService(Db context)
        {
            this.context = context;
        }

        public Task<Audit> CreateAsync(Audit resource, CancellationToken cancellationToken)
        {
            resource.Id = AUDIT_URN_PREFIX + ":" + FindUniqueId();
            context.Audits.Add(resource);
            return Task.FromResult(resource);
        }

        private int FindUniqueId()
        {
            var id = context.Audits.Count();
            while (context.Audits.Find(AUDIT_URN_PREFIX + ":" + id) != null) ++id;

            return id;
        }
    }
}