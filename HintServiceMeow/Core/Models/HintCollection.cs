using System;
using System.Collections.Generic;
using System.Linq;
using HintServiceMeow.Core.Models.Hints;

namespace HintServiceMeow.Core.Models
{
    /// <summary>
    /// The collection of hints. This class is used to store and manage hints in PlayerDisplay.
    /// HintList are used for API and HintGroup are used for internal usage.
    /// </summary>
    internal class HintCollection
    {
        private readonly Dictionary<string, List<AbstractHint>> _hintGroups = new Dictionary<string, List<AbstractHint>>();

        public IEnumerable<List<AbstractHint>> AllGroups => _hintGroups.Values;

        public IEnumerable<AbstractHint> AllHints => _hintGroups.Values.SelectMany(x => x);

        public void AddHint(string assemblyName, AbstractHint hint)
        {
            if (!_hintGroups.ContainsKey(assemblyName))
                _hintGroups[assemblyName] = new List<AbstractHint>();

            _hintGroups[assemblyName].Add(hint);
        }

        public void RemoveHint(string assemblyName, AbstractHint hint)
        {
            if (!_hintGroups.TryGetValue(assemblyName, out var hintList))
                return;

            hintList.Remove(hint);
        }

        public void RemoveHint(string assemblyName, Predicate<AbstractHint> predicate)
        {
            if (!_hintGroups.TryGetValue(assemblyName, out var hintList))
                return;

            hintList.RemoveAll(predicate);
        }

        public void ClearHints(string assemblyName)
        {
            if (_hintGroups.TryGetValue(assemblyName, out var hintList))
                hintList.Clear();
        }

        public IEnumerable<AbstractHint> GetHints(string assemblyName)
        {
            return _hintGroups.TryGetValue(assemblyName, out var hintList) ? hintList : new List<AbstractHint>();
        }
    }
}
