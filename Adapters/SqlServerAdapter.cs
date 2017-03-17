using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using Mondol.DapperPoco.Utils;

namespace Mondol.DapperPoco.Adapters
{
    public class SqlServerAdapter : SqlAdapter
    {
        private readonly DbProviderFactory _dbProviderFac;
        private static readonly Regex SimpleRegexOrderBy = new Regex(@"\bORDER\s+BY\s+", RegexOptions.RightToLeft | RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Singleline | RegexOptions.Compiled);

        public SqlServerAdapter(DbProviderFactory dbProviderFac)
        {
            _dbProviderFac = dbProviderFac;
        }

        public override DbProviderFactory GetFactory()
        {
            return _dbProviderFac;
        }

        public override string PagingBuild(ref PartedSql partedSql, object[] args, long skip, long take)
        {
            // when the query does not contain an "order by", it is very slow
            if (SimpleRegexOrderBy.IsMatch(partedSql.SelectRemovedSql))
            {
                partedSql.SelectRemovedSql = PagingUtil.ReplaceOrderBy(partedSql.SelectRemovedSql, "");
            }
            if (PagingUtil.ContainsDistinct(partedSql.SelectRemovedSql))
            {
                partedSql.SelectRemovedSql = "poco_inner.* FROM (SELECT " + partedSql.SelectRemovedSql + ") poco_inner";
            }
            var sqlPage = $"SELECT * FROM (SELECT ROW_NUMBER() OVER " +
                          $"({partedSql.OrderBySql ?? "ORDER BY (SELECT NULL)"}) poco_rn, " +
                          $"{partedSql.SelectRemovedSql}) peta_paged WHERE " +
                          $"poco_rn > {skip} AND poco_rn <= {skip + take}";
            return sqlPage;
        }

        public override long Insert(IDbConnection dbConn, string sql, object param, IDbTransaction transaction)
        {
            sql += "; select SCOPE_IDENTITY() Id";
            return dbConn.ExecuteScalar<long>(sql, param, transaction);
        }
    }
}
