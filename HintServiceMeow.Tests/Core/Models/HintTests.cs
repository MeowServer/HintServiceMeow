using System.Collections.Generic;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.HintContent;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Tests.Core.Utilities.TestDoubles;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Models;

[TestClass]
public class HintTests
{
    [TestMethod]
    public void Constructor_WhenDefault_HasExpectedDefaults()
    {
        // Arrange & Act
        Hint hint = new();

        // Assert
        Assert.AreEqual(0f, hint.XCoordinate);
        Assert.AreEqual(700f, hint.YCoordinate);
        Assert.AreEqual(HintAlignment.Center, hint.Alignment);
        Assert.AreEqual(HintVerticalAlign.Middle, hint.YCoordinateAlign);
        Assert.AreEqual(20, hint.FontSize);
        Assert.IsFalse(hint.PreserveCase);
    }

    [TestMethod]
    public void CopyConstructor_WhenSourceProvided_ClonesMutableStateButUsesNewGuid()
    {
        // Arrange
        Hint source = new()
        {
            Id = "id",
            XCoordinate = 12,
            YCoordinate = 34,
            Alignment = HintAlignment.Left,
            YCoordinateAlign = HintVerticalAlign.Top,
            PreserveCase = true,
            Text = "text"
        };

        // Act
        Hint copy = new(source);

        // Assert
        Assert.AreNotEqual(source.Guid, copy.Guid);
        Assert.AreEqual(source.XCoordinate, copy.XCoordinate);
        Assert.AreEqual(source.YCoordinate, copy.YCoordinate);
        Assert.AreEqual(source.Alignment, copy.Alignment);
        Assert.AreEqual(source.Text, copy.Text);
        Assert.AreEqual(source.PreserveCase, copy.PreserveCase);
    }

    [TestMethod]
    public void PropertySetters_WhenValueChanges_NotifiesAndInvokesAnalyser()
    {
        // Arrange
        Hint hint = new();
        FixedUpdateAnalyser analyser = new();
        hint.UpdateAnalyser = analyser;
        List<string> props = [];
        hint.PropertyChanged += (_, e) => props.Add(e.PropertyName!);

        // Act
        hint.XCoordinate = 10;
        hint.YCoordinate = 20;
        hint.PreserveCase = true;

        // Assert
        CollectionAssert.Contains(props, "XCoordinate");
        CollectionAssert.Contains(props, "YCoordinate");
        CollectionAssert.Contains(props, "PreserveCase");
        Assert.AreEqual(3, analyser.OnUpdateCallCount);
    }

    [TestMethod]
    public void TextAndAutoText_WhenSet_SwitchesContentImplementation()
    {
        // Arrange
        Hint hint = new();

        // Act
        hint.Text = "manual";

        // Assert
        Assert.IsInstanceOfType<StringContent>(hint.Content);

        // Act
        hint.AutoText = _ => "auto";

        // Assert
        Assert.IsInstanceOfType<AutoContent>(hint.Content);
        Assert.IsNotNull(hint.AutoText);
    }

    [TestMethod]
    public void ContentReplacement_WhenNewContentSet_UnsubscribesOldAndSubscribesNew()
    {
        // Arrange
        Hint hint = new();
        StringContent oldContent = new("a");
        StringContent newContent = new("b");
        hint.Content = oldContent;

        int changed = 0;
        hint.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == "Content")
                changed++;
        };

        // Act
        hint.Content = newContent;
        oldContent.Text = "x";
        newContent.Text = "y";

        // Assert
        Assert.AreEqual(2, changed); // set + new content update only
    }

    [TestMethod]
    public void ResetFields_WhenHintReturnsToPool_ClearsPreserveCase()
    {
        // Arrange
        Hint hint = new() { PreserveCase = true };

        // Act
        hint.ResetFields();

        // Assert
        Assert.IsFalse(hint.PreserveCase);
    }
}
