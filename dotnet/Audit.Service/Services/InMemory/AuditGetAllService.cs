﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Audit.Service.Lambda;
using Audit.Service.Repositories;

namespace Audit.Service.Services.InMemory
{
    public class AuditGetAllService
    {
        private readonly Db _context;

        public AuditGetAllService(Db context)
        {
            _context = context;
        }

        public IReadOnlyCollection<Models.Audit> GetAsync(RequestWrapper wrapper)
        {
            var tenant = wrapper.Tenant;
            IReadOnlyCollection<Models.Audit> list = _context.Audits
                .Where(a => a.Tenant == Constants.DefaultTenant || a.Tenant == tenant)
                .ToList();
            return list;
        }
    }
}