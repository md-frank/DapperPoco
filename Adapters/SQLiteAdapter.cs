using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using Mondol.DapperPoco.Utils;

namespace Mondol.DapperPoco.Adapters
{
    public class SQLiteAdapter : SqlAdapter
    {
        private readonly DbProviderFactory _dbProviderFac;

        public SQLiteAdapter(DbProviderFactory dbProviderFac)
        {
            _dbProviderFac = dbProviderFac;
        }

        public override DbProviderFactory GetFactory()
        {
            return _dbProviderFac;
        }

        public override long Insert(IDbConnection dbConn, string sql, object param, IDbTransaction transaction)
        {
            sql += "; SELECT last_insert_rowid();";
            return dbConn.ExecuteScalar<long>(sql, param, transaction);
        }
    }
}
