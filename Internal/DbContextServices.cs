// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using Mondol.DapperPoco.Adapters;

namespace Mondol.DapperPoco.Internal
{
    public class DbContextServices: IDbContextServices
    {
        public string ConnectionString { get; set; }
        public ISqlAdapter SqlAdapter { get; set; }
        public IEntityMapper EntityMapper { get; set; }
        public ISqlGenerater SqlGenerater { get; set; }
    }
}
