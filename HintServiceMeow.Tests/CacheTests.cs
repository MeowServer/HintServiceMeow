using HintServiceMeow.Core.Utilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace HintServiceMeow.Tests
{
    [TestClass]
    public class CacheTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_Throws_OnInvalidMaxSize()
        {
            var cache = new Cache<string, int>(0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Add_Throws_OnNullKey()
        {
            var cache = new Cache<string, int>(5);
            cache.Add(null, 1);
        }

        [TestMethod]
        public void Add_And_TryGet_ReturnsCorrectValue()
        {
            var cache = new Cache<string, int>(3);
            cache.Add("a", 1);
            cache.Add("b", 2);

            Assert.IsTrue(cache.TryGet("a", out int v1));
            Assert.AreEqual(1, v1);
            Assert.IsTrue(cache.TryGet("b", out int v2));
            Assert.AreEqual(2, v2);
            Assert.IsFalse(cache.TryGet("c", out _));
        }

        [TestMethod]
        public void TryRemove_RemovesItem_And_ReturnsValue()
        {
            var cache = new Cache<string, int>(3);
            cache.Add("a", 1);
            cache.Add("b", 2);

            Assert.IsTrue(cache.TryRemove("a", out int val));
            Assert.AreEqual(1, val);
            Assert.IsFalse(cache.TryGet("a", out _));

            Assert.IsFalse(cache.TryRemove("c", out _));
        }

        [TestMethod]
        public void Add_Replaces_OldValue()
        {
            var cache = new Cache<string, int>(3);
            cache.Add("a", 1);
            cache.Add("a", 2);

            Assert.IsTrue(cache.TryGet("a", out int v));
            Assert.AreEqual(2, v);
        }

        [TestMethod]
        public void Capacity_Is_Respected_And_LRU_Removed()
        {
            var cache = new Cache<string, int>(2);
            cache.Add("a", 1);
            cache.Add("b", 2);

            // a, b in cache (a is oldest, b is newest)
            cache.Add("c", 3); // Should remove "a"

            Assert.IsFalse(cache.TryGet("a", out _));
            Assert.IsTrue(cache.TryGet("b", out int v2) && v2 == 2);
            Assert.IsTrue(cache.TryGet("c", out int v3) && v3 == 3);
        }

        [TestMethod]
        public void Access_Updates_LRU_Order()
        {
            var cache = new Cache<string, int>(2);
            cache.Add("a", 1); // a
            cache.Add("b", 2); // b,a
            cache.TryGet("a", out _); // a,b
            cache.Add("c", 3); // b should be removed here

            Assert.IsTrue(cache.TryGet("a", out int v1) && v1 == 1);
            Assert.IsTrue(cache.TryGet("c", out int v2) && v2 == 3);
            Assert.IsFalse(cache.TryGet("b", out _));
        }

        [TestMethod]
        public void TryRemove_OnNonExistentKey_DoesNothing()
        {
            var cache = new Cache<string, int>(2);
            cache.Add("a", 1);
            Assert.IsFalse(cache.TryRemove("x", out _));
        }

        [TestMethod]
        public void Multiple_Keys_Work()
        {
            var cache = new Cache<int, string>(10);
            for (int i = 0; i < 10; i++)
                cache.Add(i, i.ToString());

            for (int i = 0; i < 10; i++)
                Assert.IsTrue(cache.TryGet(i, out string s) && s == i.ToString());
        }

        [TestMethod]
        public void Cache_Thread_Safety()
        {
            var cache = new Cache<int, int>(1000);
            var random = new Random();
            int threadCount = 8;
            var threads = new System.Threading.Thread[threadCount];

            for (int t = 0; t < threadCount; t++)
            {
                threads[t] = new System.Threading.Thread(() =>
                {
                    for (int i = 0; i < 5000; i++)
                    {
                        int key = random.Next(0, 1500);
                        cache.Add(key, key);
                        cache.TryGet(key, out _);
                        cache.TryRemove(key, out _);
                    }
                });
            }

            foreach (var th in threads) th.Start();
            foreach (var th in threads) th.Join();

            // No exception = passed test
            Assert.IsTrue(true);
        }
    }
}
