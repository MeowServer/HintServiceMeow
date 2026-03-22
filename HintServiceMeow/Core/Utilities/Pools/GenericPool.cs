using System.Collections.Concurrent;
using HintServiceMeow.Core.Interface;

internal abstract class PoolBase<T> : IPool<T>
{
    private readonly ConcurrentBag<T> objectBag = new();
    private readonly int maxSize;

    protected PoolBase(int maxSize = 20)
    {
        this.maxSize = maxSize;
    }

    public T Rent()
    {
        return objectBag.TryTake(out var item) ? item : Create();
    }

    public void Return(T item)
    {
        if (item is null)
            return;

        if (objectBag.Count < maxSize)
        {
            Reset(item);
            objectBag.Add(item);
        }
    }

    protected abstract T Create();

    protected abstract void Reset(T item);
}