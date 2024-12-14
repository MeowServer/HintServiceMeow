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
        private readonly object _lock = new object();
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
                if (!_hintGroups.TryGetValue(assemblyName, out ObservableCollection<AbstractHint> collection))
                    return;

                collection.Remove(hint);
            }
        }

        internal void RemoveHint(string assemblyName, Func<AbstractHint, bool> predicate)
        {
            lock (_lock)
            {
                if (!_hintGroups.TryGetValue(assemblyName, out ObservableCollection<AbstractHint> collection))
                    return;

                foreach (AbstractHint hint in collection.Where(predicate).ToList())
                {
                    collection.Remove(hint);
                }
            }
        }

        internal void ClearHints(string assemblyName)
        {
            lock (_lock)
            {
                if (!_hintGroups.TryGetValue(assemblyName, out ObservableCollection<AbstractHint> collection))
                    return;

                collection.Clear();
            }
        }

        public IEnumerable<AbstractHint> GetHints(string assemblyName)
        {
            lock (_lock)
            {
                if (!_hintGroups.TryGetValue(assemblyName, out ObservableCollection<AbstractHint> collection))
                    return Enumerable.Empty<AbstractHint>();

                return collection.ToList();
            }
        }

        public IEnumerable<AbstractHint> GetHints(string assemblyName, Func<AbstractHint, bool> predicate)
        {
            return GetHints(assemblyName).Where(predicate);
        }
    }
}
