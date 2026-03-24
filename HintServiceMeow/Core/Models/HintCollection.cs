namespace HintServiceMeow.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
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
                    {
                        allGroupsCache = new AbstractHint[hintGroups.Count][];

                        int index = 0;

                        foreach (List<AbstractHint> group in hintGroups.Values)
                        {
                            allGroupsCache[index++] = group.ToArray();
                        }
                    }

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
                    {
                        int total = 0;
                        foreach (List<AbstractHint> group in hintGroups.Values)
                        {
                            total += group.Count;
                        }

                        allHintsCache = new AbstractHint[total];

                        int index = 0;
                        foreach (List<AbstractHint> group in hintGroups.Values)
                        {
                            group.CopyTo(allHintsCache, index);
                            index += group.Count;
                        }
                    }

                    return allHintsCache;
                }
            }
        }

        /// <summary>
        /// Retrieves hints belonging to the specified assembly, or all hints if <paramref name="assemblyName"/> is <see langword="null"/>.
        /// </summary>
        /// <param name="assemblyName">The assembly name used to filter hints, or <see langword="null"/> to retrieve all hints.</param>
        /// <returns>A read-only list of matching hints.</returns>
        public AbstractHint[] GetHints(string? assemblyName)
        {
            if (assemblyName is null)
                return AllHints;

            lock (collectionLock)
            {
                if (!hintGroups.TryGetValue(assemblyName, out List<AbstractHint> collection))
                    return Array.Empty<AbstractHint>();

                return collection.ToArray();
            }
        }

        /// <summary>
        /// Retrieves hints belonging to the specified assembly that satisfy the given predicate.
        /// </summary>
        /// <param name="assemblyName">The assembly name used to filter hints.</param>
        /// <param name="predicate">A function to further filter hints within the assembly group.</param>
        /// <returns>A read-only list of hints that match both the assembly name and the predicate.</returns>
        public AbstractHint[] GetHints(string assemblyName, Func<AbstractHint, bool> predicate)
        {
            lock (collectionLock)
            {
                if (!hintGroups.TryGetValue(assemblyName, out List<AbstractHint>? collection))
                    return Array.Empty<AbstractHint>();

                List<AbstractHint> resultList = new List<AbstractHint>(collection.Count);

                for (int i = 0; i < collection.Count; i++)
                {
                    AbstractHint hint = collection[i];
                    if (predicate(hint))
                    {
                        resultList.Add(hint);
                    }
                }

                return resultList.ToArray();
            }
        }

        /// <summary>
        /// Adds a hint to the collection under the specified assembly group.
        /// </summary>
        /// <param name="assemblyName">The name of the assembly group to add the hint to.</param>
        /// <param name="hint">The hint to add.</param>
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

        /// <summary>
        /// Removes the specified hint from the collection.
        /// If <paramref name="assemblyName"/> is null, the hint is removed from all groups.
        /// </summary>
        /// <param name="assemblyName">The assembly group to search in, or null to search all groups.</param>
        /// <param name="hint">The hint to remove.</param>
        /// <returns>true if the hint was found and removed; otherwise false.</returns>
        internal bool RemoveHint(string? assemblyName, AbstractHint hint)
        {
            bool success = false;

            List<string>? keysToRemove = null;

            lock (collectionLock)
            {
                // If assemblyName is null, remove the hint from all groups.
                if (assemblyName is null)
                {
                    foreach (KeyValuePair<string, List<AbstractHint>> group in hintGroups)
                    {
                        if (group.Value.Remove(hint))
                        {
                            success = true;
                        }

                        if (group.Value.Count == 0)
                        {
                            keysToRemove ??= new List<string>();
                            keysToRemove.Add(group.Key);
                        }
                    }

                    // Remove all empty groups.
                    if (keysToRemove is not null)
                    {
                        foreach (string key in keysToRemove)
                        {
                            hintGroups.Remove(key);
                        }
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

                    if (assemblyCollection.Count == 0)
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

        /// <summary>
        /// Removes all hints satisfying the predicate from the collection.
        /// If <paramref name="assemblyName"/> is null, hints are removed from all groups.
        /// </summary>
        /// <param name="assemblyName">The assembly group to search in, or null to search all groups.</param>
        /// <param name="predicate">A function that returns true for hints to remove.</param>
        /// <returns>A list of hints that were removed.</returns>
        internal List<AbstractHint> RemoveHint(string? assemblyName, Func<AbstractHint, bool> predicate)
        {
            List<AbstractHint> updatedHints = [];
            List<string>? keysToRemove = null;

            lock (collectionLock)
            {
                // If assemblyName is null, remove all hints that satisfy the predicate from all groups.
                if (assemblyName is null)
                {
                    foreach (KeyValuePair<string, List<AbstractHint>> group in hintGroups)
                    {
                        group.Value.RemoveAll(h =>
                        {
                            if (predicate(h))
                            {
                                updatedHints.Add(h);
                                return true;
                            }

                            return false;
                        });

                        if (group.Value.Count == 0)
                        {
                            keysToRemove ??= new List<string>();
                            keysToRemove.Add(group.Key);
                        }
                    }

                    // Remove all empty groups.
                    if (keysToRemove is not null)
                    {
                        foreach (string key in keysToRemove)
                        {
                            hintGroups.Remove(key);
                        }
                    }
                }
                else
                {
                    // If assemblyName is not null, remove all hints that satisfy the predicate from the specified group.
                    if (!hintGroups.TryGetValue(assemblyName, out List<AbstractHint> assemblyCollection))
                        return updatedHints;

                    assemblyCollection.RemoveAll(h =>
                    {
                        if (predicate(h))
                        {
                            updatedHints.Add(h);
                            return true;
                        }

                        return false;
                    });

                    if (assemblyCollection.Count == 0)
                    {
                        hintGroups.Remove(assemblyName);
                    }
                }
            }

            if (updatedHints.Count > 0)
            {
                foreach (AbstractHint hint in updatedHints)
                {
                    OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, hint));
                }
            }

            return updatedHints;
        }

        /// <summary>
        /// Clears all hints from the specified assembly group, or from all groups if null.
        /// </summary>
        /// <param name="assemblyName">The assembly group to clear, or null to clear all groups.</param>
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
