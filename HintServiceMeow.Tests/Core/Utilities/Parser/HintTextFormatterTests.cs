using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.HintContent;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Utilities.Parser;

[TestClass]
public class HintTextFormatterTests
{
    [TestMethod]
    public void GetText_WhenPreserveCaseIsDisabled_ReturnsOriginalContent()
    {
        // Arrange
        string text = new("Hello".ToCharArray());
        Hint hint = new() { Text = text };

        // Act
        string result = HintTextFormatter.GetText(hint);

        // Assert
        Assert.AreSame(text, result);
    }

    [TestMethod]
    public void PreserveCase_WhenMixedCase_WrapsOnlyLowercaseSpans()
    {
        // Act
        string result = HintTextFormatter.PreserveCase("Hello WORLD - camelCase 123");

        // Assert
        Assert.AreEqual(
            "H<lowercase>ello </lowercase>WORLD - <lowercase>camel</lowercase>C<lowercase>ase 123</lowercase>",
            result);
    }

    [TestMethod]
    public void PreserveCase_WhenRichTextIsPresent_LeavesTagsUnchanged()
    {
        // Act
        string result = HintTextFormatter.PreserveCase("<color=#fff>Hello</color>");

        // Assert
        Assert.AreEqual("<color=#fff>H<lowercase>ello</lowercase></color>", result);
    }

    [TestMethod]
    public void PreserveCase_WhenExplicitCaseTagsArePresent_LeavesTheirContentAuthoritative()
    {
        // Arrange
        string text = "<uppercase>Do not change</uppercase> then <smallcaps>Keep me</smallcaps>";

        // Act
        string result = HintTextFormatter.PreserveCase(text);

        // Assert
        Assert.AreEqual(
            "<uppercase>Do not change</uppercase> <lowercase>then </lowercase><smallcaps>Keep me</smallcaps>",
            result);
    }

    [TestMethod]
    public void PreserveCase_WhenExplicitCaseTagsAreNested_ResumesAfterTheOuterScopeCloses()
    {
        // Arrange
        string text = "pre <uppercase>A <lowercase>Bc</lowercase> d</uppercase> post";

        // Act
        string result = HintTextFormatter.PreserveCase(text);

        // Assert
        Assert.AreEqual(
            "<lowercase>pre </lowercase><uppercase>A <lowercase>Bc</lowercase> d</uppercase> <lowercase>post</lowercase>",
            result);
    }

    [TestMethod]
    public void PreserveCase_WhenNoParseIsPresent_DoesNotInjectMarkupIntoLiteralContent()
    {
        // Arrange
        string text = "before <noparse><b>literal lower</b></noparse> after";

        // Act
        string result = HintTextFormatter.PreserveCase(text);

        // Assert
        Assert.AreEqual(
            "<lowercase>before </lowercase><noparse><b>literal lower</b></noparse> <lowercase>after</lowercase>",
            result);
    }

    [TestMethod]
    public void PreserveCase_WhenUnicodeTextIsPresent_PreservesCasedLettersAndLeavesCjkUnchanged()
    {
        // Act
        string result = HintTextFormatter.PreserveCase("École 中文 Привет");

        // Assert
        Assert.AreEqual(
            "É<lowercase>cole 中文 </lowercase>П<lowercase>ривет</lowercase>",
            result);
    }

    [TestMethod]
    public void PreserveCase_WhenNothingNeedsChanging_ReturnsOriginalInstance()
    {
        // Arrange
        string text = new("ABC 中文 123".ToCharArray());

        // Act
        string result = HintTextFormatter.PreserveCase(text);

        // Assert
        Assert.AreSame(text, result);
    }

    [TestMethod]
    public void PreserveCase_WhenSelfClosingCaseTagIsPresent_DoesNotTreatItAsAnOpenScope()
    {
        // Act
        string result = HintTextFormatter.PreserveCase("<lowercase/>after");

        // Assert
        Assert.AreEqual("<lowercase/><lowercase>after</lowercase>", result);
    }

    [TestMethod]
    public void GetText_WhenAutoContentIsUsed_FormatsItsCurrentValueWithoutMutatingIt()
    {
        // Arrange
        AutoContent content = new(_ => "Hello");
        content.TryUpdate(new ContentUpdateArg(null!, null!));
        Hint hint = new() { Content = content, PreserveCase = true };

        // Act
        string result = HintTextFormatter.GetText(hint);

        // Assert
        Assert.AreEqual("H<lowercase>ello</lowercase>", result);
        Assert.AreEqual("Hello", content.GetText());
    }

    [TestMethod]
    public void PreserveCase_WhenTransformedTextRepeats_ReusesCachedResult()
    {
        // Arrange
        string firstInput = new("cache me uniquely".ToCharArray());
        string secondInput = new("cache me uniquely".ToCharArray());

        // Act
        string firstResult = HintTextFormatter.PreserveCase(firstInput);
        string secondResult = HintTextFormatter.PreserveCase(secondInput);

        // Assert
        Assert.AreSame(firstResult, secondResult);
    }

    [TestMethod]
    public void PreserveCase_WhenUnknownAngleBracketTokenIsPresent_CopiesItUnchanged()
    {
        // Act
        string result = HintTextFormatter.PreserveCase("before <player lowercase> after");

        // Assert
        Assert.AreEqual(
            "<lowercase>before </lowercase><player lowercase> <lowercase>after</lowercase>",
            result);
    }
}
