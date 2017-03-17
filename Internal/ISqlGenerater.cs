using System;

namespace Mondol.DapperPoco.Internal
{
    /// <summary>
    /// SQL生成器
    /// </summary>
    public interface ISqlGenerater
    {
        string Insert(Type type, string tableName = null);
        string Delete(Type type, string tableName = null, string primaryKeyName = null);
        string Update(Type type, string tableName = null, string[] columns = null, string primaryKeyName = null);
        string GetAll(Type type, string tableName = null);
        string GetByColumn(Type type, string columnName, string tableName = null);
    }
}
