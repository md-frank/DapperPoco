// Copyright (c) Mondol. All rights reserved.
// 
// Author:  frank
// Email:   frank@mondol.info
// Created: 2017-01-22
// 
using System;
using System.Collections.Generic;
using System.Threading;

namespace Mondol.DapperPoco.Internal
{
    internal class ConcurrentCache<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _map = new Dictionary<TKey, TValue>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public int Count => _map.Count;

        public TValue GetOrAdd(TKey key, Func<TValue> factory)
        {
            // Check cache
            _lock.EnterReadLock();
            TValue val;
            try
            {
                if (_map.TryGetValue(key, out val))
                    return val;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            // Cache it
            _lock.EnterWriteLock();
            try
            {
                // Check again
                if (_map.TryGetValue(key, out val))
                    return val;

                // Create it
                val = factory();
                _map.Add(key, val);

                return val;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ForEach(Action<KeyValuePair<TKey, TValue>> item)
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var kv in _map)
                {
                    item(kv);
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _map.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
