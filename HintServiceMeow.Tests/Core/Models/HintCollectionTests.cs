using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Models;

[TestClass]
public class HintCollectionTests
{
    [TestMethod]
    public void AddHint_WhenNewHintAdded_RaisesAddEventAndUpdatesState()
    {
        // Arrange
        HintCollection collection = new();
        Hint hint = new() { Id = "A" };
        NotifyCollectionChangedEventArgs? evt = null;
        collection.CollectionChanged += (_, e) => evt = e;

        // Act
        collection.AddHint("asm", hint);

        // Assert
        Assert.IsNotNull(evt);
        Assert.AreEqual(NotifyCollectionChangedAction.Add, evt.Action);
        CollectionAssert.Contains(collection.GetHints("asm").ToList(), hint);
    }

    [TestMethod]
    public void RemoveHint_WhenHintDoesNotExist_DoesNotRaiseEvent()
    {
        // Arrange
        HintCollection collection = new();
        int eventCount = 0;
        collection.CollectionChanged += (_, _) => eventCount++;

        // Act
        bool removed = collection.RemoveHint("asm", new Hint());

        // Assert
        Assert.IsFalse(removed);
        Assert.AreEqual(0, eventCount);
    }

    [TestMethod]
    public void AllHints_WhenMutated_UsesSnapshotSemantics()
    {
        // Arrange
        HintCollection collection = new();
        Hint first = new() { Id = "1" };
        Hint second = new() { Id = "2" };
        collection.AddHint("asm", first);

        // Act
        IReadOnlyList<AbstractHint> snapshot = collection.AllHints;
        collection.AddHint("asm", second);

        // Assert
        Assert.AreEqual(1, snapshot.Count);
        Assert.AreEqual(2, collection.AllHints.Length);
    }

    [TestMethod]
    public void RemoveHintByPredicate_WhenAllRemoved_CleansUpEmptyGroup()
    {
        // Arrange
        HintCollection collection = new();
        collection.AddHint("asm", new Hint { Id = "x" });

        // Act
        List<AbstractHint> removed = collection.RemoveHint("asm", _ => true);

        // Assert
        Assert.AreEqual(1, removed.Count);
        Assert.AreEqual(0, collection.GetHints("asm").Count);
        Assert.AreEqual(0, collection.AllGroups.Length);
    }

    [TestMethod]
    [Timeout(10000)]
    public async Task ConcurrentAddAndRead_WhenConcurrentAccess_StaysConsistent()
    {
        // Arrange
        HintCollection collection = new();
        ConcurrentBag<Exception> errors = [];
        int addCount = 0;

        // Act
        var tasks = Enumerable.Range(0, 50).Select(i => Task.Run(() =>
        {
            try
            {
                for (int j = 0; j < 40; j++)
                {
                    int idx = i * 40 + j;
                    collection.AddHint(idx % 2 == 0 ? "a" : "b", new Hint { Id = idx.ToString() });
                    _ = collection.AllHints.Length;
                    _ = collection.AllGroups.Length;
                    Interlocked.Increment(ref addCount);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        }));
        await Task.WhenAll(tasks);

        // Assert
        Assert.AreEqual(0, errors.Count);
        Assert.AreEqual(2000, collection.AllHints.Length);
    }
}
