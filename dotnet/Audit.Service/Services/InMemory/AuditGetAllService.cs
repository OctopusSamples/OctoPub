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

        public IReadOnlyCollection<Models.Audit> Get(RequestWrapper wrapper)
        {
            var tenant = wrapper.Partition;
            IReadOnlyCollection<Models.Audit> list = _context.Audits
                .Where(a => a.Partition == Constants.DefaultPartition || a.Partition == tenant)
                .ToList();
            return list;
        }
    }
}