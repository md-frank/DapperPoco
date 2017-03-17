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

        public Task<int> UpdateAsync<T>(T entity, string[] columns = null, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Update(entity, columns, tableName, primaryKeyName), cancellationToken);
        }

        public Task<int> UpdateAsync<T>(T entity, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken), params Expression<Func<T, object>>[] columns) where T : class
        {
            return Task.Run(() => Update(entity, tableName, primaryKeyName, columns), cancellationToken);
        }

        public Task<int> DeleteAsync<T>(T entity, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Delete(entity, tableName, primaryKeyName), cancellationToken);
        }

        public Task<int> SaveAsync<T>(T entity, string[] columns = null, string tableName = null, string primaryKeyName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => Save(entity, columns, tableName, primaryKeyName), cancellationToken);
        }

        public Task<List<T>> FetchAllAsync<T>(string tableName = null, CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return Task.Run(() => FetchAll<T>(tableName), cancellationToken);
        }

        public Task<T> ExecuteScalarAsync<T>(string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] args)
        {
            return Task.Run(() => ExecuteScalar<T>(sql, args), cancellationToken);
        }

        #endregion

        #region Query/Execute

        public Task<IEnumerable<TReturn>> QueryAsync<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] args)
        {
            return Task.Run(() => Query(map, sql, args), cancellationToken);
        }

        public Task<T> FirstOrDefaultAsync<T>(string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] args)
        {
            return Task.Run(() => FirstOrDefault<T>(sql, args), cancellationToken);
        }

        public Task<List<T>> FetchAsync<T>(string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] args)
        {
            return Task.Run(() => Fetch<T>(sql, args), cancellationToken);
        }

        public Task<List<TReturn>> FetchAsync<TFirst, TSecond, TReturn>(Func<TFirst, TSecond, TReturn> map, string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] args)
        {
            return Task.Run(() => Fetch(map, sql, args), cancellationToken);
        }

        public Task<int> ExecuteAsync(string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] args)
        {
            return Task.Run(() => Execute(sql, args), cancellationToken);
        }

        public Task<Paged<T>> PagedAsync<T>(int page, int itemsPerPage, string sql, CancellationToken cancellationToken = default(CancellationToken), params object[] args) where T : new()
        {
            return Task.Run(() => Paged<T>(page, itemsPerPage, sql, args), cancellationToken);
        }

        public Task<Paged<T>> PagedAsync<T>(int page, int itemsPerPage, string sqlCount, object[] countArgs, string sqlPage, object[] pageArgs,
            CancellationToken cancellationToken = default(CancellationToken)) where T : new()
        {
            return Task.Run(() => Paged<T>(page, itemsPerPage, sqlCount, countArgs, sqlPage, pageArgs), cancellationToken);
        }

        #endregion
    }
}
