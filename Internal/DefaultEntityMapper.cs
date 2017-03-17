using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Mondol.DapperPoco.Metadata;

namespace Mondol.DapperPoco.Internal
{
    internal class DefaultEntityMapper : IEntityMapper
    {
        private readonly Entities _entity;
        private readonly ConcurrentCache<RuntimeTypeHandle, EntityTableInfo> _caches = new ConcurrentCache<RuntimeTypeHandle, EntityTableInfo>();

        public DefaultEntityMapper(Entities entity)
        {
            _entity = entity;
        }

        public EntityTableInfo GetEntityTableInfo(Type entityType)
        {
            return _caches.GetOrAdd(entityType.TypeHandle, () =>
            {
                FluentEntityTableInfo fluentEti;
                _entity.TryGetEntityTableInfo(entityType, out fluentEti);

                var tableName = InflectTableName(entityType, fluentEti);
                var columns = InflectColumns(entityType, fluentEti, tableName);

                return new EntityTableInfo
                {
                    TableName = tableName,
                    Columns = columns
                };
            });
        }

        private string InflectTableName(Type entityType, FluentEntityTableInfo fluentEti)
        {
            return !string.IsNullOrEmpty(fluentEti?.TableName) ? fluentEti.TableName : entityType.Name;
        }

        private EntityColumnInfo[] InflectColumns(Type entityType, FluentEntityTableInfo fluentEti, string tableName)
        {
            var props = entityType.GetProperties();
            var lstRetn = new List<EntityColumnInfo>(props.Length);
            foreach (var prop in props)
            {
                var fluentEci = fluentEti?.Columns?.FirstOrDefault(p => p.Property.Name == prop.Name);
                if (fluentEci?.IsIgnore == true)
                    continue;

                var colName = !string.IsNullOrEmpty(fluentEci?.ColumnName) ? fluentEci.ColumnName : prop.Name;
                var isPrimaryKey = false;
                var isAutoIncrement = false;
                if (fluentEci?.IsPrimaryKey.HasValue ?? false)
                    isPrimaryKey = fluentEci.IsPrimaryKey.Value;
                else if (IsPrimaryKeyName(tableName, colName))
                    isPrimaryKey = true;

                if (fluentEci?.IsAutoIncrement.HasValue ?? false)
                {
                    isAutoIncrement = fluentEci.IsAutoIncrement.Value;
                }
                else
                {
                    var t = prop.PropertyType;
                    if (t.GetTypeInfo().IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
                        t = t.GetGenericArguments()[0];

                    if (isPrimaryKey && IsPrimaryKeyName(tableName, colName) &&
                        (t == typeof(long) || t == typeof(ulong) ||
                        t == typeof(int) || t == typeof(uint) ||
                        t == typeof(short) || t == typeof(ushort)))
                        isAutoIncrement = true;
                }

                lstRetn.Add(new EntityColumnInfo
                {
                    IsPrimaryKey = isPrimaryKey,
                    IsAutoIncrement = isAutoIncrement,
                    ColumnName = colName,
                    Property = prop
                });
            }

            return lstRetn.ToArray();
        }

        private bool IsPrimaryKeyName(string tableName, string colName)
        {
            return colName.Equals("Id", StringComparison.OrdinalIgnoreCase) ||
                   colName.Equals(tableName + "Id", StringComparison.OrdinalIgnoreCase) ||
                   colName == tableName + "_Id";
        }
    }
}
