// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
namespace Mondol.DapperPoco.Metadata
{
    public class FluentEntityTableInfo
    {
        public string TableName { get; set; }
        public FluentEntityColumnInfo[] Columns { get; set; }
    }
}
