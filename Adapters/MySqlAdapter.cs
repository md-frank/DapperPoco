using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using Mondol.DapperPoco.Utils;

namespace Mondol.DapperPoco.Adapters
{
    public class MySqlAdapter : SqlAdapter
    {
        private readonly DbProviderFactory _dbProviderFac;

        public MySqlAdapter(DbProviderFactory dbProviderFac)
        {
            _dbProviderFac = dbProviderFac;
        }

        public override DbProviderFactory GetFactory()
        {
            return _dbProviderFac;
        }

        public override string EscapeSqlIdentifier(string sqlIdentifier)
        {
            return string.Format("`{0}`", sqlIdentifier);
        }        
    }
}
