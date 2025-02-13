using HintServiceMeow.Core.Models.Hints;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly Dictionary<string, ObservableCollection<AbstractHint>> _hintGroups = new();

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs argument)
        {
            CollectionChanged?.Invoke(this, argument);
        }

        public List<List<AbstractHint>> AllGroups
        {
            get
            {
                lock (_lock)
                {
                    return _hintGroups.Values.Select(x => x.ToList()).ToList();
                }
            }
        }

        internal void AddHint(string assemblyName, AbstractHint hint)
        {
            lock (_lock)
            {
                if (!_hintGroups.TryGetValue(assemblyName, out ObservableCollection<AbstractHint> collection))
                {
                    collection = new ObservableCollection<AbstractHint>();
                    collection.CollectionChanged += OnCollectionChanged;
                    _hintGroups.Add(assemblyName, collection);
                }

                collection.Add(hint);
            }
        }

        internal void RemoveHint(string assemblyName, AbstractHint hint)
        {
            lock (_lock)
            {
                //If assemblyName is null, remove the hint from all groups.
                if (assemblyName is null)
                {
                    foreach (var collection in _hintGroups.Values)
                    {
                        collection.Remove(hint);
                    }
                    return;
                }

                //If assemblyName is not null, remove the hint from the specified group.
                if (!_hintGroups.TryGetValue(assemblyName, out ObservableCollection<AbstractHint> assemblyCollection))
                    return;

                assemblyCollection.Remove(hint);
            }
        }

        internal void RemoveHint(string assemblyName, Func<AbstractHint, bool> predicate)
        {
            lock (_lock)
            {
                //If assemblyName is null, remove all hints that satisfy the predicate from all groups.
                if (assemblyName is null)
                {
                    foreach (var collection in _hintGroups.Values)
                    {
                        foreach (AbstractHint hint in collection.Where(predicate).ToList())
                        {
                            collection.Remove(hint);
                        }
                    }

                    return;
                }

                //If assemblyName is not null, remove all hints that satisfy the predicate from the specified group.
                if (!_hintGroups.TryGetValue(assemblyName, out ObservableCollection<AbstractHint> assemblyCollection))
                    return;

                foreach (AbstractHint hint in assemblyCollection.Where(predicate).ToList())
                {
                    assemblyCollection.Remove(hint);
                }
            }
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

                    return;
                }

                //If assemblyName is not null, clear the specified group.
                if (!_hintGroups.TryGetValue(assemblyName, out ObservableCollection<AbstractHint> assemblyCollection))
                    return;

                assemblyCollection.Clear();
            }
        }

        public IEnumerable<AbstractHint> GetHints(string assemblyName)
        {
            lock (_lock)
            {
                if (assemblyName is null)
                    return _hintGroups.Values.SelectMany(x => x).ToList();

                if (!_hintGroups.TryGetValue(assemblyName, out ObservableCollection<AbstractHint> collection))
                    return Enumerable.Empty<AbstractHint>();

                return collection.ToList();
            }
        }

        public IEnumerable<AbstractHint> GetHints(string assemblyName, Func<AbstractHint, bool> predicate)
        {
            if (assemblyName is null)
                return GetHints(null).Where(predicate);

            return GetHints(assemblyName).Where(predicate);
        }
    }
}
