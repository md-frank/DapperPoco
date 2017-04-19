// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Mondol.DapperPoco
{
    public class SqlBuilder
    {
        private static readonly Regex _rexParam = new Regex(@"(?<=@p)\d+", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private readonly List<KeyValuePair<string, object[]>> _sqlClauses = new List<KeyValuePair<string, object[]>>();

        public SqlBuilder Append(string sql, params object[] args)
        {
            _sqlClauses.Add(new KeyValuePair<string, object[]>(sql, args));
            return this;
        }

        public SqlBuilder Like(string likeBefore, string likeStr)
        {
            Append(likeBefore + " like @p0", $"'{EscapeString(likeStr)}'");
            return this;
        }

        public Sql Build()
        {
            var sbSql = StringBuilderCache.Allocate();
            var lstParam = new List<KeyValuePair<string, object>>();
            var index = 0;
            foreach (var sqlClause in _sqlClauses)
            {
                var clauseId = index++;
                var sql = _rexParam.Replace(sqlClause.Key, m => clauseId + m.Value);
                sbSql.Append(sql + " ");
                for (var i = 0; i < sqlClause.Value.Length; ++i)
                {
                    var key = $"p{clauseId}{i}";
                    var val = sqlClause.Value[i];
                    lstParam.Add(new KeyValuePair<string, object>(key, val));
                }
            }

            return new Sql(StringBuilderCache.ReturnAndFree(sbSql), parameters: lstParam);
        }

        public static string EscapeString(string str)
        {
            return str?.Replace("'", "''");
        }
    }
}
