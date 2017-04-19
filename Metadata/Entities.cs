// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System;
using System.Collections.Generic;

namespace Mondol.DapperPoco.Metadata
{
    internal class Entities
    {
        private readonly Dictionary<RuntimeTypeHandle, FluentEntityTableInfo> _entityTypes;

        internal Entities(Dictionary<RuntimeTypeHandle, FluentEntityTableInfo> entityTypes)
        {
            _entityTypes = entityTypes;
        }

        public bool TryGetEntityTableInfo(Type type, out FluentEntityTableInfo eti)
        {
            return _entityTypes.TryGetValue(type.TypeHandle, out eti);
        }

        public FluentEntityTableInfo GetEntityTableInfo(Type type)
        {
            FluentEntityTableInfo eType;
            if (!_entityTypes.TryGetValue(type.TypeHandle, out eType))
                throw new KeyNotFoundException($"ÀàÐÍ {type.FullName} Î´×¢²á");

            return eType;
        }
    }
}
