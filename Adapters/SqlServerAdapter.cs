// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using Dapper;
using Mondol.DapperPoco.Utils;

namespace Mondol.DapperPoco.Adapters
{
    /// <summary>
    /// Õ®”√SqlServer  ≈‰∆˜
    /// </summary>
    public class SqlServerAdapter : SqlAdapter
    {
        private readonly DbProviderFactory _dbProviderFac;

        public SqlServerAdapter(DbProviderFactory dbProviderFac)
        {
            _dbProviderFac = dbProviderFac;
        }

        public override DbProviderFactory GetFactory()
        {
            return _dbProviderFac;
        }

        public override string PagingBuild(ref PartedSql partedSql, object args, long skip, long take)
        {
            if (string.IsNullOrEmpty(partedSql.OrderBy))
                throw new InvalidOperationException("miss order by");

            var hasDistinct = partedSql.Select.IndexOf("DISTINCT", StringComparison.OrdinalIgnoreCase) == 0;
            var select = "SELECT";
            if (hasDistinct)
            {
                partedSql.Select = partedSql.Select.Substring("DISTINCT".Length);
                select = "SELECT DISTINCT";
            }
            if (skip <= 0)
            {
                var sbSql = StringBuilderCache.Allocate().AppendFormat("{0} TOP {1} {2}", select, take, partedSql.Select)
                            .Append(" FROM ").Append(partedSql.Body).Append(" order by ").Append(partedSql.OrderBy);
                return StringBuilderCache.ReturnAndFree(sbSql);
            }
            else
            {
                var sbSql = StringBuilderCache.Allocate()
                            .AppendFormat("SELECT * FROM (SELECT {0}, ROW_NUMBER() OVER " +
                                          "(order by {1}) As RowNum FROM {2}) AS RowConstrainedResult " +
                                          "WHERE RowNum > {3} AND RowNum <= {4}",
                                          partedSql.Select, partedSql.OrderBy, partedSql.Body, skip, skip + take);

                return StringBuilderCache.ReturnAndFree(sbSql);
            }
        }

        public override long Insert(IDbConnection dbConn, string sql, object param, IDbTransaction transaction)
        {
            sql += "; select SCOPE_IDENTITY() Id";
            return dbConn.ExecuteScalar<long>(sql, param, transaction);
        }
    }
}
