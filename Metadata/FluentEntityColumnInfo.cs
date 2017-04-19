// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
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
