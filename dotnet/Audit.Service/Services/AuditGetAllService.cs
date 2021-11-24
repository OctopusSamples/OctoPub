using System.Collections.Generic;
using System.Linq;
using Audit.Service.Lambda;
using Audit.Service.Repositories;
using Microsoft.EntityFrameworkCore;
using RestSQL.SqlKata;
using SqlKata.Compilers;

namespace Audit.Service.Services
{
    public class AuditGetAllService
    {
        private readonly Db _context;

        public AuditGetAllService(Db context)
        {
            _context = context;
        }

        public IReadOnlyCollection<Models.Audit> Get(IList<string> partitions, string filter)
        {
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var compiler = new MySqlCompiler();
                var testExpressionBuilder = new SqlKataBuilder();
                var q = testExpressionBuilder.Build(filter);
                var sql = compiler.Compile(q.From("audits"));
                return _context.Audits.FromSqlRaw(sql.Sql, sql.Bindings.ToArray()).ToList();
            }

            IReadOnlyCollection<Models.Audit> list = _context.Audits
                .Where(a => partitions.Contains(a.DataPartition))
                .ToList();
            return list;
        }
    }
}