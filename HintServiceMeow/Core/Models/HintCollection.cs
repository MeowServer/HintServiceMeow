using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using HintServiceMeow.Core.Models.Hints;

namespace HintServiceMeow.Core.Models
{
    /// <summary>
    /// The collection of hints. This class is used to store and manage hints in PlayerDisplay.
    /// HintList are used for API and HintGroup are used for internal usage.
    /// </summary>
    internal class HintCollection
    {
        private readonly Dictionary<string, HashSet<AbstractHint>> _hintGroups = new Dictionary<string, HashSet<AbstractHint>>();
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public IEnumerable<HashSet<AbstractHint>> AllGroups
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _hintGroups.Values.ToList(); //DO NOT REMOVE TO LIST WITHOUT CONSIDERING THREAD SAFETY
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public IEnumerable<AbstractHint> AllHints
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _hintGroups.Values.SelectMany(x => x).ToList(); //DO NOT REMOVE TO LIST WITHOUT CONSIDERING THREAD SAFETY
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public void AddHint(string assemblyName, AbstractHint hint)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_hintGroups.ContainsKey(assemblyName))
                    _hintGroups[assemblyName] = new HashSet<AbstractHint>();

                _hintGroups[assemblyName].Add(hint);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveHint(string assemblyName, AbstractHint hint)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_hintGroups.TryGetValue(assemblyName, out var hintList))
                    return;

                hintList.Remove(hint);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveHint(string assemblyName, Predicate<AbstractHint> predicate)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_hintGroups.TryGetValue(assemblyName, out var hintList))
                    return;

                hintList.RemoveWhere(predicate);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void ClearHints(string assemblyName)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_hintGroups.TryGetValue(assemblyName, out var hintList))
                    hintList.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<AbstractHint> GetHints(string assemblyName)
        {
            _lock.EnterReadLock();
            try
            {
                return _hintGroups.TryGetValue(assemblyName, out var hintList) ? hintList.ToList() : new List<AbstractHint>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

}
