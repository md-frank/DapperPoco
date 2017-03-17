using System;
using System.Collections.Generic;
using System.Linq;
using Mondol.DapperPoco.Adapters;
using Mondol.DapperPoco.Metadata;

namespace Mondol.DapperPoco.Internal
{
    internal class DefaultSqlGenerater : ISqlGenerater
    {
        private readonly IEntityMapper _mapper;
        private readonly ISqlAdapter _sqlAdapter;
        private readonly ConcurrentCache<string, string> _sqlsCache = new ConcurrentCache<string, string>();

        public DefaultSqlGenerater(IEntityMapper mapper, ISqlAdapter sqlAdapter)
        {
            _mapper = mapper;
            _sqlAdapter = sqlAdapter;
        }

        public string Insert(Type type, string tableName)
        {
            var key = $"{nameof(Insert)}_{type.FullName}_{tableName}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);
                var insertCols = tableInfo.Columns.Where(p => !p.IsAutoIncrement).ToList();
                var iColsName = string.Join(", ", insertCols.Select(p => _sqlAdapter.EscapeSqlIdentifier(p.ColumnName)));
                var iColsParams = string.Join(", ", insertCols.Select(p => paramPrefix + p.Property.Name));
                return $"insert into {tableName} ({iColsName}) values ({iColsParams})";
            });
        }

        public string Update(Type type, string tableName, string[] columns, string primaryKeyName)
        {
            var keyCols = columns?.Length > 0 ? string.Join(",", columns) : "All";
            var key = $"{nameof(Update)}_{type.FullName}_{tableName}_{keyCols}_{primaryKeyName}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var pkInfo = GetPrimaryKey(tableInfo, primaryKeyName);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                List<EntityColumnInfo> updCols;
                if (columns?.Length > 0)
                {
                    updCols = columns.Select(p =>
                    {
                        var c = tableInfo.Columns.FirstOrDefault(p1 => p1.ColumnName == p);
                        if (null == c)
                            throw new ArgumentException($"指定的列 {p} 不存在");
                        return c;
                    }).ToList();
                }
                else
                {
                    updCols = tableInfo.Columns.Where(p => !p.IsAutoIncrement && p != pkInfo).ToList();
                }

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);
                var pkName = _sqlAdapter.EscapeSqlIdentifier(pkInfo.ColumnName);
                var setCols = string.Join(", ", updCols.Select(p =>
                {
                    var colName = _sqlAdapter.EscapeSqlIdentifier(p.ColumnName);
                    var colParam = paramPrefix + p.Property.Name;
                    return $"{colName} = {colParam}";
                }));
                return $"update {tableName} set {setCols} where {pkName} = {paramPrefix}{pkInfo.Property.Name}";
            });
        }

        public string Delete(Type type, string tableName, string primaryKeyName)
        {
            var key = $"{nameof(Delete)}_{type.FullName}_{tableName}_{primaryKeyName}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var pkInfo = GetPrimaryKey(tableInfo, primaryKeyName);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);
                var pkName = _sqlAdapter.EscapeSqlIdentifier(pkInfo.ColumnName);
                return $"delete from {tableName} where {pkName} = {paramPrefix}{pkInfo.Property.Name}";
            });
        }

        public string GetAll(Type type, string tableName)
        {
            var key = $"{nameof(GetAll)}_{type.FullName}_{tableName}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);
                var qCols = string.Join(", ", tableInfo.Columns.Select(p => _sqlAdapter.EscapeSqlIdentifier(p.ColumnName)));
                return $"select {qCols} from {tableName}";
            });
        }

        public string GetByPrimaryKey(Type type, string tableName, string primaryKeyName)
        {
            var key = $"{nameof(GetByPrimaryKey)}_{type.FullName}_{tableName}_{primaryKeyName}";
            return _sqlsCache.GetOrAdd(key, () =>
            {
                var tableInfo = _mapper.GetEntityTableInfo(type);
                var pkInfo = GetPrimaryKey(tableInfo, primaryKeyName);
                var paramPrefix = _sqlAdapter.GetParameterPrefix();

                if (string.IsNullOrEmpty(tableName))
                    tableName = tableInfo.TableName;
                tableName = _sqlAdapter.EscapeTableName(tableName);
                var pkName = _sqlAdapter.EscapeSqlIdentifier(pkInfo.ColumnName);
                var qCols = string.Join(", ", tableInfo.Columns.Select(p => _sqlAdapter.EscapeSqlIdentifier(p.ColumnName)));
                return $"select {qCols} from {tableName} where {pkName} = {paramPrefix}{pkInfo.Property.Name}";
            });
        }

        private EntityColumnInfo GetPrimaryKey(EntityTableInfo tableInfo, string primaryKey = null)
        {
            EntityColumnInfo pkInfo;

            if (string.IsNullOrEmpty(primaryKey))
            {
                //优先选取自动增长的主键
                pkInfo = tableInfo.Columns.FirstOrDefault(p => p.IsPrimaryKey && p.IsAutoIncrement);
                if (pkInfo == null)
                    pkInfo = tableInfo.Columns.FirstOrDefault(p => p.IsPrimaryKey);
            }
            else
            {
                pkInfo = tableInfo.Columns.FirstOrDefault(p => p.ColumnName == primaryKey);
            }
            if (pkInfo == null)
                throw new InvalidProgramException($"获取实体 {tableInfo.TableName} 的主键失败");
            return pkInfo;
        }
    }
}
