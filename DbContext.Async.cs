// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Mondol.DapperPoco
{
    public partial class DbContext
    {
        #region Poco

        public Task<int> InsertAsync<T>(T entity, string tableName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Insert(entity, tableName), cancellationToken);
        }

        public Task<int> UpdateAsync<T>(T entity, Expression<Func<T, object>> columns, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Update(entity, columns, tableName, primaryKeyName), cancellationToken);
        }

        public Task<int> UpdateAsync<T>(T entity, ICollection<string> columns = null, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Update(entity, columns, tableName, primaryKeyName), cancellationToken);
        }

        public Task<int> UpdateAsync<T>(IEnumerable<T> entities, Expression<Func<T, object>> columns = null, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Update(entities, columns, tableName, primaryKeyName), cancellationToken);
        }

        public Task<int> DeleteAsync<T>(T entity, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Delete(entity, tableName, primaryKeyName), cancellationToken);
        }

        public Task<int> DeleteByColumnsAsync<T>(T entity, Expression<Func<T, object>> columns, string tableName = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            return Task.Run(() => DeleteByColumns(entity, columns, tableName), cancellationToken);
        }

        public Task<int> SaveAsync<T>(T entity, string[] columns = null, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Save(entity, columns, tableName, primaryKeyName), cancellationToken);
        }

        public Task<int> SaveAsync<T>(T entity, Expression<Func<T, object>> columns, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Save(entity, columns, tableName, primaryKeyName), cancellationToken);
        }

        public Task<List<T>> FetchAllAsync<T>(string tableName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => FetchAll<T>(tableName), cancellationToken);
        }

        public Task<List<T>> FetchByPropertyAsync<T>(T entity, Expression<Func<T, object>> properties, string tableName = null, CancellationToken cancellationToken = default(CancellationToken))
            where T : class
        {
            return Task.Run(() => FetchByProperty(entity, properties, tableName), cancellationToken);
        }

        public Task<T> ExecuteScalarAsync<T>(string sql, object sqlArgs = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => ExecuteScalar<T>(sql, sqlArgs), cancellationToken);
        }

        #endregion

        #region Query/Execute

        public Task<T> FirstOrDefaultAsync<T>(string sql, object sqlArgs = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => FirstOrDefault<T>(sql, sqlArgs), cancellationToken);
        }

        public Task<List<T>> FetchAsync<T>(string sql, object sqlArgs = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Fetch<T>(sql, sqlArgs), cancellationToken);
        }

        public Task<List<TReturn>> FetchAsync<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, object sqlArgs = null, string splitOn = "Id", CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Fetch(map, sql, sqlArgs, splitOn), cancellationToken);
        }

        public Task<int> ExecuteAsync(string sql, object sqlArgs = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => Execute(sql, sqlArgs), cancellationToken);
        }

        public Task<Paged<T>> PagedAsync<T>(int page, int itemsPerPage, string pageSql, object pageSqlArgs = null, string countSql = null, object countSqlArgs = null, CancellationToken cancellationToken = default(CancellationToken)) where T : new()
        {
            return Task.Run(() => Paged<T>(page, itemsPerPage, pageSql, pageSqlArgs, countSql, countSqlArgs), cancellationToken);
        }

        public Task<Paged<TReturn>> PagedAsync<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, int page, int itemsPerPage, string pageSql, object pageSqlArgs = null,
                                                              string countSql = null, object countSqlArgs = null, string splitOn = "Id", CancellationToken cancellationToken = default(CancellationToken)) where TReturn : new()
        {
            return Task.Run(() => Paged(map, page, itemsPerPage, pageSql, pageSqlArgs, countSql, countSqlArgs, splitOn), cancellationToken);
        }

        #endregion
    }
}
