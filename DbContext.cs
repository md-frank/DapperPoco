// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Dapper;
using Mondol.DapperPoco.Internal;
using Mondol.DapperPoco.Utils;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Mondol.DapperPoco.Metadata;

namespace Mondol.DapperPoco
{
    public abstract partial class DbContext : IDisposable, IInfrastructure<IDbContextServices>
    {
        private IDbContextServices _dbContextServices;

        private IDbConnection _dbConn;
        private IDbTransaction _transaction;

        private bool _disposed;
        private bool _initializing;

        public void Dispose()
        {
            _disposed = true;
            _transaction?.Dispose();
            _dbConn?.Dispose();
        }

        #region Properties

        public int? CommandTimeout { get; set; }

        #endregion

        #region Poco

        public int Insert<T>(T entity, string tableName = null) where T : class
        {
            bool isEnumerable;
            var type = GetEnumerableElementType(typeof(T), out isEnumerable);

            var sql = DbContextServices.SqlGenerater.Insert(type, tableName);
            if (isEnumerable)
            {
                return DbConnection.Execute(sql, entity, _transaction);
            }
            else
            {
                var id = DbContextServices.SqlAdapter.Insert(DbConnection, sql, entity, _transaction);
                var eTabInfo = DbContextServices.EntityMapper.GetEntityTableInfo(type);
                var eci = eTabInfo.Columns.FirstOrDefault(p => p.IsAutoIncrement);
                eci?.SetValue(entity, id);

                return 1;
            }
        }

        public int Update<T>(T entity, ICollection<string> columns = null, string tableName = null, string primaryKeyName = null) where T : class
        {
            bool isEnumerable;
            var type = GetEnumerableElementType(typeof(T), out isEnumerable);

            var sql = DbContextServices.SqlGenerater.Update(type, tableName, columns, primaryKeyName);
            return DbConnection.Execute(sql, entity, _transaction, CommandTimeout);
        }

        public int Update<T>(T entity, Expression<Func<T, object>> columns, string tableName = null, string primaryKeyName = null) where T : class
        {
            var eTabInfo = DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));

            ICollection<string> colNames = null;
            if (columns != null)
            {
                var cPis = ExpressionUtil.GetPropertyAccessList(columns);
                colNames = cPis.Select(p => eTabInfo.Columns.First(p1 => p1.Property.Name == p.Name).ColumnName).ToList();
            }
            return Update(entity, colNames, tableName, primaryKeyName);
        }

        public int Update<T>(IEnumerable<T> entities, Expression<Func<T, object>> columns, string tableName = null, string primaryKeyName = null) where T : class
        {
            var eTabInfo = DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));

            ICollection<string> colNames = null;
            if (columns != null)
            {
                var cPis = ExpressionUtil.GetPropertyAccessList(columns);
                colNames = cPis.Select(p => eTabInfo.Columns.First(p1 => p1.Property.Name == p.Name).ColumnName).ToList();
            }
            return Update(entities, colNames, tableName, primaryKeyName);
        }

        public int Delete<T>(T entity, string tableName = null, string primaryKeyName = null) where T : class
        {
            bool isEnumerable;
            var type = GetEnumerableElementType(typeof(T), out isEnumerable);

            var sql = DbContextServices.SqlGenerater.DeleteByColumns(type, tableName, new[] { primaryKeyName });
            return DbConnection.Execute(sql, entity, _transaction, CommandTimeout);
        }

        public int DeleteByColumns<T>(T entity, Expression<Func<T, object>> columns, string tableName = null) where T : class
        {
            bool isEnumerable;
            var type = GetEnumerableElementType(typeof(T), out isEnumerable);
            var eTabInfo = DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));
            var cPis = ExpressionUtil.GetPropertyAccessList(columns);
            var colNames = cPis.Select(p => eTabInfo.Columns.First(p1 => p1.Property.Name == p.Name).ColumnName).ToList();

            var sql = DbContextServices.SqlGenerater.DeleteByColumns(type, tableName, colNames);
            return DbConnection.Execute(sql, entity, _transaction, CommandTimeout);
        }

        public int Save<T>(T entity, string[] columns = null, string tableName = null, string primaryKeyName = null) where T : class
        {
            var updCount = Update(entity, columns, tableName, primaryKeyName);
            if (updCount < 1)
                return Insert(entity, tableName);
            return updCount;
        }

        public int Save<T>(T entity, Expression<Func<T, object>> columns, string tableName = null, string primaryKeyName = null) where T : class
        {
            var updCount = Update(entity, columns, tableName, primaryKeyName);
            if (updCount < 1)
                return Insert(entity, tableName);
            return updCount;
        }

        public List<T> FetchAll<T>(string tableName = null) where T : class
        {
            var sql = DbContextServices.SqlGenerater.GetAll(typeof(T), tableName);
            return Fetch<T>(sql);
        }

        public List<T> FetchByProperty<T>(T entity, Expression<Func<T, object>> properties, string tableName = null) where T : class
        {
            var eTabInfo = DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));

            var cPis = ExpressionUtil.GetPropertyAccessList(properties);
            var colNames = cPis.Select(p => eTabInfo.Columns.First(p1 => p1.Property.Name == p.Name).ColumnName).ToList();

            var sql = DbContextServices.SqlGenerater.GetByColumns(typeof(T), colNames, tableName);
            return Fetch<T>(sql, entity);
        }

        public IEnumerable<T> QueryByProperty<T>(T entity, Expression<Func<T, object>> properties, string tableName = null) where T : class
        {
            var eTabInfo = DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));

            var cPis = ExpressionUtil.GetPropertyAccessList(properties);
            var colNames = cPis.Select(p => eTabInfo.Columns.First(p1 => p1.Property.Name == p.Name).ColumnName).ToList();

            var sql = DbContextServices.SqlGenerater.GetByColumns(typeof(T), colNames, tableName);
            return Query<T>(sql, entity);
        }

        #endregion

        #region Query/Execute

        public IEnumerable<T> Query<T>(string sql, object sqlArgs = null)
        {
            return DbConnection.Query<T>(sql, Sql.ConvertToDapperParam(sqlArgs), _transaction, false, CommandTimeout);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            return DbConnection.Query(sql, map, Sql.ConvertToDapperParam(sqlArgs), _transaction, false, splitOn, CommandTimeout);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(Func<TFirst, TSecond, TThird, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            return DbConnection.Query(sql, map, Sql.ConvertToDapperParam(sqlArgs), _transaction, false, splitOn, CommandTimeout);
        }

        public T FirstOrDefault<T>(string sql, object sqlArgs = null)
        {
            return DbConnection.QueryFirstOrDefault<T>(sql, Sql.ConvertToDapperParam(sqlArgs), _transaction, CommandTimeout);
        }

        public List<T> Fetch<T>(string sql, object sqlArgs = null)
        {
            return (List<T>)DbConnection.Query<T>(sql, Sql.ConvertToDapperParam(sqlArgs), _transaction, true, CommandTimeout);
        }

        public List<TReturn> Fetch<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            return (List<TReturn>)DbConnection.Query(sql, map, Sql.ConvertToDapperParam(sqlArgs), _transaction, true, splitOn, CommandTimeout);
        }

        public List<TReturn> Fetch<TFirst, TSecond, TThird, TReturn>(Func<TFirst, TSecond, TThird, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id")
        {
            return (List<TReturn>)DbConnection.Query(sql, map, Sql.ConvertToDapperParam(sqlArgs), _transaction, true, splitOn, CommandTimeout);
        }

        public int Execute(string sql, object sqlArgs)
        {
            return DbConnection.Execute(sql, Sql.ConvertToDapperParam(sqlArgs), _transaction, CommandTimeout);
        }

        public T ExecuteScalar<T>(string sql, object sqlArgs)
        {
            return DbConnection.ExecuteScalar<T>(sql, Sql.ConvertToDapperParam(sqlArgs), _transaction, CommandTimeout);
        }

        public Paged<T> Paged<T>(int page, int itemsPerPage, string pageSql, object pageSqlArgs = null, string countSql = null, object countSqlArgs = null) where T : new()
        {
            var partedSql = PagingUtil.SplitSql(pageSql);
            pageSql = DbContextServices.SqlAdapter.PagingBuild(ref partedSql, pageSqlArgs, (page - 1) * itemsPerPage, itemsPerPage);
            if (string.IsNullOrEmpty(countSql))
            {
                countSql = PagingUtil.GetCountSql(partedSql);
                countSqlArgs = pageSqlArgs;
            }

            return PagedInternal(page, itemsPerPage, countSql, countSqlArgs, () =>
                Fetch<T>(pageSql, pageSqlArgs)
            );
        }

        public Paged<TReturn> Paged<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, int page, int itemsPerPage, string pageSql, object pageSqlArgs = null,
                                                              string countSql = null, object countSqlArgs = null, string splitOn = "Id") where TReturn : new()
        {
            var partedSql = PagingUtil.SplitSql(pageSql);
            pageSql = DbContextServices.SqlAdapter.PagingBuild(ref partedSql, pageSqlArgs, (page - 1) * itemsPerPage, itemsPerPage);
            if (string.IsNullOrEmpty(countSql))
            {
                countSql = PagingUtil.GetCountSql(partedSql);
                countSqlArgs = pageSqlArgs;
            }

            return PagedInternal(page, itemsPerPage, countSql, countSqlArgs, () =>
                Fetch(map, pageSql, pageSqlArgs, splitOn)
            );
        }

        private Paged<T> PagedInternal<T>(int page, int itemsPerPage, string sqlCount, object countArgs, Func<List<T>> itemsCb) where T : new()
        {
            if (page < 1)
                throw new ArgumentOutOfRangeException(nameof(page));
            if (itemsPerPage < 1 || itemsPerPage > 100)
                throw new ArgumentOutOfRangeException(nameof(itemsPerPage));

            var totalCount = ExecuteScalar<long>(sqlCount, countArgs);
            return new Paged<T>
            {
                CurrentPage = page,
                ItemsPerPage = itemsPerPage,
                TotalItems = totalCount,
                TotalPages = (int)Math.Ceiling((double)totalCount / itemsPerPage),
                Items = itemsCb()
            };
        }

        #endregion

        #region Transaction               

        public ITransaction GetTransaction(IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            return new Transaction(this, isolation);
        }

        public void BeginTransaction(IsolationLevel isolation = IsolationLevel.ReadCommitted)
        {
            if (_transaction != null)
                throw new InvalidOperationException("当前已有一个事务");

            _transaction = DbConnection.BeginTransaction(isolation);
        }

        public void CommitTransaction()
        {
            _transaction.Commit();
            _transaction = null;
        }

        public void RollbackTransaction()
        {
            _transaction.Rollback();
            _transaction = null;
        }

        #endregion

        IDbContextServices IInfrastructure<IDbContextServices>.Instance => DbContextServices;

        protected internal virtual void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
        }

        protected internal virtual void OnConfigured()
        {
        }

        protected internal virtual void OnEntitiesBuilding(EntitiesBuilder entityBuilder)
        {
        }

        #region Private                

        private IDbContextServices DbContextServices
        {
            get
            {
                if (_disposed)
                    throw new ObjectDisposedException(GetType().FullName);

                if (_dbContextServices == null)
                    InitServices();
                return _dbContextServices;
            }
        }

        private void InitServices()
        {
            if (_initializing)
                throw new InvalidDataException("当前正在初始化");

            try
            {
                _initializing = true;

                var optsBuilder = new DbContextOptionsBuilder();
                OnConfiguring(optsBuilder);
                var options = optsBuilder.Build();

                var entityMapper = EntityMapperFactory.Instance.GetEntityMapper(this);
                var sqlGenerater = new DefaultSqlGenerater(entityMapper, options.SqlAdapter);
                var dbCtxSvces = new DbContextServices
                {
                    ConnectionString = options.ConnectionString,
                    SqlAdapter = options.SqlAdapter,
                    EntityMapper = entityMapper,
                    SqlGenerater = sqlGenerater
                };
                _dbContextServices = dbCtxSvces;

                OnConfigured();
            }
            finally
            {
                _initializing = false;
            }
        }

        private IDbConnection DbConnection
        {
            get
            {
                if (_dbConn == null)
                {
                    var dbSvce = DbContextServices;
                    _dbConn = dbSvce.SqlAdapter.GetFactory().CreateConnection();
                    _dbConn.ConnectionString = dbSvce.ConnectionString;

                    /*
                     * 此处要返回一个打开的连接
                     * 1. 否则BeginTransaction会异常
                     * 2. Context内重用连接可提高性能
                     */
                    _dbConn.Open();
                }
                return _dbConn;
            }
        }

        private Type GetEnumerableElementType(Type type, out bool isEnumerable)
        {
            isEnumerable = false;
            if (type.IsArray)
            {
                isEnumerable = true;
                return type.GetElementType();
            }
            else if (type.IsGenericType())
            {
                //对应IEnumerable<T>
                isEnumerable = true;
                return type.GetGenericArguments()[0];
            }
            return type;
        }

        #endregion
    }
}
