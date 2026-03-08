namespace HintServiceMeow.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Linq;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// The collection of hints. This class is used to store and manage hints in PlayerDisplay.
    /// HintList are used for API and HintGroup are used for internal usage.
    /// </summary>
    public class HintCollection : INotifyCollectionChanged
    {
        private readonly object collectionLock = new();
        private readonly Dictionary<string, List<AbstractHint>> hintGroups = new();

        private AbstractHint[][]? allGroupsCache;
        private AbstractHint[]? allHintsCache;

        /// <summary>
        /// Occurs when the hint collection is modified (hints added, removed, or cleared).
        /// </summary>
        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        /// <summary>
        /// Gets a read-only list of all hint groups, where each group corresponds to a registered assembly.
        /// </summary>
        public AbstractHint[][] AllGroups
        {
            get
            {
                lock (collectionLock)
                {
                    if (allGroupsCache == null)
                        allGroupsCache = hintGroups.Values.Select(x => x.ToArray()).ToArray();

                    return allGroupsCache;
                }
            }
        }

        /// <summary>
        /// Gets a read-only flat list of all hints across all groups.
        /// </summary>
        public AbstractHint[] AllHints
        {
            get
            {
                lock (collectionLock)
                {
                    if (allHintsCache == null)
                        allHintsCache = hintGroups.Values.SelectMany(x => x).ToArray();

                    return allHintsCache;
                }
            }
        }

        /// <summary>
        /// Retrieves hints belonging to the specified assembly, or all hints if <paramref name="assemblyName"/> is <see langword="null"/>.
        /// </summary>
        /// <param name="assemblyName">The assembly name used to filter hints, or <see langword="null"/> to retrieve all hints.</param>
        /// <returns>A read-only list of matching hints.</returns>
        public IReadOnlyList<AbstractHint> GetHints(string? assemblyName)
        {
            lock (collectionLock)
            {
                if (assemblyName is null)
                    return hintGroups.Values.SelectMany(x => x).ToList().AsReadOnly();

                if (!hintGroups.TryGetValue(assemblyName, out List<AbstractHint> collection))
                    return new List<AbstractHint>().AsReadOnly();

                return collection.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Retrieves hints belonging to the specified assembly that satisfy the given predicate.
        /// </summary>
        /// <param name="assemblyName">The assembly name used to filter hints.</param>
        /// <param name="predicate">A function to further filter hints within the assembly group.</param>
        /// <returns>A read-only list of hints that match both the assembly name and the predicate.</returns>
        public IReadOnlyList<AbstractHint> GetHints(string assemblyName, Func<AbstractHint, bool> predicate)
        {
            return GetHints(assemblyName).Where(predicate).ToList().AsReadOnly();
        }

        internal void AddHint(string assemblyName, AbstractHint hint)
        {
            lock (collectionLock)
            {
                if (!hintGroups.TryGetValue(assemblyName, out List<AbstractHint> collection))
                {
                    collection = new List<AbstractHint>();
                    hintGroups.Add(assemblyName, collection);
                }

                collection.Add(hint);
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, hint));
        }

        internal bool RemoveHint(string? assemblyName, AbstractHint hint)
        {
            bool success = false;

            lock (collectionLock)
            {
                // If assemblyName is null, remove the hint from all groups.
                if (assemblyName is null)
                {
                    foreach (List<AbstractHint>? collection in hintGroups.Values)
                    {
                        if (collection.Remove(hint))
                        {
                            success = true;
                        }
                    }

                    // Remove all empty groups.
                    foreach (string? key in hintGroups.Where(x => !x.Value.Any()).Select(x => x.Key).ToList())
                    {
                        hintGroups.Remove(key);
                    }
                }
                else
                {
                    // If assemblyName is not null, remove the hint from the specified group.
                    if (!hintGroups.TryGetValue(assemblyName, out List<AbstractHint> assemblyCollection))
                        return false;

                    if (assemblyCollection.Remove(hint))
                    {
                        success = true;
                    }

                    if (!assemblyCollection.Any())
                    {
                        hintGroups.Remove(assemblyName);
                    }
                }
            }

            if (success)
            {
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, hint));
            }

            return success;
        }

        internal List<AbstractHint> RemoveHint(string? assemblyName, Func<AbstractHint, bool> predicate)
        {
            List<AbstractHint> updatedHints = [];

            lock (collectionLock)
            {
                // If assemblyName is null, remove all hints that satisfy the predicate from all groups.
                if (assemblyName is null)
                {
                    foreach (List<AbstractHint>? collection in hintGroups.Values)
                    {
                        for (int i = 0; i < collection.Count; i++)
                        {
                            AbstractHint hint = collection[i];

                            if (!predicate(hint))
                                continue;

                            // If the hint satisfies the predicate, remove it from the collection.
                            collection.RemoveAt(i);
                            updatedHints.Add(hint);
                            i--;
                        }
                    }

                    // Remove all empty groups.
                    foreach (string? key in hintGroups.Where(x => !x.Value.Any()).Select(x => x.Key).ToList())
                    {
                        hintGroups.Remove(key);
                    }
                }
                else
                {
                    // If assemblyName is not null, remove all hints that satisfy the predicate from the specified group.
                    if (!hintGroups.TryGetValue(assemblyName, out List<AbstractHint> assemblyCollection))
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
                        hintGroups.Remove(assemblyName);
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

        internal void ClearHints(string? assemblyName)
        {
            lock (collectionLock)
            {
                // If assemblyName is null, clear all groups.
                if (assemblyName is null)
                {
                    hintGroups.Clear();
                }
                else
                {
                    // If assemblyName is not null, clear the specified group.
                    if (!hintGroups.TryGetValue(assemblyName, out List<AbstractHint> assemblyCollection))
                        return;

                    assemblyCollection.Clear();
                    hintGroups.Remove(assemblyName);
                }
            }

            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        private void OnCollectionChanged(NotifyCollectionChangedEventArgs argument)
        {
            lock (collectionLock)
            {
                // Clear caches on any collection change.
                allGroupsCache = null;
                allHintsCache = null;
            }

            CollectionChanged?.Invoke(this, argument);
        }
    }
}
