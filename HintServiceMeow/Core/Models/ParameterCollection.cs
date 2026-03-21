using System;
using System.Collections;
using System.Collections.Generic;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models
{
    public class ParameterCollection : IEnumerable<Tuple<string, IHintParameter>>
    {
        private readonly object collectionLock = new object();
        private readonly List<Tuple<string, IHintParameter>> list = new(4);

        public ParameterCollection() { }

        public ParameterCollection(ParameterCollection other)
        {
            lock (other.collectionLock)
            {
                list.AddRange(other.list);
            }
        }

        public void Add(string tagName, IHintParameter parameter)
        {
            lock (collectionLock)
            {
                list.RemoveAll(x => x.Item1 == tagName);
                list.Add(Tuple.Create(tagName, parameter));
            }
        }

        public void RemoveAll(Predicate<Tuple<string, IHintParameter>> match)
        {
            lock (collectionLock)
            {
                list.RemoveAll(match);
            }
        }

        public void RemoveParameter(string tagName) => RemoveAll(x => x.Item1 == tagName);

        public void RemoveParameters<T>() where T : IHintParameter => RemoveAll(p => p.Item2 is T);

        public Tuple<string, IHintParameter>[] ToArray()
        {
            lock (collectionLock)
            {
                return list.ToArray();
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            lock (collectionLock)
            {
                return list.ToArray().GetEnumerator();
            }
        }

        IEnumerator<Tuple<string, IHintParameter>> IEnumerable<Tuple<string, IHintParameter>>.GetEnumerator()
        {
            lock (collectionLock)
            {
                return ((IEnumerable<Tuple<string, IHintParameter>>)list.ToArray()).GetEnumerator();
            }
        }
    }
}
