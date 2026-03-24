using System.Collections.Concurrent;
using HintServiceMeow.Core.Interface;

/// <summary>
/// A standard pool base. Inherit to create a pool for a specific type.
/// </summary>
/// <typeparam name="T">The type of instance you want to pool.</typeparam>
internal abstract class PoolBase<T> : IPool<T>
{
    private readonly ConcurrentBag<T> objectBag = new();
    private readonly int maxSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="PoolBase{T}"/> class with the specified maximum pool size.
    /// </summary>
    /// <param name="maxSize">The maximum number of items that the pool can contain. Must be greater than zero. The default value is 20.</param>
    protected PoolBase(int maxSize = 20)
    {
        this.maxSize = maxSize;
    }

    /// <summary>
    /// Retrieves an object from the pool for use by the caller.
    /// </summary>
    /// <returns>An instance of type T from the pool if available; otherwise, a new instance is created and returned.</returns>
    public T Rent()
    {
        return objectBag.TryTake(out var item) ? item : Create();
    }

    /// <summary>
    /// Returns an object to the pool for potential reuse.
    /// </summary>
    /// <param name="item">The object to return to the pool. Cannot be null.</param>
    public void Return(T? item)
    {
        if (item is null)
            return;

        if (objectBag.Count < maxSize)
        {
            Reset(item);
            objectBag.Add(item);
        }
    }

    /// <summary>
    /// Creates a new instance of type T.
    /// </summary>
    /// <returns>A new instance of type T.</returns>
    protected abstract T Create();

    /// <summary>
    /// Resets the specified item to its initial state or default values.
    /// </summary>
    /// <param name="item">The item to reset. The state of this item will be modified to its initial or default configuration.</param>
    protected abstract void Reset(T item);
}