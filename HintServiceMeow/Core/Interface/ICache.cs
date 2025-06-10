namespace HintServiceMeow.Core.Interface
{
    internal interface ICache<TKey, TItem>
    {
        public void Add(TKey key, TItem data);

        public bool TryGet(TKey key, out TItem item);

        public bool TryRemove(TKey key, out TItem item);
    }
}
