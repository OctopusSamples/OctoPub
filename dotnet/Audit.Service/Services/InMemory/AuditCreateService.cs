﻿using Audit.Service.Lambda;
using Audit.Service.Repositories;

namespace Audit.Service.Services.InMemory
{
    public class AuditCreateService
    {
        private readonly Db _context;

        public AuditCreateService(Db context)
        {
            _context = context;
        }

        public Models.Audit Create(Models.Audit resource, RequestWrapper wrapper)
        {
            resource.Id = null;
            resource.DataPartition = wrapper.DataPartition;
            _context.Audits.Add(resource);
            _context.SaveChanges();
            return resource;
        }
    }
}