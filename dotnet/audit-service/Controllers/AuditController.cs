using audit_service.Models;
using JsonApiDotNetCore.Configuration;
using JsonApiDotNetCore.Controllers;
using JsonApiDotNetCore.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;

namespace audit_service.Controllers
{
    public class AuditController : JsonApiController<Audit, string>
    {
        public AuditController(
            IJsonApiOptions options,
            ILoggerFactory loggerFactory,
            IGetAllService<Audit, string> getAll = null,
            IGetByIdService<Audit, string> getById = null,
            IGetSecondaryService<Audit, string> getSecondary = null,
            IGetRelationshipService<Audit, string> getRelationship = null,
            ICreateService<Audit, string> create = null,
            IAddToRelationshipService<Audit, string> addToRelationship = null,
            IUpdateService<Audit, string> update = null,
            ISetRelationshipService<Audit, string> setRelationship = null,
            IDeleteService<Audit, string> delete = null,
            IRemoveFromRelationshipService<Audit, string> removeFromRelationship = null)
            : base(options, loggerFactory, getAll, getById, getSecondary, getRelationship, create, addToRelationship,
                update, setRelationship, delete, removeFromRelationship)
        {
        }
    }
}