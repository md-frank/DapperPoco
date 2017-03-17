using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using Dapper;
using Mondol.DapperPoco.Utils;

namespace Mondol.DapperPoco.Adapters
{
    public abstract class SqlAdapter : ISqlAdapter
    {
        public abstract DbProviderFactory GetFactory();
        public bool IsSupportGuid => false;
        public virtual string EscapeTableName(string tableName)
        {
            return tableName.IndexOf('.') >= 0 ? tableName : EscapeSqlIdentifier(tableName);
        }

        public virtual string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("[{0}]", sqlIdentifier);
        }

        public virtual string PagingBuild(ref PartedSql partedSql, object[] args, long skip, long take)
        {
            var pageSql = $"{partedSql.RawSql} LIMIT @{take} OFFSET @{skip}";
            return pageSql;
        }

        public virtual object MapParameterValue(object value)
        {
            if (value is bool)
                return ((bool)value) ? 1 : 0;

            return value;
        }

        public virtual string GetParameterPrefix()
        {
            return "@";
        }

        public virtual long Insert(IDbConnection dbConn, string sql, object param, IDbTransaction transaction)
        {
            sql += "; SELECT @@IDENTITY AS Id";
            return dbConn.ExecuteScalar<long>(sql, param, transaction);
        }
    }
}
