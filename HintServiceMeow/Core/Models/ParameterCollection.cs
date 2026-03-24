namespace HintServiceMeow.Core.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using HintServiceMeow.Core.Interface;

    public class ParameterCollection : IEnumerable<Tuple<string, IParameter>>
    {
        private readonly object collectionLock = new object();
        private readonly List<Tuple<string, IParameter>> list = new(4);

        public ParameterCollection()
        {
        }

        public ParameterCollection(ParameterCollection other)
        {
            lock (other.collectionLock)
            {
                list.AddRange(other.list);
            }
        }

        public void Add(string tagName, IParameter parameter)
        {
            lock (collectionLock)
            {
                list.RemoveAll(x => x.Item1 == tagName);
                list.Add(Tuple.Create(tagName, parameter));
            }
        }

        public void RemoveAll(Predicate<Tuple<string, IParameter>> match)
        {
            lock (collectionLock)
            {
                list.RemoveAll(match);
            }
        }

        public void RemoveParameter(string tagName) => RemoveAll(x => x.Item1 == tagName);

        public void RemoveParameters<T>()
            where T : IParameter => RemoveAll(p => p.Item2 is T);

        public Tuple<string, IParameter>[] ToArray()
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

        IEnumerator<Tuple<string, IParameter>> IEnumerable<Tuple<string, IParameter>>.GetEnumerator()
        {
            lock (collectionLock)
            {
                return ((IEnumerable<Tuple<string, IParameter>>)list.ToArray()).GetEnumerator();
            }
        }
    }
}
