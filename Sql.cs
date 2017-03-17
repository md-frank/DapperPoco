
using System;
using System.Collections.Generic;
using Dapper;

namespace Mondol.DapperPoco
{
    /// <summary>
    /// SQL语句包装类
    /// </summary>
    public class Sql
    {
        public Sql(string sql, object parameters)
        {
            Statement = sql;
            Parameters = parameters;
        }

        public Sql(string sql, params object[] args)
        {
            Statement = sql;
            Parameters = ConvertToDapperParam(args);
        }

        /// <summary>
        /// SQL语句
        /// </summary>
        public string Statement { get; }

        /// <summary>
        /// 语句参数
        /// </summary>
        public object Parameters { get; }

        /// <summary>
        /// 将params object[] args参数转换为Dapper的SQL参数，支持形式如下：
        /// (sql, val1, val2) //基本类型
        /// (sql, model)
        /// (sql, models)        
        /// (sql, model, model2);
        /// (sql, new { p0 = groupType, p1 = OptionState.Display }, new { p0 = groupType, p1 = OptionState.Display });
        /// </summary>
        public static object ConvertToDapperParam(object[] args)
        {
            if (args?.Length == 0)
                return null;

            if (IsDbSupportedType(args[0]))
            {
                var param = new KeyValuePair<string, object>[args.Length];
                for (var i = 0; i < args.Length; ++i)
                {
                    param[i] = new KeyValuePair<string, object>("p" + i, args[i]);
                }
                return param;
            }
            else
            {
                return args.Length == 1 ? args[0] : args;
            }
        }

        private static bool IsDbSupportedType(object obj)
        {
            return obj is string ||
                   obj is Enum ||
                   obj is byte[] ||
                   obj.GetType().IsValueType();
        }
    }
}