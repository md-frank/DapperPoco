// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System.Data;
using System.Data.Common;

namespace Mondol.DapperPoco.Adapters
{
    public interface ISqlAdapter
    {
        DbProviderFactory GetFactory();

        /// <summary>
        ///     Gets a flag for whether the DB has native support for GUID/UUID.
        /// </summary>
        bool IsSupportGuid { get; }

        /// <summary>
        ///     Escape a tablename into a suitable format for the associated database provider.
        /// </summary>
        /// <param name="tableName">
        ///     The name of the table (as specified by the client program, or as attributes on the associated
        ///     POCO class.
        /// </param>
        /// <returns>The escaped table name</returns>
        string EscapeTableName(string tableName);

        /// <summary>
        ///     Escape and arbitary SQL identifier into a format suitable for the associated database provider
        /// </summary>
        /// <param name="sqlIdentifier">The SQL identifier to be escaped</param>
        /// <returns>The escaped identifier</returns>
        string EscapeSqlIdentifier(string sqlIdentifier);

        /// <summary>
        ///     Returns the prefix used to delimit parameters in SQL query strings.
        /// </summary>
        /// <returns>The providers character for prefixing a query parameter.</returns>
        string GetParameterPrefix();

        /// <summary>
        ///     Builds an SQL query suitable for performing page based queries to the database
        /// </summary>
        /// <param name="partedSql">partedSql</param>
        /// <param name="sqlArgs">Arguments to any embedded parameters in the SQL query</param>
        /// <param name="skip">The number of rows that should be skipped by the query</param>
        /// <param name="take">The number of rows that should be retruend by the query</param>
        string PagingBuild(ref PartedSql partedSql, object sqlArgs, long skip, long take);

        long Insert(IDbConnection dbConn, string sql, object sqlArgs, IDbTransaction transaction = null);
    }
}
