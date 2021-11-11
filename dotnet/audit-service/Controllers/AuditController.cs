using audit_service.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace audit_service.Controllers
{
    public class AuditController : JsonApiController<Audit, int>
    {
        public AuditController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IGetAllService<Audit, int> getAll = null,
            IGetByIdService<Audit, int> getById = null,
            IGetSecondaryService<Audit, int> getSecondary = null,
            IGetRelationshipService<Audit, int> getRelationship = null,
            ICreateService<Audit, int> create = null,
            IAddToRelationshipService<Audit, int> addToRelationship = null,
            IUpdateService<Audit, int> update = null,
            ISetRelationshipService<Audit, int> setRelationship = null,
            IDeleteService<Audit, int> delete = null,
            IRemoveFromRelationshipService<Audit, int> removeFromRelationship = null)
            : base(options, loggerFactory, getAll, getById, getSecondary, getRelationship, create, addToRelationship,
                update, setRelationship, delete, removeFromRelationship)
        {
        }
    }
}