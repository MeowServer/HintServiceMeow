using System;
using System.Collections.Generic;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Models.Parser;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Core.Utilities.Tools;
using HintServiceMeow.Tests.Core.Utilities.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Utilities.Tools;

[TestClass]
public class CoordinateToolsTests
{
    [TestMethod]
    public void GetYCoordinate_WhenConvertedBetweenAligns_IsReversible()
    {
        // Arrange
        CoordinateTools tools = new();

        // Act
        float bottom = tools.GetYCoordinate(500, 80, HintVerticalAlign.Top, HintVerticalAlign.Bottom);
        float restored = tools.GetYCoordinate(bottom, 80, HintVerticalAlign.Bottom, HintVerticalAlign.Top);

        // Assert
        Assert.AreEqual(500, restored);
    }

    [TestMethod]
    public void GetXCoordinateWithAlignment_WhenAlignmentVaries_AppliesCanvasOffsets()
    {
        // Arrange
        CoordinateTools tools = new();
        Hint hint = new() { Text = "A", FontSize = 20, XCoordinate = 100 };

        // Act
        float center = tools.GetXCoordinateWithAlignment(hint, HintAlignment.Center);
        float left = tools.GetXCoordinateWithAlignment(hint, HintAlignment.Left);
        float right = tools.GetXCoordinateWithAlignment(hint, HintAlignment.Right);

        // Assert
        Assert.IsTrue(left < center);
        Assert.IsTrue(right > center);
    }

    [TestMethod]
    [Ignore("Behavior changed. Now CoordianteTools shouldn't throw when line height is negative.")]
    public void GetTextHeight_WhenLineHeightIsNegative_Throws()
    {
        // Arrange
        CoordinateTools tools = new();

        // Act & Assert
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => tools.GetTextHeight("x", 20, -1));
    }

    [TestMethod]
    public void GetTextHeight_WhenLineHeightIsNegative_DontThrow()
    {
        // Arrange
        CoordinateTools tools = new();

        // Act
        float height = tools.GetTextHeight("x", 20, -1);

        // Assert
        Assert.IsTrue(height > 0);
    }

    [TestMethod]
    public void GetLineInfos_WhenParsingCompletes_ReturnsParserToPool()
    {
        // Arrange
        RecordingPool<RichTextParser> pool = new(() => new RichTextParser());
        CoordinateTools tools = new(pool);

        // Act
        IReadOnlyList<LineInfo> lines = tools.GetLineInfos("abc", 20);

        // Assert
        Assert.IsTrue(lines.Count > 0);
        Assert.AreEqual(1, pool.RentCount);
        Assert.AreEqual(1, pool.ReturnCount);
    }
}
