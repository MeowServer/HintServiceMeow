using System;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Utilities
{
    internal class Cache<TKey, TItem>
    {
        private readonly object _lock = new();

        private readonly Dictionary<TKey, CacheItem> _cache = new();
        private readonly CacheItem _removeQueueHead;
        private readonly int _maxSize;

        public Cache(int maxSize)
        {
            if (maxSize < 1)
                throw new ArgumentOutOfRangeException(nameof(maxSize), "Cache size must be greater than 0.");

            _removeQueueHead = new CacheItem(default, default);
            _removeQueueHead.PrevItem = _removeQueueHead;
            _removeQueueHead.NextItem = _removeQueueHead;

            _maxSize = maxSize;
        }

        public void Add(TKey key, TItem data)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            CacheItem item = new CacheItem(key, data);

            lock (_lock)
            {
                // Remove the old item
                if (_cache.TryGetValue(key, out CacheItem oldItem))
                {
                    RemoveFromList(oldItem);
                    _cache.Remove(oldItem.Key);
                }

                _cache.Add(key, item);

                // Inset into queue
                InsertToList(item);

                // If reach maximum capacity, remove the oldest item
                if (_cache.Count > _maxSize)
                {
                    CacheItem lastItem = _removeQueueHead.PrevItem;
                    RemoveFromList(lastItem);
                    _cache.Remove(lastItem.Key);
                }
            }
        }

        public bool TryGet(TKey key, out TItem item)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out CacheItem cachedItem))
                {
                    RemoveFromList(cachedItem);// Remove from queue
                    InsertToList(cachedItem);// Insert to the front

                    item = cachedItem.Data;
                    return true;
                }
            }

            item = default;
            return false;
        }

        public bool TryRemove(TKey key, out TItem item)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out CacheItem cachedItem))
                {
                    RemoveFromList(cachedItem);
                    _cache.Remove(cachedItem.Key);
                    item = cachedItem.Data;
                    return true;
                }
            }

            item = default;
            return false;
        }

        private void RemoveFromList(CacheItem item)
        {
            if (item == null || item == _removeQueueHead)
                return;

            item.PrevItem.NextItem = item.NextItem;
            item.NextItem.PrevItem = item.PrevItem;
        }

        /// <summary>
        /// Insert an item to the first place of the remove queue
        /// </summary>
        /// <param name="item"></param>
        private void InsertToList(CacheItem item)
        {
            item.PrevItem = _removeQueueHead;
            item.NextItem = _removeQueueHead.NextItem;
            item.NextItem.PrevItem = item;
            item.PrevItem.NextItem = item;
        }

        private class CacheItem
        {
            public readonly TKey Key;
            public readonly TItem Data;

            public CacheItem PrevItem;
            public CacheItem NextItem;

            public CacheItem(TKey key, TItem data)
            {
                Key = key;
                Data = data;
            }
        }
    }
}
