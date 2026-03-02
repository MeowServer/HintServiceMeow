using HintServiceMeow.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HintServiceMeow.Tests
{
    [TestClass]
    public class CacheTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_InvalidMaxSize_ThrowsArgumentOutOfRangeException()
        {
            // Arrange & Act
            var cache = new Cache<string, int>(0);

            // Assert - ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_NullKey_ThrowsArgumentNullException()
        {
            // Arrange
            var cache = new Cache<string, int>(5);

            // Act
            cache.Add(null, 1);

            // Assert - ExpectedException
        }

        [TestMethod]
        public void Add_ValidKeyValue_CanBeRetrievedByTryGet()
        {
            // Arrange
            var cache = new Cache<string, int>(3);

            // Act
            cache.Add("a", 1);
            cache.Add("b", 2);

            // Assert
            Assert.IsTrue(cache.TryGet("a", out int v1));
            Assert.AreEqual(1, v1);
            Assert.IsTrue(cache.TryGet("b", out int v2));
            Assert.AreEqual(2, v2);
            Assert.IsFalse(cache.TryGet("c", out _));
        }

        [TestMethod]
        public void TryRemove_ExistingKey_RemovesAndReturnsValue()
        {
            // Arrange
            var cache = new Cache<string, int>(3);
            cache.Add("a", 1);
            cache.Add("b", 2);

            // Act
            bool removedA = cache.TryRemove("a", out int val);
            bool removedC = cache.TryRemove("c", out _);

            // Assert
            Assert.IsTrue(removedA);
            Assert.AreEqual(1, val);
            Assert.IsFalse(cache.TryGet("a", out _));
            Assert.IsFalse(removedC);
        }

        [TestMethod]
        public void Add_DuplicateKey_ReplacesOldValue()
        {
            // Arrange
            var cache = new Cache<string, int>(3);
            cache.Add("a", 1);

            // Act
            cache.Add("a", 2);

            // Assert
            Assert.IsTrue(cache.TryGet("a", out int v));
            Assert.AreEqual(2, v);
        }

        [TestMethod]
        public void Add_ExceedsCapacity_RemovesLRUItem()
        {
            // Arrange
            var cache = new Cache<string, int>(2);
            cache.Add("a", 1);
            cache.Add("b", 2);

            // Act
            cache.Add("c", 3); // Should remove "a" (oldest)

            // Assert
            Assert.IsFalse(cache.TryGet("a", out _));
            Assert.IsTrue(cache.TryGet("b", out int v2) && v2 == 2);
            Assert.IsTrue(cache.TryGet("c", out int v3) && v3 == 3);
        }

        [TestMethod]
        public void TryGet_ExistingKey_UpdatesLRUOrder()
        {
            // Arrange
            var cache = new Cache<string, int>(2);
            cache.Add("a", 1); // a
            cache.Add("b", 2); // b, a

            // Act
            cache.TryGet("a", out _); // a, b (a is now most recent)
            cache.Add("c", 3); // c, a (b should be removed as LRU)

            // Assert
            Assert.IsTrue(cache.TryGet("a", out int v1) && v1 == 1);
            Assert.IsTrue(cache.TryGet("c", out int v2) && v2 == 3);
            Assert.IsFalse(cache.TryGet("b", out _));
        }

        [TestMethod]
        public void TryRemove_NonExistentKey_ReturnsFalse()
        {
            // Arrange
            var cache = new Cache<string, int>(2);
            cache.Add("a", 1);

            // Act
            bool result = cache.TryRemove("x", out _);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void Add_MultipleKeys_AllRetrievableByTryGet()
        {
            // Arrange
            var cache = new Cache<int, string>(10);

            // Act
            for (int i = 0; i < 10; i++)
                cache.Add(i, i.ToString());

            // Assert
            for (int i = 0; i < 10; i++)
                Assert.IsTrue(cache.TryGet(i, out string s) && s == i.ToString());
        }

        [TestMethod]
        [Timeout(10000)]
        public async Task Add_ConcurrentAccess_ThreadSafe()
        {
            // Arrange
            var cache = new Cache<int, int>(1000);
            int threadCount = 50;
            int opsPerThread = 5000;
            int exceptionCount = 0;
            var results = new ConcurrentBag<bool>();

            // Act
            var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
            {
                var random = new Random();
                for (int i = 0; i < opsPerThread; i++)
                {
                    try
                    {
                        int key = random.Next(0, 1500);
                        cache.Add(key, key);
                        cache.TryGet(key, out _);
                        cache.TryRemove(key, out _);
                        results.Add(true);
                    }
                    catch
                    {
                        Interlocked.Increment(ref exceptionCount);
                        results.Add(false);
                    }
                }
            })).ToArray();

            await Task.WhenAll(tasks);

            // Assert
            Assert.AreEqual(0, exceptionCount, "Cache should not throw exceptions under concurrent access");
            Assert.AreEqual(threadCount * opsPerThread, results.Count, "All operations should complete");
        }
    }
}
