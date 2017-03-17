using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Mondol.DapperPoco.Metadata;

namespace Mondol.DapperPoco
{
    public interface IEntityBuilder
    {
        FluentEntityTableInfo Build();
    }

    public class EntityBuilder<TEntity> : IEntityBuilder where TEntity : class
    {
        private string _tableName;
        private readonly Dictionary<string, FluentEntityColumnInfo> _properties = new Dictionary<string, FluentEntityColumnInfo>();

        public EntityBuilder<TEntity> TableName(string name)
        {
            _tableName = name;
            return this;
        }

        public EntityBuilder<TEntity> ColumnName(Expression<Func<TEntity, object>> propertyExpression, string name)
        {
            var prop = GetProperty(propertyExpression);
            GetFluentEntityColumnInfo(prop).ColumnName = name;

            return this;
        }

        public EntityBuilder<TEntity> Ignore(Expression<Func<TEntity, object>> propertyExpression)
        {
            var prop = GetProperty(propertyExpression);
            GetFluentEntityColumnInfo(prop).IsIgnore = true;

            return this;
        }

        public EntityBuilder<TEntity> PrimaryKey(Expression<Func<TEntity, object>> propertyExpression, bool? autoIncrement = null)
        {
            var prop = GetProperty(propertyExpression);
            var eci = GetFluentEntityColumnInfo(prop);
            eci.IsPrimaryKey = true;
            eci.IsAutoIncrement = autoIncrement;

            return this;
        }

        public FluentEntityTableInfo Build()
        {
            return new FluentEntityTableInfo()
            {
                TableName = _tableName,
                Columns = _properties.Values.ToArray()
            };
        }

        private FluentEntityColumnInfo GetFluentEntityColumnInfo(PropertyInfo prop)
        {
            FluentEntityColumnInfo eci;
            if (!_properties.TryGetValue(prop.Name, out eci))
            {
                eci = new FluentEntityColumnInfo() { Property = prop };
                _properties.Add(prop.Name, eci);
            }
            return eci;
        }

        private PropertyInfo GetProperty(Expression<Func<TEntity, object>> propertyExpression)
        {
            //TODO: 此处对EF Core有依赖，日后去掉依赖
            var pi = propertyExpression.GetPropertyAccess();
            return pi;
        }
    }
}
