using System;
using Mondol.DapperPoco.Metadata;

namespace Mondol.DapperPoco.Internal
{
    public interface IEntityMapper
    {
        EntityTableInfo GetEntityTableInfo(Type entityType);
    }
}
