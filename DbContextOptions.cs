// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System;
using Mondol.DapperPoco.Adapters;

namespace Mondol.DapperPoco
{
    public class DbContextOptions
    {
        public DbContextOptions(string connectionString, ISqlAdapter sqlAdapter)
        {
            ConnectionString = connectionString;
            SqlAdapter = sqlAdapter;
        }

        public string ConnectionString { get; }
        public ISqlAdapter SqlAdapter { get; }

        internal static IServiceProvider Services { get; set; }
    }
}
