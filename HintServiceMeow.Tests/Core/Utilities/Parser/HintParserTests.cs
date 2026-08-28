using System;
using System.Text;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Tests.Core.Utilities.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Utilities.Parser;

[TestClass]
public class HintParserTests
{
    [TestMethod]
    public void ParseToMessage_WhenHiddenAndEmptyHintsExist_FiltersThemOut()
    {
        // Arrange
        HintCollection collection = new();
        collection.AddHint("a", new Hint { Text = "visible" });
        collection.AddHint("a", new Hint { Hide = true, Text = "hidden" });
        collection.AddHint("a", new Hint { Text = "" });

        HintParser parser = new();

        // Act
        string message = parser.ParseToMessage(collection);

        // Assert
        StringAssert.Contains(message, "visible");
        Assert.IsFalse(message.Contains("hidden"));
    }

    [TestMethod]
    public void ParseToMessage_WhenIllegalTagsPresent_RemovesThem()
    {
        // Arrange
        HintCollection collection = new();
        collection.AddHint("a", new Hint { Text = "<line-height=10>{X}<voffset=20>Y</voffset>" });

        HintParser parser = new();

        // Act
        string message = parser.ParseToMessage(collection);

        // Assert
        Assert.IsFalse(message.Contains("{X}"));
        Assert.IsFalse(message.Contains("<line-height=10>"));
        StringAssert.Contains(message, "Y");
    }

    [TestMethod]
    public void ParseToMessage_WhenPreserveCaseChanges_KeepsLegacyDefaultAndSeparatesCachedOutput()
    {
        // Arrange
        HintCollection collection = new();
        Hint hint = new() { Text = "Hello" };
        collection.AddHint("a", hint);

        HintParser parser = new();

        // Act
        string legacyMessage = parser.ParseToMessage(collection);
        hint.PreserveCase = true;
        string preservedMessage = parser.ParseToMessage(collection);

        // Assert
        StringAssert.Contains(legacyMessage, "Hello");
        Assert.IsFalse(legacyMessage.Contains("<lowercase>"));
        StringAssert.Contains(preservedMessage, "H<lowercase>ello</lowercase>");
    }

    [TestMethod]
    public void ParseToMessage_WhenMultipleHints_SortsByVisualBottomPosition()
    {
        // Arrange
        HintCollection collection = new();
        Hint low = new() { Text = "low", YCoordinate = 100 };
        Hint high = new() { Text = "high", YCoordinate = 400 };
        collection.AddHint("a", high);
        collection.AddHint("a", low);

        HintParser parser = new(coordinateTool: new StubCoordinateTools { YConverter = (h, _) => h.YCoordinate });

        // Act
        string message = parser.ParseToMessage(collection);

        // Assert
        Assert.IsTrue(message.IndexOf("low", StringComparison.Ordinal) < message.IndexOf("high", StringComparison.Ordinal));
    }

    [TestMethod]
    public void ParseToMessage_WhenDynamicHintHasNoFreePosition_AppliesHideStrategy()
    {
        // Arrange
        HintCollection collection = new();
        collection.AddHint("a", new Hint { Text = "blocker", YCoordinate = 100, XCoordinate = 0 });
        collection.AddHint("a", new DynamicHint
        {
            Text = "dynamic",
            TargetX = 0,
            TargetY = 100,
            LeftBoundary = 0,
            RightBoundary = 0,
            TopBoundary = 100,
            BottomBoundary = 100,
            Strategy = DynamicHintStrategy.Hide,
        });

        StubCoordinateTools tools = new() { TextWidth = _ => 1000, TextHeight = _ => 500, YConverter = (h, _) => h.YCoordinate };
        HintParser parser = new(coordinateTool: tools);

        // Act
        string message = parser.ParseToMessage(collection);

        // Assert
        Assert.IsFalse(message.Contains("dynamic"));
    }

    [TestMethod]
    public void ParseToMessage_WhenMultipleGroups_InsertsGroupStyleResetBetweenGroups()
    {
        // Arrange
        HintCollection collection = new();
        collection.AddHint("a", new Hint { Text = "<b>g1" });
        collection.AddHint("b", new Hint { Text = "g2" });

        HintParser parser = new();

        // Act
        string message = parser.ParseToMessage(collection);

        // Assert
        StringAssert.Contains(message, "</align></size></b></i>");
    }

    [TestMethod]
    public void ParseToMessage_WhenCompletes_ReturnsAllRentedBuildersToPool()
    {
        // Arrange
        RecordingPool<StringBuilder> sbPool = new(() => new StringBuilder());
        RecordingPool<RichTextParser> parserPool = new(() => new RichTextParser());

        HintCollection collection = new();
        collection.AddHint("a", new Hint { Text = "hello" });

        HintParser parser = new(stringBuilderPool: sbPool, richTextParserPool: parserPool);

        // Act
        _ = parser.ParseToMessage(collection);

        // Assert
        Assert.AreEqual(sbPool.RentCount, sbPool.ReturnCount);
        Assert.AreEqual(parserPool.RentCount, parserPool.ReturnCount);
    }
}
