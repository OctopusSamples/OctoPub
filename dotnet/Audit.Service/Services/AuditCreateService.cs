﻿using Audit.Service.Repositories;

namespace Audit.Service.Services
{
    /// <summary>
    /// The service used to create new audit records.
    /// </summary>
    public class AuditCreateService
    {
        private readonly Db context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditCreateService"/> class.
        /// </summary>
        /// <param name="context">The entity framework context.</param>
        public AuditCreateService(Db context)
        {
            this.context = context;
        }

        /// <summary>
        /// Create a new audit record.
        /// </summary>
        /// <param name="resource">The new audit record.</param>
        /// <returns>The audit record once it has been saved.</returns>
        public Models.Audit Create(Models.Audit resource)
        {
            resource.Id = null;
            context.Audits.Add(resource);
            context.SaveChanges();
            return resource;
        }
    }
}