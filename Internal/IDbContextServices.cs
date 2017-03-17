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
