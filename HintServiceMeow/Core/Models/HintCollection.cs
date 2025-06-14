using HintServiceMeow.Core.Models.Hints;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace HintServiceMeow.Core.Models
{
    /// <summary>
    /// The collection of hints. This class is used to store and manage hints in PlayerDisplay.
    /// HintList are used for API and HintGroup are used for internal usage.
    /// </summary>
    public class HintCollection : INotifyCollectionChanged
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, List<AbstractHint>> _hintGroups = new();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs argument)
        {
            CollectionChanged?.Invoke(this, argument);
        }

        public IReadOnlyList<IReadOnlyList<AbstractHint>> AllGroups
        {
            get
            {
                lock (_lock)
                {
                    return _hintGroups.Values.Select(x => x.ToList().AsReadOnly()).ToList().AsReadOnly();
                }
            }
        }

        public IReadOnlyList<AbstractHint> AllHints
        {
            get
            {
                lock (_lock)
                {
                    return _hintGroups.Values.SelectMany(x => x).ToList().AsReadOnly();
                }
            }
        }

        internal void AddHint(string assemblyName, AbstractHint hint)
        {
            lock (_lock)
            {
                if (!_hintGroups.TryGetValue(assemblyName, out List<AbstractHint> collection))
                {
                    collection = new List<AbstractHint>();
                    _hintGroups.Add(assemblyName, collection);
                }

                collection.Add(hint);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, hint));
        }

        internal bool RemoveHint(string assemblyName, AbstractHint hint)
        {
            bool success = false;

            lock (_lock)
            {
                //If assemblyName is null, remove the hint from all groups.
                if (assemblyName is null)
                {
                    foreach (var collection in _hintGroups.Values)
                    {
                        if (collection.Remove(hint))
                        {
                            success = true;
                        }
                    }
                }
                else
                {
                    //If assemblyName is not null, remove the hint from the specified group.
                    if (!_hintGroups.TryGetValue(assemblyName, out List<AbstractHint> assemblyCollection))
                        return false;

                    if (assemblyCollection.Remove(hint))
                    {
                        success = true;
                    }

                    if (!assemblyCollection.Any())
                    {
                        _hintGroups.Remove(assemblyName);
                    }
                }
            }

            if (success)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, hint));
            }

            return success;
        }

        internal List<AbstractHint> RemoveHint(string assemblyName, Func<AbstractHint, bool> predicate)
        {
            List<AbstractHint> updatedHints = new();

            lock (_lock)
            {
                //If assemblyName is null, remove all hints that satisfy the predicate from all groups.
                if (assemblyName is null)
                {
                    foreach (var collection in _hintGroups.Values)
                    {
                        for (int i = 0; i < collection.Count; i++)
                        {
                            AbstractHint hint = collection[i];

                            if (!predicate(hint))
                                continue;

                            //If the hint satisfies the predicate, remove it from the collection.
                            if (collection.Remove(hint))
                            {
                                updatedHints.Add(hint);
                                i--;
                            }
                        }
                    }
                }
                else
                {
                    //If assemblyName is not null, remove all hints that satisfy the predicate from the specified group.
                    if (!_hintGroups.TryGetValue(assemblyName, out List<AbstractHint> assemblyCollection))
                        return updatedHints;

                    for (int i = 0; i < assemblyCollection.Count; i++)
                    {
                        AbstractHint hint = assemblyCollection[i];

                        if (!predicate(hint))
                            continue;

                        if (assemblyCollection.Remove(hint))
                        {
                            updatedHints.Add(hint);
                            i--;
                        }
                    }

                    if (!assemblyCollection.Any())
                    {
                        _hintGroups.Remove(assemblyName);
                    }
                }
            }

            if (updatedHints.Any())
            {
                foreach (AbstractHint hint in updatedHints)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, hint));
                }
            }

            return updatedHints;
        }

        internal void ClearHints(string assemblyName)
        {
            lock (_lock)
            {
                //If assemblyName is null, clear all groups.
                if (assemblyName is null)
                {
                    foreach (var collection in _hintGroups.Values)
                    {
                        collection.Clear();
                    }
                }
                else
                {
                    //If assemblyName is not null, clear the specified group.
                    if (!_hintGroups.TryGetValue(assemblyName, out List<AbstractHint> assemblyCollection))
                        return;

                    assemblyCollection.Clear();
                }
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        public IReadOnlyList<AbstractHint> GetHints(string assemblyName)
        {
            lock (_lock)
            {
                if (assemblyName is null)
                    return _hintGroups.Values.SelectMany(x => x).ToList().AsReadOnly();

                if (!_hintGroups.TryGetValue(assemblyName, out List<AbstractHint> collection))
                    return new List<AbstractHint>().AsReadOnly();

                return collection.ToList().AsReadOnly();
            }
        }

        public IReadOnlyList<AbstractHint> GetHints(string assemblyName, Func<AbstractHint, bool> predicate)
        {
            return GetHints(assemblyName).Where(predicate).ToList().AsReadOnly();
        }
    }
}
