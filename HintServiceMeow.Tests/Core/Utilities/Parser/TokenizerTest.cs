// ─────────────────────────────────────────────────────────────────────────────
// TokenizerTests.cs
//
// Comprehensive test suite for HintServiceMeow.Core.Utilities.Parser.Tokenizer
//
// ══ BUG IDENTIFIED IN PRODUCTION CODE ═══════════════════════════════════════
//
//  BUG  (TryHandleRichTag – tagParameter extracted via TryMatchValidTag)
//
//       In TryHandleRichTag, the tag value (the part after '=') is extracted
//       using:
//
//           tagParameter = TagChecker.TryMatchValidTag(
//               rawText, equalSignIndex + 1, tagEnd - equalSignIndex);
//
//       TryMatchValidTag scans through KnownTags[] and returns the matching
//       entry only if the substring matches a KNOWN TAG NAME such as "b",
//       "color", "size", etc.
//
//       Tag values are arbitrary strings ("red", "32", "#FF0080", "Arial",
//       "left", "center", etc.) and are almost never valid tag names.
//       The result is that TagValue is null for virtually every tag that
//       carries a value.
//
//       EXCEPTIONS (accidentally correct):
//         <size=b>        → TagValue="b"   (because "b" is a known tag)
//         <size=s>        → TagValue="s"   (because "s" is a known tag)
//         <font=size>     → TagValue=null  ("size" is NOT length-matched to
//                                           4-char candidates "font","mark",
//                                           "nobr","link"; actually "size" has
//                                           4 chars so it matches "size" → "size"!)
//         <size=font>     → TagValue="font" (because "font" is a known tag)
//
//       All tests that assert TagValue assert the CORRECT (expected) value.
//       Any test whose name includes "TagValue" that fails is a BUG-regression.
//
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using ParserToken = HintServiceMeow.Core.Models.Parser.Token;


namespace HintServiceMeow.Tests.Core.Utilities.Parser;

[TestClass]
public class TokenizerTests
{
    // ═════════════════════════════════════════════════════════════════════════
    // ① Diagnostic dump helpers
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Converts a <see cref="List{Token}"/> into a multi-line diagnostic string
    /// that is embedded in every Assert failure message.
    /// </summary>
    private static string Dump(List<ParserToken> tokens, string input)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("══ Tokenizer result dump ════════════════════════════════════");
        sb.AppendLine($"  Input   : \"{Escape(input)}\"");
        sb.AppendLine($"  # Tokens: {tokens.Count}");
        for (int i = 0; i < tokens.Count; i++)
        {
            ParserToken t = tokens[i];
            sb.Append($"  [{i}] Type={t.Type,-14}");
            switch (t.Type)
            {
                case RichTextTokenType.Text:
                    sb.Append($"  Text=\"{Escape(t.Text ?? "")}\"");
                    break;
                case RichTextTokenType.OpenTag:
                case RichTextTokenType.CloseTag:
                case RichTextTokenType.SelfCloseTag:
                    sb.Append($"  TagName={t.TagName ?? "null",-12}");
                    sb.Append($"  TagValue={t.TagValue ?? "null"}");
                    break;
                case RichTextTokenType.Parameter:
                    sb.Append($"  Parameter={t.Parameter?.GetType().Name ?? "null"}");
                    break;
                case RichTextTokenType.LineBreak:
                    sb.Append("  (line break)");
                    break;
            }
            sb.AppendLine();
        }
        sb.AppendLine("══════════════════════════════════════════════════════════════");
        return sb.ToString();
    }

    private static string Escape(string s) =>
        s.Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");

    // ═════════════════════════════════════════════════════════════════════════
    // ② Helpers
    // ═════════════════════════════════════════════════════════════════════════

    private static Tokenizer NewTokenizer() => new Tokenizer();

    private static Tuple<string, IParameter>[] NoParams =>
        Array.Empty<Tuple<string, IParameter>>();

    private List<ParserToken> Tokenize(string input,
        Tuple<string, IParameter>[]? parameters = null) =>
        NewTokenizer().Tokenize(input, parameters ?? NoParams);

    private sealed class StubParam : IParameter
    {
        public global::Hints.HintParameter GetScpslHintParameter() =>
            throw new NotSupportedException();
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 1 – Plain text
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_EmptyString_ReturnsNoTokens()
    {
        const string input = "";
        var tokens = Tokenize(input);

        Assert.AreEqual(0, tokens.Count,
            $"Empty string must produce 0 tokens, got {tokens.Count}." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_PlainText_ReturnsSingleTextToken()
    {
        const string input = "hello world";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"Plain text must produce 1 token, got {tokens.Count}." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Text, tokens[0].Type,
            $"Token[0] must be Text." + Dump(tokens, input));
        Assert.AreEqual("hello world", tokens[0].Text,
            $"Token[0].Text must be \"hello world\", got \"{tokens[0].Text}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_PlainText_TagNameAndParameterAreNull()
    {
        const string input = "abc";
        var tokens = Tokenize(input);

        Assert.IsNull(tokens[0].TagName,
            $"Text token must have TagName=null." + Dump(tokens, input));
        Assert.IsNull(tokens[0].TagValue,
            $"Text token must have TagValue=null." + Dump(tokens, input));
        Assert.IsNull(tokens[0].Parameter,
            $"Text token must have Parameter=null." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 2 – Line breaks
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_NewlineCharacter_ProducesLineBreakToken()
    {
        const string input = "\n";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"Single newline must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.LineBreak, tokens[0].Type,
            $"Token[0] must be LineBreak." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_NewlineBetweenText_ProducesThreeTokens()
    {
        const string input = "A\nB";
        var tokens = Tokenize(input);

        Assert.AreEqual(3, tokens.Count,
            $"\"A\\nB\" must produce 3 tokens (Text, LineBreak, Text), got {tokens.Count}." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Text, tokens[0].Type, "Token[0] must be Text." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.LineBreak, tokens[1].Type, "Token[1] must be LineBreak." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Text, tokens[2].Type, "Token[2] must be Text." + Dump(tokens, input));
        Assert.AreEqual("A", tokens[0].Text, $"Token[0].Text must be \"A\", got \"{tokens[0].Text}\"." + Dump(tokens, input));
        Assert.AreEqual("B", tokens[2].Text, $"Token[2].Text must be \"B\", got \"{tokens[2].Text}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_MultipleNewlines_ProducesMultipleLineBreakTokens()
    {
        const string input = "A\n\nB";
        var tokens = Tokenize(input);

        int lineBreakCount = tokens.Count(t => t.Type == RichTextTokenType.LineBreak);
        Assert.AreEqual(2, lineBreakCount,
            $"Two newlines must produce 2 LineBreak tokens, got {lineBreakCount}." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_BrTag_ProducesLineBreakToken()
    {
        const string input = "A<br>B";
        var tokens = Tokenize(input);

        Assert.IsTrue(tokens.Any(t => t.Type == RichTextTokenType.LineBreak),
            $"<br> must produce a LineBreak token." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_BrTag_CaseInsensitive()
    {
        const string input = "<BR>";
        var tokens = Tokenize(input);

        Assert.IsTrue(tokens.Any(t => t.Type == RichTextTokenType.LineBreak),
            $"<BR> (uppercase) must produce a LineBreak token." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_BrTag_ProducesNoOpenTagToken()
    {
        const string input = "<br>";
        var tokens = Tokenize(input);

        bool hasOpenOrClose = tokens.Any(t =>
            t.Type == RichTextTokenType.OpenTag ||
            t.Type == RichTextTokenType.CloseTag ||
            t.Type == RichTextTokenType.SelfCloseTag);

        Assert.IsFalse(hasOpenOrClose,
            $"<br> must produce only a LineBreak (no Open/Close/SelfClose token)." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_EscapeBackslashN_ProducesLineBreakToken()
    {
        // Literal backslash followed by 'n' in source code = two characters \n
        const string input = @"A\nB";
        var tokens = Tokenize(input);

        Assert.IsTrue(tokens.Any(t => t.Type == RichTextTokenType.LineBreak),
            $@"The escape \n must be treated as a line break." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_EscapeBackslashN_TextSegmentsBoundedCorrectly()
    {
        const string input = @"X\nY";
        var tokens = Tokenize(input);

        var textTokens = tokens.Where(t => t.Type == RichTextTokenType.Text).ToList();
        Assert.IsTrue(textTokens.Any(t => t.Text == "X"),
            $@"Text before \n must be ""X""." + Dump(tokens, input));
        Assert.IsTrue(textTokens.Any(t => t.Text == "Y"),
            $@"Text after \n must be ""Y""." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_EscapeDoubleBackslash_ProducesSingleBackslashText()
    {
        const string input = @"A\\B";   // source: A\\B  (4 chars)
        var tokens = Tokenize(input);

        // The tokenizer collapses \\ into a single \ in the output text segment
        var textToken = tokens.FirstOrDefault(t => t.Type == RichTextTokenType.Text);
        Assert.IsNotNull(textToken.Text,
            $"Expected at least one text token." + Dump(tokens, input));
        StringAssert.Contains(textToken.Text, "\\",
            $@"Escape \\ must produce a literal backslash in text." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 3 – Open tags (no value)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_BoldTag_ProducesOpenTagToken()
    {
        const string input = "<b>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"<b> must produce exactly 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.OpenTag, tokens[0].Type,
            $"Token[0] must be OpenTag." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_BoldTag_TagNameIsLowercaseB()
    {
        const string input = "<b>";
        var tokens = Tokenize(input);

        Assert.AreEqual("b", tokens[0].TagName,
            $"<b> TagName must be \"b\", got \"{tokens[0].TagName}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_BoldTag_TagValueIsNull()
    {
        const string input = "<b>";
        var tokens = Tokenize(input);

        Assert.IsNull(tokens[0].TagValue,
            $"<b> (no value) TagValue must be null, got \"{tokens[0].TagValue}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_BoldTag_CaseInsensitive()
    {
        const string input = "<B>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"<B> must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.OpenTag, tokens[0].Type,
            $"<B> must produce an OpenTag." + Dump(tokens, input));
        Assert.AreEqual("b", tokens[0].TagName,
            $"<B> TagName must be normalized to \"b\"." + Dump(tokens, input));
    }

    [TestMethod]
    [DataRow("<i>")]
    [DataRow("<u>")]
    [DataRow("<s>")]
    [DataRow("<sub>")]
    [DataRow("<sup>")]
    [DataRow("<b>")]
    [DataRow("<noparse>")]
    [DataRow("<nobr>")]
    [DataRow("<smallcaps>")]
    [DataRow("<uppercase>")]
    [DataRow("<lowercase>")]
    [DataRow("<allcaps>")]
    public void Tokenize_ValuelessOpenTag_ProducesOpenTagWithNullValue(string input)
    {
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"Input \"{input}\" must produce exactly 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.OpenTag, tokens[0].Type,
            $"Input \"{input}\" must be an OpenTag." + Dump(tokens, input));
        Assert.IsNotNull(tokens[0].TagName,
            $"Input \"{input}\" TagName must not be null." + Dump(tokens, input));
        Assert.IsNull(tokens[0].TagValue,
            $"Input \"{input}\" TagValue must be null (no '=' present), got \"{tokens[0].TagValue}\"." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 4 – Open tags with values  (BUG regressions)
    //
    // Each test below asserts that TagValue equals the raw value string from
    // the input.  These will FAIL until the BUG is fixed:
    //   tagParameter = TagChecker.TryMatchValidTag(...)   ← must be raw substring
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_ColorTagNamedRed_TagValueIsRed()
    {
        const string input = "<color=red>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"<color=red> must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.OpenTag, tokens[0].Type,
            $"Token must be OpenTag." + Dump(tokens, input));
        Assert.AreEqual("color", tokens[0].TagName,
            $"TagName must be \"color\", got \"{tokens[0].TagName}\"." + Dump(tokens, input));
        Assert.AreEqual("red", tokens[0].TagValue,
            $"BUG: TagValue must be \"red\", got \"{tokens[0].TagValue ?? "null"}\".\n" +
            $"(TryMatchValidTag is being called on the value; \"red\" is not a known tag name so it returns null.)" +
            Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_ColorTagHex_TagValueIsHexString()
    {
        const string input = "<color=#FF0080>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"<color=#FF0080> must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual("#FF0080", tokens[0].TagValue,
            $"BUG: TagValue must be \"#FF0080\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_SizeTagNumericValue_TagValueIsNumberString()
    {
        const string input = "<size=32>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"<size=32> must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.OpenTag, tokens[0].Type,
            $"Token must be OpenTag." + Dump(tokens, input));
        Assert.AreEqual("size", tokens[0].TagName,
            $"TagName must be \"size\"." + Dump(tokens, input));
        Assert.AreEqual("32", tokens[0].TagValue,
            $"BUG: TagValue must be \"32\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_SizeTagEmValue_TagValueIsEmString()
    {
        const string input = "<size=2em>";
        var tokens = Tokenize(input);

        Assert.AreEqual("2em", tokens[0].TagValue,
            $"BUG: TagValue must be \"2em\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_SizeTagPercentValue_TagValueIsPercentString()
    {
        const string input = "<size=150%>";
        var tokens = Tokenize(input);

        Assert.AreEqual("150%", tokens[0].TagValue,
            $"BUG: TagValue must be \"150%\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_AlignTagLeftValue_TagValueIsLeft()
    {
        const string input = "<align=left>";
        var tokens = Tokenize(input);

        Assert.AreEqual("left", tokens[0].TagValue,
            $"BUG: TagValue must be \"left\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_AlignTagCenterValue_TagValueIsCenter()
    {
        const string input = "<align=center>";
        var tokens = Tokenize(input);

        Assert.AreEqual("center", tokens[0].TagValue,
            $"BUG: TagValue must be \"center\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_AlignTagRightValue_TagValueIsRight()
    {
        const string input = "<align=right>";
        var tokens = Tokenize(input);

        Assert.AreEqual("right", tokens[0].TagValue,
            $"BUG: TagValue must be \"right\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_LineHeightTagPixelValue_TagValueIsPixelString()
    {
        const string input = "<line-height=30px>";
        var tokens = Tokenize(input);

        Assert.AreEqual("30px", tokens[0].TagValue,
            $"BUG: TagValue must be \"30px\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_LineIndentTagValue_TagValueIsNumberString()
    {
        const string input = "<line-indent=10>";
        var tokens = Tokenize(input);

        Assert.AreEqual("10", tokens[0].TagValue,
            $"BUG: TagValue must be \"10\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_MarginTagValue_TagValueIsNumberString()
    {
        const string input = "<margin=20>";
        var tokens = Tokenize(input);

        Assert.AreEqual("20", tokens[0].TagValue,
            $"BUG: TagValue must be \"20\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_IndentTagValue_TagValueIsNumberString()
    {
        const string input = "<indent=15>";
        var tokens = Tokenize(input);

        Assert.AreEqual("15", tokens[0].TagValue,
            $"BUG: TagValue must be \"15\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_FontTagArbitraryName_TagValueIsFontName()
    {
        const string input = "<font=Arial>";
        var tokens = Tokenize(input);

        Assert.AreEqual("Arial", tokens[0].TagValue,
            $"BUG: TagValue must be \"Arial\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_RotateTagValue_TagValueIsNumberString()
    {
        const string input = "<rotate=45>";
        var tokens = Tokenize(input);

        Assert.AreEqual("45", tokens[0].TagValue,
            $"BUG: TagValue must be \"45\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_VOffsetTagValue_TagValueIsNumberString()
    {
        const string input = "<voffset=5>";
        var tokens = Tokenize(input);

        Assert.AreEqual("5", tokens[0].TagValue,
            $"BUG: TagValue must be \"5\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_WidthTagValue_TagValueIsNumberString()
    {
        const string input = "<width=400>";
        var tokens = Tokenize(input);

        Assert.AreEqual("400", tokens[0].TagValue,
            $"BUG: TagValue must be \"400\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_MarkTagValue_TagValueIsColorString()
    {
        const string input = "<mark=yellow>";
        var tokens = Tokenize(input);

        Assert.AreEqual("yellow", tokens[0].TagValue,
            $"BUG: TagValue must be \"yellow\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_CSpaceTagValue_TagValueIsNumberString()
    {
        const string input = "<cspace=3>";
        var tokens = Tokenize(input);

        Assert.AreEqual("3", tokens[0].TagValue,
            $"BUG: TagValue must be \"3\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_MSpaceTagValue_TagValueIsNumberString()
    {
        const string input = "<mspace=10>";
        var tokens = Tokenize(input);

        Assert.AreEqual("10", tokens[0].TagValue,
            $"BUG: TagValue must be \"10\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_FontWeightTagValue_TagValueIsNumberString()
    {
        const string input = "<font-weight=700>";
        var tokens = Tokenize(input);

        Assert.AreEqual("700", tokens[0].TagValue,
            $"BUG: TagValue must be \"700\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    // ─── Accidental "correct" behavior via tag-name collision ───────────────
    // These document cases where the value happens to equal a known tag name,
    // so TryMatchValidTag accidentally returns a non-null result.
    // They should still produce the correct TagValue string.

    [TestMethod]
    public void Tokenize_SizeTagValueB_TagValueIsB()
    {
        // "b" is a known tag, so TryMatchValidTag currently returns "b" here —
        // this is accidentally correct but not by design.
        const string input = "<size=b>";
        var tokens = Tokenize(input);

        Assert.AreEqual("b", tokens[0].TagValue,
            $"TagValue must be \"b\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_SizeTagValueS_TagValueIsS()
    {
        const string input = "<size=s>";
        var tokens = Tokenize(input);

        Assert.AreEqual("s", tokens[0].TagValue,
            $"TagValue must be \"s\" (accidental tag-name collision), got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 5 – Close tags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_CloseBoldTag_ProducesCloseTagToken()
    {
        const string input = "</b>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"</b> must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.CloseTag, tokens[0].Type,
            $"Token must be CloseTag." + Dump(tokens, input));
        Assert.AreEqual("b", tokens[0].TagName,
            $"TagName must be \"b\", got \"{tokens[0].TagName}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_CloseColorTag_ProducesCloseTagWithColorName()
    {
        const string input = "</color>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"</color> must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.CloseTag, tokens[0].Type,
            $"Token must be CloseTag." + Dump(tokens, input));
        Assert.AreEqual("color", tokens[0].TagName,
            $"TagName must be \"color\", got \"{tokens[0].TagName}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_CloseTag_TagValueIsAlwaysNull()
    {
        // Close tags never carry a value
        const string input = "</size>";
        var tokens = Tokenize(input);

        Assert.IsNull(tokens[0].TagValue,
            $"Close tag TagValue must always be null, got \"{tokens[0].TagValue}\"." + Dump(tokens, input));
    }

    [TestMethod]
    [DataRow("</b>", "b")]
    [DataRow("</i>", "i")]
    [DataRow("</u>", "u")]
    [DataRow("</s>", "s")]
    [DataRow("</sub>", "sub")]
    [DataRow("</sup>", "sup")]
    [DataRow("</color>", "color")]
    [DataRow("</size>", "size")]
    [DataRow("</align>", "align")]
    [DataRow("</noparse>", "noparse")]
    [DataRow("</nobr>", "nobr")]
    [DataRow("</smallcaps>", "smallcaps")]
    [DataRow("</uppercase>", "uppercase")]
    [DataRow("</lowercase>", "lowercase")]
    [DataRow("</line-height>", "line-height")]
    [DataRow("</line-indent>", "line-indent")]
    [DataRow("</margin>", "margin")]
    [DataRow("</indent>", "indent")]
    [DataRow("</font>", "font")]
    [DataRow("</font-weight>", "font-weight")]
    [DataRow("</mark>", "mark")]
    [DataRow("</rotate>", "rotate")]
    [DataRow("</voffset>", "voffset")]
    [DataRow("</width>", "width")]
    [DataRow("</cspace>", "cspace")]
    [DataRow("</mspace>", "mspace")]
    public void Tokenize_CloseTag_TagNameParsedCorrectly(string input, string expectedName)
    {
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"Input \"{input}\" must produce 1 token, got {tokens.Count}." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.CloseTag, tokens[0].Type,
            $"Input \"{input}\" must be a CloseTag." + Dump(tokens, input));
        Assert.AreEqual(expectedName, tokens[0].TagName,
            $"Input \"{input}\" TagName must be \"{expectedName}\", got \"{tokens[0].TagName}\"." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 6 – Self-closing tags  (alpha, space, pos, sprite)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_AlphaTag_ProducesSelfCloseTagToken()
    {
        const string input = "<alpha=#FF>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"<alpha=#FF> must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.SelfCloseTag, tokens[0].Type,
            $"Token must be SelfCloseTag." + Dump(tokens, input));
        Assert.AreEqual("alpha", tokens[0].TagName,
            $"TagName must be \"alpha\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_AlphaTag_TagValueIsHexString()
    {
        const string input = "<alpha=#FF>";
        var tokens = Tokenize(input);

        Assert.AreEqual("#FF", tokens[0].TagValue,
            $"BUG: TagValue must be \"#FF\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_AlphaTagZero_TagValueIsZeroHex()
    {
        const string input = "<alpha=#00>";
        var tokens = Tokenize(input);

        Assert.AreEqual("#00", tokens[0].TagValue,
            $"BUG: TagValue must be \"#00\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_SpaceTag_ProducesSelfCloseTagToken()
    {
        const string input = "<space=20>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"<space=20> must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.SelfCloseTag, tokens[0].Type,
            $"Token must be SelfCloseTag." + Dump(tokens, input));
        Assert.AreEqual("space", tokens[0].TagName,
            $"TagName must be \"space\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_SpaceTag_TagValueIsNumberString()
    {
        const string input = "<space=20>";
        var tokens = Tokenize(input);

        Assert.AreEqual("20", tokens[0].TagValue,
            $"BUG: TagValue must be \"20\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_PosTag_ProducesSelfCloseTagToken()
    {
        const string input = "<pos=50>";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"<pos=50> must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.SelfCloseTag, tokens[0].Type,
            $"Token must be SelfCloseTag." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_PosTag_TagValueIsNumberString()
    {
        const string input = "<pos=50>";
        var tokens = Tokenize(input);

        Assert.AreEqual("50", tokens[0].TagValue,
            $"BUG: TagValue must be \"50\", got \"{tokens[0].TagValue ?? "null"}\"." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 7 – Token sequences (mixed input)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_TextWrappedInBoldTags_ProducesThreeTokens()
    {
        const string input = "<b>hello</b>";
        var tokens = Tokenize(input);

        Assert.AreEqual(3, tokens.Count,
            $"<b>hello</b> must produce 3 tokens, got {tokens.Count}." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.OpenTag, tokens[0].Type, "Token[0] must be OpenTag." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Text, tokens[1].Type, "Token[1] must be Text." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.CloseTag, tokens[2].Type, "Token[2] must be CloseTag." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_NestedTags_TokenOrderIsCorrect()
    {
        const string input = "<b><i>text</i></b>";
        var tokens = Tokenize(input);

        Assert.AreEqual(5, tokens.Count,
            $"<b><i>text</i></b> must produce 5 tokens." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.OpenTag, tokens[0].Type, "Token[0] must be OpenTag <b>." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.OpenTag, tokens[1].Type, "Token[1] must be OpenTag <i>." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Text, tokens[2].Type, "Token[2] must be Text." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.CloseTag, tokens[3].Type, "Token[3] must be CloseTag </i>." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.CloseTag, tokens[4].Type, "Token[4] must be CloseTag </b>." + Dump(tokens, input));
        Assert.AreEqual("b", tokens[0].TagName, "Token[0] must be tag \"b\"." + Dump(tokens, input));
        Assert.AreEqual("i", tokens[1].TagName, "Token[1] must be tag \"i\"." + Dump(tokens, input));
        Assert.AreEqual("i", tokens[3].TagName, "Token[3] must be close \"i\"." + Dump(tokens, input));
        Assert.AreEqual("b", tokens[4].TagName, "Token[4] must be close \"b\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_TextAroundTag_TextTokensPreserved()
    {
        const string input = "before<b>after";
        var tokens = Tokenize(input);

        Assert.AreEqual(3, tokens.Count,
            $"Must produce 3 tokens (Text, Open, Text)." + Dump(tokens, input));
        Assert.AreEqual("before", tokens[0].Text,
            $"Token[0].Text must be \"before\"." + Dump(tokens, input));
        Assert.AreEqual("after", tokens[2].Text,
            $"Token[2].Text must be \"after\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_MultiLineWithTags_CorrectTokenSequence()
    {
        const string input = "<b>line1</b>\nline2";
        var tokens = Tokenize(input);

        // Expected: Open(<b>), Text("line1"), Close(</b>), LineBreak, Text("line2")
        Assert.AreEqual(5, tokens.Count,
            $"Must produce 5 tokens." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.OpenTag, tokens[0].Type, "Token[0] = OpenTag." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Text, tokens[1].Type, "Token[1] = Text." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.CloseTag, tokens[2].Type, "Token[2] = CloseTag." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.LineBreak, tokens[3].Type, "Token[3] = LineBreak." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Text, tokens[4].Type, "Token[4] = Text." + Dump(tokens, input));
        Assert.AreEqual("line1", tokens[1].Text, "Token[1].Text must be \"line1\"." + Dump(tokens, input));
        Assert.AreEqual("line2", tokens[4].Text, "Token[4].Text must be \"line2\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_ConsecutiveTextChunks_MergedIntoSingleTextToken()
    {
        // No tags or breaks → entire string is one Text token
        const string input = "abcdef";
        var tokens = Tokenize(input);

        Assert.AreEqual(1, tokens.Count,
            $"Continuous plain text must be one token." + Dump(tokens, input));
        Assert.AreEqual("abcdef", tokens[0].Text,
            $"Token text mismatch." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 8 – Unknown / malformed tags treated as literal text
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_UnknownTag_NotEmittedAsTagToken()
    {
        const string input = "<thisisnotavalidtag>text</thisisnotavalidtag>";
        var tokens = Tokenize(input);

        bool hasTagToken = tokens.Any(t =>
            t.Type is RichTextTokenType.OpenTag or RichTextTokenType.CloseTag or RichTextTokenType.SelfCloseTag);

        Assert.IsFalse(hasTagToken,
            $"Unknown tag must not produce any tag tokens." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_UnknownTag_ContentStillProducesTextToken()
    {
        const string input = "<foobar>hello</foobar>";
        var tokens = Tokenize(input);

        bool hasTextToken = tokens.Any(t => t.Type == RichTextTokenType.Text && t.Text.Contains("hello"));
        Assert.IsTrue(hasTextToken,
            $"The text \"hello\" between unknown tags must still appear as a Text token." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_MalformedTagNoClosingBracket_TreatedAsLiteralText()
    {
        // "<b" without closing ">" — must not produce a tag token
        const string input = "<b text";
        var tokens = Tokenize(input);

        bool hasTagToken = tokens.Any(t =>
            t.Type is RichTextTokenType.OpenTag or RichTextTokenType.CloseTag or RichTextTokenType.SelfCloseTag);

        Assert.IsFalse(hasTagToken,
            $"Unclosed '<' must not produce a tag token." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_MalformedTagNoClosingBracket_ProducesTextToken()
    {
        const string input = "<b text";
        var tokens = Tokenize(input);

        // The characters should still appear as text (the '<' itself and "b text")
        bool hasText = tokens.Any(t => t.Type == RichTextTokenType.Text);
        Assert.IsTrue(hasText,
            $"Content of malformed tag must appear as text." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_EmptyAngleBrackets_TreatedAsLiteralText()
    {
        const string input = "<>";
        var tokens = Tokenize(input);
        // <> contains nothing; TryMatchValidTag on empty string returns null → treated as text
        Assert.IsFalse(tokens.Any(t =>
            t.Type is RichTextTokenType.OpenTag or RichTextTokenType.CloseTag or RichTextTokenType.SelfCloseTag),
            $"Empty angle brackets <> must not produce a tag token." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 9 – Parameters
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_MatchingParameter_ProducesParameterToken()
    {
        var stub = new StubParam();
        var @params = new[] { Tuple.Create<string, IParameter>("foo", stub) };
        const string input = "{foo}";

        var tokens = Tokenize(input, @params);

        Assert.AreEqual(1, tokens.Count,
            $"{{foo}} with matching param must produce 1 token." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Parameter, tokens[0].Type,
            $"Token must be Parameter." + Dump(tokens, input));
        Assert.AreSame(stub, tokens[0].Parameter,
            $"Token.Parameter must be the registered StubParam instance." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_MatchingParameter_CaseInsensitive()
    {
        var stub = new StubParam();
        var @params = new[] { Tuple.Create<string, IParameter>("FOO", stub) };
        const string input = "{foo}";

        var tokens = Tokenize(input, @params);

        Assert.AreEqual(RichTextTokenType.Parameter, tokens[0].Type,
            $"Parameter matching must be case-insensitive." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_UnrecognisedParameter_TreatedAsLiteralText()
    {
        const string input = "{unknown}";
        var tokens = Tokenize(input, NoParams);

        Assert.IsFalse(tokens.Any(t => t.Type == RichTextTokenType.Parameter),
            $"Unrecognised {{unknown}} must not produce a Parameter token." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_TwoParameters_BothCapturedInOrder()
    {
        var p0 = new StubParam();
        var p1 = new StubParam();
        var @params = new[]
        {
            Tuple.Create<string, IParameter>("a", p0),
            Tuple.Create<string, IParameter>("b", p1),
        };
        const string input = "{a}{b}";

        var tokens = Tokenize(input, @params);

        var paramTokens = tokens.Where(t => t.Type == RichTextTokenType.Parameter).ToList();
        Assert.AreEqual(2, paramTokens.Count,
            $"Two parameters must produce 2 Parameter tokens." + Dump(tokens, input));
        Assert.AreSame(p0, paramTokens[0].Parameter,
            $"First parameter token must be p0." + Dump(tokens, input));
        Assert.AreSame(p1, paramTokens[1].Parameter,
            $"Second parameter token must be p1." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_ParameterSurroundedByText_CorrectTokenSequence()
    {
        var stub = new StubParam();
        var @params = new[] { Tuple.Create<string, IParameter>("val", stub) };
        const string input = "before{val}after";

        var tokens = Tokenize(input, @params);

        Assert.AreEqual(3, tokens.Count,
            $"before{{val}}after must produce 3 tokens (Text, Param, Text)." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Text, tokens[0].Type, "Token[0] must be Text." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Parameter, tokens[1].Type, "Token[1] must be Parameter." + Dump(tokens, input));
        Assert.AreEqual(RichTextTokenType.Text, tokens[2].Type, "Token[2] must be Text." + Dump(tokens, input));
        Assert.AreEqual("before", tokens[0].Text,
            $"Token[0].Text must be \"before\"." + Dump(tokens, input));
        Assert.AreEqual("after", tokens[2].Text,
            $"Token[2].Text must be \"after\"." + Dump(tokens, input));
    }

    [TestMethod]
    public void Tokenize_UnclosedBrace_TreatedAsLiteralText()
    {
        const string input = "{unclosed";
        var tokens = Tokenize(input, NoParams);

        Assert.IsFalse(tokens.Any(t => t.Type == RichTextTokenType.Parameter),
            $"Unclosed '{{' must not produce a Parameter token." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 10 – Tag name case-insensitivity
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    [DataRow("<B>", "b")]
    [DataRow("<I>", "i")]
    [DataRow("<U>", "u")]
    [DataRow("<S>", "s")]
    [DataRow("<SUB>", "sub")]
    [DataRow("<SUP>", "sup")]
    [DataRow("<NOPARSE>", "noparse")]
    [DataRow("<COLOR=red>", "color")]
    [DataRow("<SIZE=32>", "size")]
    public void Tokenize_UppercaseTagNames_NormalizedToLowercase(string input, string expectedName)
    {
        var tokens = Tokenize(input);

        Assert.IsTrue(tokens.Count >= 1,
            $"Input \"{input}\" must produce at least 1 token." + Dump(tokens, input));

        var tagToken = tokens.FirstOrDefault(t =>
            t.Type is RichTextTokenType.OpenTag or RichTextTokenType.SelfCloseTag);

        Assert.IsNotNull(tagToken.TagName,
            $"Input \"{input}\" must produce a tag token with a non-null TagName." + Dump(tokens, input));
        Assert.AreEqual(expectedName, tagToken.TagName,
            $"Input \"{input}\" TagName must be \"{expectedName}\" (lowercase), got \"{tagToken.TagName}\"." + Dump(tokens, input));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 11 – Repeated calls (state isolation)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void Tokenize_CalledTwiceOnSameInstance_StateIsReset()
    {
        var tokenizer = NewTokenizer();

        var r1 = tokenizer.Tokenize("<b>hello</b>", NoParams);
        var r2 = tokenizer.Tokenize("<i>world</i>", NoParams);

        Assert.AreEqual(3, r1.Count,
            $"First call must produce 3 tokens.\n" + Dump(r1, "<b>hello</b>"));
        Assert.AreEqual(3, r2.Count,
            $"Second call must produce 3 tokens (state must not carry over).\n" + Dump(r2, "<i>world</i>"));
        Assert.AreEqual("b", r1[0].TagName,
            $"First call Token[0] must be \"b\"." + Dump(r1, "<b>hello</b>"));
        Assert.AreEqual("i", r2[0].TagName,
            $"Second call Token[0] must be \"i\"." + Dump(r2, "<i>world</i>"));
    }

    [TestMethod]
    public void Tokenize_EmptyThenNonEmpty_NonEmptyProducesCorrectResult()
    {
        var tokenizer = NewTokenizer();

        var r1 = tokenizer.Tokenize("", NoParams);
        var r2 = tokenizer.Tokenize("hello", NoParams);

        Assert.AreEqual(0, r1.Count, "Empty string must produce 0 tokens." + Dump(r1, ""));
        Assert.AreEqual(1, r2.Count, "Non-empty must produce 1 token after empty." + Dump(r2, "hello"));
        Assert.AreEqual("hello", r2[0].Text, "Token text mismatch." + Dump(r2, "hello"));
    }
}