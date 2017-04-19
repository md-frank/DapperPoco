// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System;
using Mondol.DapperPoco.Metadata;

namespace Mondol.DapperPoco.Internal
{
    public interface IEntityMapper
    {
        EntityTableInfo GetEntityTableInfo(Type entityType);
    }
}
