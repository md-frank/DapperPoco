using System.Reflection;

namespace Mondol.DapperPoco.Metadata
{
    public class FluentEntityColumnInfo
    {
        public PropertyInfo Property { get; set; }
        public string ColumnName { get; set; }
        public bool? IsPrimaryKey { get; set; }
        public bool? IsAutoIncrement { get; set; }
        public bool? IsIgnore { get; set; }
    }
}
