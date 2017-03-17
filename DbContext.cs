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
            var isList = false;
            var type = typeof(T);

            if (type.IsArray)
            {
                isList = true;
                type = type.GetElementType();
            }
            else if (type.IsGenericType())
            {
                //对应IEnumerable<T>
                isList = true;
                type = type.GetGenericArguments()[0];
            }

            var sql = DbContextServices.SqlGenerater.Insert(type, tableName);
            if (isList)
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

        public int Update<T>(T entity, string tableName = null, string[] columns = null, string primaryKeyName = null) where T : class
        {
            var type = typeof(T);
            if (type.IsArray)
                type = type.GetElementType();

            var sql = DbContextServices.SqlGenerater.Update(type, tableName, columns, primaryKeyName);
            return DbConnection.Execute(sql, entity, _transaction, CommandTimeout);
        }

        public int Update<T>(T entity, params Expression<Func<T, object>>[] columns) where T : class
        {
            var eTabInfo = DbContextServices.EntityMapper.GetEntityTableInfo(typeof(T));

            //TODO: 此处对EF Core有依赖，日后去掉依赖
            var cPis = columns.Select(Microsoft.EntityFrameworkCore.Internal.ExpressionExtensions.GetPropertyAccess);
            var colNames = cPis.Select(p => eTabInfo.Columns.First(p1 => p1.Property.Name == p.Name).ColumnName).ToArray();
            return Update(entity, null, colNames);
        }

        public int Delete<T>(T entity, string tableName = null, string primaryKeyName = null) where T : class
        {
            var type = typeof(T);
            if (type.IsArray)
                type = type.GetElementType();

            var sql = DbContextServices.SqlGenerater.Delete(type, tableName, primaryKeyName);
            return DbConnection.Execute(sql, entity, _transaction, CommandTimeout);
        }

        public int Save<T>(T entity, string tableName = null, string[] columns = null, string primaryKeyName = null) where T : class
        {
            var updCount = Update(entity, tableName, columns, primaryKeyName);
            if (updCount < 1)
                return Insert(entity, tableName);
            return updCount;
        }

        public T FetchByPrimaryKey<T>(T entity, string tableName = null, string primaryKeyName = null) where T : class
        {
            var sql = DbContextServices.SqlGenerater.GetByPrimaryKey(typeof(T), tableName, primaryKeyName);
            return FirstOrDefault<T>(sql, entity);
        }

        public List<T> FetchAll<T>(string tableName = null) where T : class
        {
            var sql = DbContextServices.SqlGenerater.GetAll(typeof(T), tableName);
            return Fetch<T>(sql);
        }

        #endregion

        #region Query/Execute

        public IEnumerable<T> Query<T>(string sql, params object[] args)
        {
            return DbConnection.Query<T>(sql, new Sql(sql, args), _transaction, false, CommandTimeout);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, params object[] args)
        {
            return DbConnection.Query(sql, map, Sql.ConvertToDapperParam(args), _transaction, false, "Id", CommandTimeout);
        }

        public IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string splitOn, string sql, params object[] args)
        {
            return DbConnection.Query(sql, map, Sql.ConvertToDapperParam(args), _transaction, false, splitOn, CommandTimeout);
        }

        public T FirstOrDefault<T>(string sql, params object[] args)
        {
            return DbConnection.QueryFirstOrDefault<T>(sql, Sql.ConvertToDapperParam(args), _transaction, CommandTimeout);
        }

        public List<T> Fetch<T>(string sql, params object[] args)
        {
            return (List<T>)DbConnection.Query<T>(sql, Sql.ConvertToDapperParam(args), _transaction, true, CommandTimeout);
        }

        public List<TReturn> Fetch<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, params object[] args)
        {
            return (List<TReturn>)DbConnection.Query(sql, map, Sql.ConvertToDapperParam(args), _transaction, true, "Id", CommandTimeout);
        }

        public int Execute(string sql, params object[] args)
        {
            return DbConnection.Execute(sql, Sql.ConvertToDapperParam(args), _transaction, CommandTimeout);
        }

        public T ExecuteScalar<T>(string sql, params object[] args)
        {
            return DbConnection.ExecuteScalar<T>(sql, Sql.ConvertToDapperParam(args), _transaction, CommandTimeout);
        }

        public Paged<T> Paged<T>(int page, int itemsPerPage, string sql, params object[] args) where T : new()
        {
            PartedSql partedSql;
            if (!PagingUtil.SplitSql(sql, out partedSql))
                throw new Exception("Unable to parse SQL statement for paged query");

            var pageSql = DbContextServices.SqlAdapter.PagingBuild(ref partedSql, args, (page - 1) * itemsPerPage, itemsPerPage);
            return Paged<T>(page, itemsPerPage, partedSql.CountSql, args, pageSql, args);
        }

        /// <summary>
        ///     Retrieves a page of records	and the total number of available records
        /// </summary>
        /// <typeparam name="T">The Type representing a row in the result set</typeparam>
        /// <param name="page">The 1 based page number to retrieve</param>
        /// <param name="itemsPerPage">The number of records per page</param>
        /// <param name="sqlCount">The SQL to retrieve the total number of records</param>
        /// <param name="countArgs">Arguments to any embedded parameters in the sqlCount statement</param>
        /// <param name="sqlPage">The SQL To retrieve a single page of results</param>
        /// <param name="pageArgs">Arguments to any embedded parameters in the sqlPage statement</param>
        /// <returns>A Page of results</returns>
        /// <remarks>
        ///     This method allows separate SQL statements to be explicitly provided for the two parts of the page query.
        ///     The page and itemsPerPage parameters are not used directly and are used simply to populate the returned Page
        ///     object.
        /// </remarks>
        public Paged<T> Paged<T>(int page, int itemsPerPage, string sqlCount, object[] countArgs, string sqlPage, object[] pageArgs) where T : new()
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
                TotalItems = ExecuteScalar<long>(sqlCount, countArgs),
                TotalPages = (int)Math.Ceiling((double)totalCount / itemsPerPage),
                Items = Fetch<T>(sqlPage, pageArgs)
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

        #endregion
    }
}
