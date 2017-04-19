// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using Mondol.DapperPoco.Adapters;

namespace Mondol.DapperPoco.Internal
{
    public interface IDbContextServices
    {
        string ConnectionString { get; }
        ISqlAdapter SqlAdapter { get; }
        IEntityMapper EntityMapper { get; }
        ISqlGenerater SqlGenerater { get; }
    }
}
