﻿using System.Collections.Generic;
using System.Linq;
using Audit.Service.Repositories;
using Microsoft.EntityFrameworkCore;
using RestSQL.SqlKata;
using SqlKata.Compilers;

namespace Audit.Service.Services
{
    public class AuditGetAllService
    {
        private readonly Db context;

        public AuditGetAllService(Db context)
        {
            this.context = context;
        }

        public IReadOnlyCollection<Models.Audit> Get(IList<string> partitions, string filter)
        {
            var filteredPartitions = GetFilteredPartitions(partitions);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                var compiler = new MySqlCompiler();
                var testExpressionBuilder = new SqlKataBuilder();
                var q = testExpressionBuilder.Build(filter);
                var sql = compiler.Compile(q.From("audits"));
                return context.Audits.FromSqlRaw(sql.Sql, sql.Bindings.ToArray())
                    .Where(a => filteredPartitions.Contains(a.DataPartition)).ToList();
            }

            IReadOnlyCollection<Models.Audit> list = context.Audits
                .Where(a => filteredPartitions.Contains(a.DataPartition))
                .ToList();
            return list;
        }

        private IList<string> GetFilteredPartitions(IList<string> partitions)
        {
            if (partitions == null) return new List<string>();

            return partitions
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Select(p => p.Trim())
                .ToList();
        }
    }
}