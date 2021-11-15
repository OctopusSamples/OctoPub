using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.Extensions.Logging;

namespace Audit.Service.Controllers
{
    public class AuditController : JsonApiController<Models.Audit, int>
    {
        public AuditController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IGetAllService<Models.Audit, int> getAll = null,
            IGetByIdService<Models.Audit, int> getById = null,
            IGetSecondaryService<Models.Audit, int> getSecondary = null,
            IGetRelationshipService<Models.Audit, int> getRelationship = null,
            ICreateService<Models.Audit, int> create = null,
            IAddToRelationshipService<Models.Audit, int> addToRelationship = null,
            IUpdateService<Models.Audit, int> update = null,
            ISetRelationshipService<Models.Audit, int> setRelationship = null,
            IDeleteService<Models.Audit, int> delete = null,
            IRemoveFromRelationshipService<Models.Audit, int> removeFromRelationship = null)
            : base(options, loggerFactory, getAll, getById, getSecondary, getRelationship, create, addToRelationship,
                update, setRelationship, delete, removeFromRelationship)
        {
        }
    }
}