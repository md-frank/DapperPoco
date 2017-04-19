// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-04-16
// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mondol
{
    public static class StringBuilderCache
    {
        [ThreadStatic]
        private static StringBuilder _cache;

        public static StringBuilder Allocate()
        {
            var sb = _cache;
            if (sb == null)
                return new StringBuilder();

            sb.Length = 0;
            _cache = null;
            return sb;
        }

        public static void Free(StringBuilder sb)
        {
            _cache = sb;
        }

        public static string ReturnAndFree(StringBuilder sb)
        {
            var str = sb.ToString();
            _cache = sb;
            return str;
        }
    }
}
