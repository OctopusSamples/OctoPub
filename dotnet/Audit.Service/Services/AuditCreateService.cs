using Audit.Service.Repositories;

namespace Audit.Service.Services
{
    public class AuditCreateService
    {
        private readonly Db context;

        public AuditCreateService(Db context)
        {
            this.context = context;
        }

        public Models.Audit Create(Models.Audit resource)
        {
            resource.Id = null;
            context.Audits.Add(resource);
            context.SaveChanges();
            return resource;
        }
    }
}