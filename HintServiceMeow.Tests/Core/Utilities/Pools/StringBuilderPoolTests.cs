using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HintServiceMeow.Core.Utilities.Pools;
using HintServiceMeow.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Utilities.Pools
{
    [TestClass]
    public class StringBuilderPoolTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Drain the singleton pool to ensure test isolation
            var pool = StringBuilderPool.Instance;
            var bag = ReflectionHelper.GetFieldValue<ConcurrentBag<StringBuilder>>(pool, "stringBuilderQueue");
            while (bag.TryTake(out _)) { }
        }

        #region Basic Rent/Return

        [TestMethod]
        public void Rent_WhenPoolEmpty_ReturnsNewStringBuilder()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;

            // Act
            var sb = pool.Rent();

            // Assert
            Assert.IsNotNull(sb);
            Assert.AreEqual(0, sb.Length);
        }

        [TestMethod]
        public void Rent_WhenPoolEmpty_InitialCapacityIsAtLeast2000()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;

            // Act
            var sb = pool.Rent();

            // Assert
            Assert.IsTrue(sb.Capacity >= 2000,
                $"Expected capacity >= 2000 but got {sb.Capacity}");
        }

        [TestMethod]
        public void Return_ThenRent_ReusesSameInstance()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;
            var original = pool.Rent();

            // Act
            pool.Return(original);
            var reused = pool.Rent();

            // Assert
            Assert.AreSame(original, reused);
        }

        [TestMethod]
        public void Return_ClearsStringBuilderContent()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;
            var sb = pool.Rent();
            sb.Append("some data that should be cleared");

            // Act
            pool.Return(sb);
            var reused = pool.Rent();

            // Assert
            Assert.AreSame(sb, reused);
            Assert.AreEqual(0, reused.Length, "StringBuilder should be cleared after Return");
        }

        [TestMethod]
        public void Rent_MultipleTimes_ReturnsDistinctInstances()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;
            int count = 5;
            var instances = new List<StringBuilder>();

            // Act
            for (int i = 0; i < count; i++)
                instances.Add(pool.Rent());

            // Assert - all should be distinct references
            var set = new HashSet<StringBuilder>(new ReferenceComparer<StringBuilder>());
            foreach (var sb in instances)
            {
                Assert.IsTrue(set.Add(sb), "Pool returned duplicate StringBuilder instance");
            }
        }

        #endregion

        #region ToStringReturn

        [TestMethod]
        public void ToStringReturn_ReturnsCorrectString()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;
            var sb = pool.Rent();
            sb.Append("Hello");

            // Act
            string result = pool.ToStringReturn(sb);

            // Assert
            Assert.AreEqual("Hello", result);
        }

        [TestMethod]
        public void ToStringReturn_ReturnsStringBuilderToPool()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;
            var sb = pool.Rent();
            sb.Append("data");

            // Act
            pool.ToStringReturn(sb);
            var reused = pool.Rent();

            // Assert
            Assert.AreSame(sb, reused);
            Assert.AreEqual(0, reused.Length, "StringBuilder should be cleared after ToStringReturn");
        }

        [TestMethod]
        public void ToStringReturn_EmptyStringBuilder_ReturnsEmptyString()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;
            var sb = pool.Rent();

            // Act
            string result = pool.ToStringReturn(sb);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region Concurrency

        [TestMethod]
        [Timeout(10000)]
        public async Task ConcurrentRent_NoDuplicateInstances()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;
            int count = 100;

            // Pre-fill pool with distinct StringBuilders
            for (int i = 0; i < count; i++)
                pool.Return(new StringBuilder());

            var results = new ConcurrentBag<StringBuilder>();

            // Act
            var tasks = Enumerable.Range(0, count).Select(_ => Task.Run(() =>
            {
                results.Add(pool.Rent());
            }));
            await Task.WhenAll(tasks);

            // Assert - no duplicate references
            var set = new HashSet<StringBuilder>(new ReferenceComparer<StringBuilder>());
            foreach (var sb in results)
            {
                Assert.IsTrue(set.Add(sb), "Pool returned duplicate StringBuilder instance in concurrent access");
            }
        }

        [TestMethod]
        [Timeout(10000)]
        public async Task ConcurrentReturn_ThenRent_AllAvailable()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;
            int count = 100;
            var builders = Enumerable.Range(0, count).Select(_ => new StringBuilder()).ToList();

            // Act - return all concurrently
            var returnTasks = builders.Select(sb => Task.Run(() => pool.Return(sb)));
            await Task.WhenAll(returnTasks);

            // Rent all back
            var rented = new ConcurrentBag<StringBuilder>();
            var rentTasks = Enumerable.Range(0, count).Select(_ => Task.Run(() =>
            {
                rented.Add(pool.Rent());
            }));
            await Task.WhenAll(rentTasks);

            // Assert
            Assert.AreEqual(count, rented.Count);
        }

        [TestMethod]
        [Timeout(10000)]
        public async Task ConcurrentMixed_RentReturnToStringReturn_NoException()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;

            // Pre-fill pool
            for (int i = 0; i < 50; i++)
                pool.Return(new StringBuilder());

            // Act - mixed concurrent operations
            var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
            {
                switch (i % 3)
                {
                    case 0:
                        var sb = pool.Rent();
                        sb.Append("data");
                        pool.Return(sb);
                        break;
                    case 1:
                        var sb2 = pool.Rent();
                        sb2.Append("test");
                        pool.ToStringReturn(sb2);
                        break;
                    case 2:
                        pool.Rent();
                        break;
                }
            }));

            // Assert - should not throw
            await Task.WhenAll(tasks);
        }

        #endregion

        #region Boundary

        [TestMethod]
        public void Return_Null_ThrowsOrHandlesSafely()
        {
            // Arrange
            var pool = StringBuilderPool.Instance;

            // Act & Assert
            // Return null should not throw an exception.
            pool.Return(null);
        }

        #endregion

        #region Singleton

        [TestMethod]
        public void Instance_AlwaysReturnsSameInstance()
        {
            // Act
            var instance1 = StringBuilderPool.Instance;
            var instance2 = StringBuilderPool.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        #endregion
    }
}
