using Audit.Service.Application.Lambda;
using Audit.Service.Infrastructure.Repositories;

namespace Audit.Service.Infrastructure.Services
{
    /// <summary>
    /// The service used to get individual audit records.
    /// </summary>
    public class AuditGetByIdService
    {
        private readonly Db context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditGetByIdService"/> class.
        /// </summary>
        /// <param name="context">The entity framework context.</param>
        public AuditGetByIdService(Db context)
        {
            this.context = context;
        }

        /// <summary>
        /// Gets a single audit record by ID.
        /// </summary>
        /// <param name="id">The ID of the audit record.</param>
        /// <param name="wrapper">The details of the request.</param>
        /// <returns>The matching audit record, or null.</returns>
        public Domain.Entities.Audit? Get(int id, RequestWrapper wrapper)
        {
            var audit = context.Audits.Find(id);
            return audit != null && (audit.DataPartition == Constants.DefaultPartition ||
                                     audit.DataPartition == wrapper.DataPartition)
                ? audit
                : null;
        }
    }
}