// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System.Collections.Generic;

namespace Mondol.DapperPoco
{
    /// <summary>
    ///     Holds the results of a paged request.
    /// </summary>
    /// <typeparam name="T">The type of Poco in the returned result set</typeparam>
    public class Paged<T> where T : new()
    {
        /// <summary>
        /// 当前页码
        /// </summary>
        public int CurrentPage { get; set; }

        /// <summary>
        /// 总页数
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// 总记录数
        /// </summary>
        public long TotalItems { get; set; }

        /// <summary>
        /// 每页记录数
        /// </summary>
        public int ItemsPerPage { get; set; }

        /// <summary>
        /// 当前页记录列表
        /// </summary>
        public List<T> Items { get; set; }
    }
}
