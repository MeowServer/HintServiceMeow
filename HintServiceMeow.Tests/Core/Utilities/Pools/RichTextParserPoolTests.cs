using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Core.Utilities.Pools;
using HintServiceMeow.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Utilities.Pools
{
    [TestClass]
    public class RichTextParserPoolTests
    {
        [TestInitialize]
        public void Setup()
        {
            // Drain the singleton pool to ensure test isolation
            RichTextParserPool pool = RichTextParserPool.Instance;
            ConcurrentBag<RichTextParser> queue = ReflectionHelper.GetFieldValueFromParent<ConcurrentBag<RichTextParser>>(pool, "objectBag");
            while (queue.TryTake(out _)) { }
        }

        #region Basic Rent/Return

        [TestMethod]
        public void Rent_WhenPoolEmpty_ReturnsNewParser()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;

            // Act
            RichTextParser parser = pool.Rent();

            // Assert
            Assert.IsNotNull(parser);
        }

        [TestMethod]
        public void Return_ThenRent_ReusesSameInstance()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            RichTextParser original = pool.Rent();

            // Act
            pool.Return(original);
            RichTextParser reused = pool.Rent();

            // Assert
            Assert.AreSame(original, reused);
        }

        [TestMethod]
        public void Rent_MultipleTimes_ReturnsDistinctInstances()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            int count = 5;
            List<RichTextParser> instances = [];

            // Act
            for (int i = 0; i < count; i++)
                instances.Add(pool.Rent());

            // Assert
            HashSet<RichTextParser> set = new(new ReferenceComparer<RichTextParser>());
            foreach (RichTextParser parser in instances)
            {
                Assert.IsTrue(set.Add(parser), "Pool returned duplicate RichTextParser instance");
            }
        }

        #endregion

        #region State Clearing Bug Detection

        /// <summary>
        /// [BUG DETECTION] Return() does not call ClearStatus() on the parser.
        /// Expected: After Return and Rent, parser fontSizeStack should be empty.
        /// Actual: State from previous use is retained because Return() just enqueues without clearing.
        /// </summary>
        [TestMethod]
        [Ignore("RichTextParserPool no longer clear data")]
        public void Return_DoesNotClearParserState_FontSizeStack()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            RichTextParser parser = pool.Rent();

            Stack<float> fontSizeStack = ReflectionHelper.GetFieldValue<Stack<float>>(parser, "fontSizeStack");
            fontSizeStack.Push(42f);
            Assert.AreEqual(1, fontSizeStack.Count);

            // Act
            pool.Return(parser);
            RichTextParser reused = pool.Rent();

            // Assert
            Assert.AreSame(parser, reused);
            Stack<float> reusedStack = ReflectionHelper.GetFieldValue<Stack<float>>(reused, "fontSizeStack");
            // If this PASSES (Count == 0), the bug is fixed.
            // If this FAILS, the bug still exists (state leaks between uses).
            Assert.AreEqual(0, reusedStack.Count,
                "[BUG] Parser fontSizeStack not cleared on Return — state leaks between uses");
        }

        /// <summary>
        /// [BUG DETECTION] Return() does not call ClearStatus() on the parser.
        /// Expected: After Return and Rent, parser style should be FontStyle.Normal.
        /// Actual: Style from previous use is retained.
        /// </summary>
        [TestMethod]
        [Ignore("RichTextParserPool no longer clear data")]
        public void Return_DoesNotClearParserState_StyleField()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            RichTextParser parser = pool.Rent();
            ReflectionHelper.SetFieldValue(parser, "style", FontStyle.Bold);

            // Act
            pool.Return(parser);
            RichTextParser reused = pool.Rent();

            // Assert
            FontStyle style = ReflectionHelper.GetFieldValue<FontStyle>(reused, "style");
            // If this PASSES (style == Normal), the bug is fixed.
            Assert.AreEqual(FontStyle.Normal, style,
                "[BUG] Parser style field not cleared on Return — state leaks between uses");
        }

        /// <summary>
        /// [BUG DETECTION] Return() does not call ClearStatus() on the parser.
        /// Expected: After Return and Rent, hintAlignmentStack should be empty.
        /// Actual: Alignment stack from previous use is retained.
        /// </summary>
        [TestMethod]
        [Ignore("RichTextParserPool no longer clear data")]
        public void Return_DoesNotClearParserState_AlignmentStack()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            RichTextParser parser = pool.Rent();

            Stack<HintAlignment> alignStack = ReflectionHelper.GetFieldValue<Stack<HintAlignment>>(parser, "hintAlignmentStack");
            alignStack.Push(HintAlignment.Left);

            // Act
            pool.Return(parser);
            RichTextParser reused = pool.Rent();

            // Assert
            Stack<HintAlignment> reusedStack = ReflectionHelper.GetFieldValue<Stack<HintAlignment>>(reused, "hintAlignmentStack");
            Assert.AreEqual(0, reusedStack.Count,
                "[BUG] Parser hintAlignmentStack not cleared on Return — state leaks between uses");
        }

        /// <summary>
        /// [BUG DETECTION] Return() does not call ClearStatus() on the parser.
        /// Expected: After Return and Rent, caseStyleStack should be empty.
        /// Actual: Case style stack from previous use is retained.
        /// </summary>
        [TestMethod]
        [Ignore("RichTextParserPool no longer clear data")]
        public void Return_DoesNotClearParserState_CaseStyleStack()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            RichTextParser parser = pool.Rent();

            List<CaseStyle> caseStyleStack = ReflectionHelper.GetFieldValue<List<CaseStyle>>(parser, "caseStyleStack");
            caseStyleStack.Add(CaseStyle.Uppercase);

            // Act
            pool.Return(parser);
            RichTextParser reused = pool.Rent();

            // Assert
            List<CaseStyle> reusedStack = ReflectionHelper.GetFieldValue<List<CaseStyle>>(reused, "caseStyleStack");
            Assert.AreEqual(0, reusedStack.Count,
                "[BUG] Parser caseStyleStack not cleared on Return — state leaks between uses");
        }

        /// <summary>
        /// [BUG DETECTION] Return() does not call ClearStatus() on the parser.
        /// Expected: After Return and Rent, scriptStyles should be empty.
        /// Actual: Script styles from previous use is retained.
        /// </summary>
        [TestMethod]
        [Ignore("RichTextParserPool no longer clear data")]
        public void Return_DoesNotClearParserState_ScriptStyles()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            RichTextParser parser = pool.Rent();

            List<ScriptStyle> scriptStyles = ReflectionHelper.GetFieldValue<List<ScriptStyle>>(parser, "scriptStyles");
            scriptStyles.Add(ScriptStyle.Superscript);

            // Act
            pool.Return(parser);
            RichTextParser reused = pool.Rent();

            // Assert
            List<ScriptStyle> reusedStyles = ReflectionHelper.GetFieldValue<List<ScriptStyle>>(reused, "scriptStyles");
            Assert.AreEqual(0, reusedStyles.Count,
                "[BUG] Parser scriptStyles not cleared on Return — state leaks between uses");
        }

        /// <summary>
        /// [BUG DETECTION] Return() does not call ClearStatus() on the parser.
        /// Expected: After Return and Rent, currentRawLineText should be empty.
        /// Actual: Raw line text from previous use is retained.
        /// </summary>
        [TestMethod]
        [Ignore("RichTextParserPool no longer clear data")]
        public void Return_DoesNotClearParserState_RawLineText()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            RichTextParser parser = pool.Rent();

            StringBuilder rawLineText = ReflectionHelper.GetFieldValue<StringBuilder>(parser, "currentRawLineText");
            rawLineText.Append("leftover text from previous parse");

            // Act
            pool.Return(parser);
            RichTextParser reused = pool.Rent();

            // Assert
            StringBuilder reusedText = ReflectionHelper.GetFieldValue<StringBuilder>(reused, "currentRawLineText");
            Assert.AreEqual(0, reusedText.Length,
                "[BUG] Parser currentRawLineText not cleared on Return — state leaks between uses");
        }

        #endregion

        #region Concurrency

        [TestMethod]
        [Timeout(10000)]
        public async Task ConcurrentRent_NoDuplicateInstances()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            int count = 100;

            // Pre-fill pool
            for (int i = 0; i < count; i++)
                pool.Return(new RichTextParser());

            ConcurrentBag<RichTextParser> results = [];

            // Act
            var tasks = Enumerable.Range(0, count).Select(_ => Task.Run(() =>
            {
                results.Add(pool.Rent());
            }));
            await Task.WhenAll(tasks);

            // Assert
            HashSet<RichTextParser> set = new(new ReferenceComparer<RichTextParser>());
            foreach (RichTextParser parser in results)
            {
                Assert.IsTrue(set.Add(parser),
                    "Pool returned duplicate RichTextParser instance in concurrent access");
            }
        }

        [TestMethod]
        [Timeout(10000)]
        public async Task ConcurrentReturn_ThenRent_AllAvailable()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            int count = 100;
            List<RichTextParser> parsers = Enumerable.Range(0, count).Select(_ => new RichTextParser()).ToList();

            // Act - return all concurrently
            var returnTasks = parsers.Select(p => Task.Run(() => pool.Return(p)));
            await Task.WhenAll(returnTasks);

            // Rent all back
            ConcurrentBag<RichTextParser> rented = [];
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
        [Ignore("Requires FontTool to be available for ParseText")]
        public async Task ConcurrentRentAndParse_ResultsIndependent()
        {
            // This test requires FontTool.Instance to be available for ParseText to work.
            // In a game runtime environment, this would verify that concurrent parsing
            // produces independent results for each parser instance.
            await Task.CompletedTask;
        }

        #endregion

        #region Boundary

        [TestMethod]
        public void Return_WhenNull_DoesNotThrows()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;

            // Act & Assert
            pool.Return(null);
        }

        [TestMethod]
        public void DoubleReturn_WhenSameParserReturnedTwice_DoesNotCauseDuplicateRent()
        {
            // Arrange
            RichTextParserPool pool = RichTextParserPool.Instance;
            RichTextParser parser = pool.Rent();

            // Act - return the same instance twice
            pool.Return(parser);
            pool.Return(parser);

            RichTextParser first = pool.Rent();
            RichTextParser second = pool.Rent();

            // Assert
            // ConcurrentQueue stores two references to the same object.
            // Both rents return the same instance - this is a potential problem.
            Assert.AreSame(first, second,
                "Double return causes same instance to be rented twice — potential issue " +
                "if two consumers modify it concurrently");
        }

        #endregion

        #region Singleton

        [TestMethod]
        public void Instance_WhenAccessedMultipleTimes_AlwaysReturnsSameInstance()
        {
            // Act
            RichTextParserPool instance1 = RichTextParserPool.Instance;
            RichTextParserPool instance2 = RichTextParserPool.Instance;

            // Assert
            Assert.AreSame(instance1, instance2);
        }

        #endregion
    }
}
