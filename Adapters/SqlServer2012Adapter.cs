// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-04-16
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
    /// SqlServer 2012  ≈‰∆˜
    /// </summary>
    public class SqlServer2012Adapter : SqlServerAdapter
    {
        public SqlServer2012Adapter(DbProviderFactory dbProviderFac)
            : base(dbProviderFac)
        {
        }

        public override string PagingBuild(ref PartedSql partedSql, object args, long skip, long take)
        {
            if (string.IsNullOrEmpty(partedSql.OrderBy))
                throw new ArgumentException("miss order by");

            return $"{partedSql.Raw} OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY";
        }
    }
}
