// ─────────────────────────────────────────────────────────────────────────────
// RichTextParserTests.cs
//
// Comprehensive test suite for HintServiceMeow.Core.Utilities.Parser.RichTextParser
//
// ══ KNOWN BUGS UNDER TEST ══════════════════════════════════════════════════════
//
//  BUG-1  (ParseText – Reset before capture)
//         RichTextParser.ParseText() calls Reset() BEFORE calling
//         lineInfos.ToArray() / parameters.ToArray().  Reset() invokes
//         lineInfos.Clear() and parameters.Clear(), so every call returns
//         empty arrays regardless of the input.
//         All tests in Section 1 and beyond act as regressions for this.
//
//  BUG-2  (HandleOpenTag – wrong null-guard causes malformed CleanText)
//         The condition that appends "=<value>" to the StringBuilder reads:
//             if (!string.IsNullOrEmpty(tagName))   // ← BUG: should be `value`
//         Because tagName is never empty inside this branch, the '=' is always
//         appended even for value-less tags such as <b>, <i>, <u> etc.,
//         producing <b=> instead of <b> in CleanText.
//         The self-close and noparse early-return paths correctly check `value`.
//         Tests in Section 3 act as regressions for this.
//
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Models.Parser;
using HintServiceMeow.Core.Models.Parser.Style;
using HintServiceMeow.Core.Models.Parser.ValueObject;
using HintServiceMeow.Core.Utilities.Parser;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HintServiceMeow.Tests.Core.Utilities.Parser;

[TestClass]
public class RichTextParserTests
{
    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static RichTextParser NewParser() => new RichTextParser();

    /// <summary>
    /// Builds a <see cref="RichTextParserSetting"/> with sensible defaults so
    /// individual tests only need to supply the parts they care about.
    /// </summary>
    private static RichTextParserSetting DefaultSetting(
        string[]? illegalTags = null,
        HashSet<string>? ignoreTags = null,
        TextMeshStyle? style = null) =>
        new RichTextParserSetting(
            style ?? TextMeshStyle.Default,
            Array.Empty<Tuple<string, IHintParameter>>(),
            illegalTags ?? Array.Empty<string>(),
            ignoreTags ?? new HashSet<string>());

    // Convenience accessors ──────────────────────────────────────────────────

    private static TextSegmentStyle SegStyle(RichTextParserResult r, int line = 0, int seg = 0)
        => r.LineInfos[line].CharacterInfos[seg].Style;

    private static LineStyle LineStyle(RichTextParserResult r, int line = 0)
        => r.LineInfos[line].Style;

    // ─────────────────────────────────────────────────────────────────────────
    // Stub IHintParameter used by parameter tests
    // ─────────────────────────────────────────────────────────────────────────

    private sealed class StubParameter : IHintParameter
    {
        public global::Hints.HintParameter GetScpslHintParameter() =>
            throw new NotSupportedException("Stub — not used in unit tests.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 1 – Basic output structure
    //   Minimal tests that catch BUG-1 (Reset before capture).
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>BUG-1 regression: plain text must produce exactly one LineInfo.</summary>
    [TestMethod]
    public void ParseText_PlainText_ReturnsSingleLine()
    {
        var result = NewParser().ParseText("hello", DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            "BUG-1: Reset() must not be called before lineInfos.ToArray().");
    }

    [TestMethod]
    public void ParseText_PlainText_SingleSegmentHasCorrectText()
    {
        var result = NewParser().ParseText("hello", DefaultSetting());

        Assert.AreEqual(1, result.LineInfos[0].CharacterInfos.Length);
        Assert.AreEqual("hello", result.LineInfos[0].CharacterInfos[0].Text);
    }

    [TestMethod]
    public void ParseText_EmptyString_ReturnsOneLineWithNoSegments()
    {
        var result = NewParser().ParseText(string.Empty, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length);
        Assert.AreEqual(0, result.LineInfos[0].CharacterInfos.Length);
    }

    [TestMethod]
    public void ParseText_CalledTwice_EachCallReturnsIndependentResults()
    {
        var parser = NewParser();

        var r1 = parser.ParseText("first", DefaultSetting());
        var r2 = parser.ParseText("second", DefaultSetting());

        Assert.AreEqual(1, r1.LineInfos.Length);
        Assert.AreEqual(1, r2.LineInfos.Length);
        Assert.AreEqual("first", r1.LineInfos[0].CharacterInfos[0].Text);
        Assert.AreEqual("second", r2.LineInfos[0].CharacterInfos[0].Text);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 2 – Line breaks
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_NewlineCharacter_ProducesTwoLines()
    {
        var result = NewParser().ParseText("A\nB", DefaultSetting());
        Assert.AreEqual(2, result.LineInfos.Length);
    }

    [TestMethod]
    public void ParseText_NewlineCharacter_EachLineContainsCorrectText()
    {
        var result = NewParser().ParseText("line1\nline2", DefaultSetting());

        Assert.AreEqual("line1", result.LineInfos[0].CharacterInfos[0].Text);
        Assert.AreEqual("line2", result.LineInfos[1].CharacterInfos[0].Text);
    }

    [TestMethod]
    public void ParseText_BrTag_TreatedAsLineBreak()
    {
        var result = NewParser().ParseText("A<br>B", DefaultSetting());
        Assert.AreEqual(2, result.LineInfos.Length);
    }

    [TestMethod]
    public void ParseText_EscapeSequenceBackslashN_TreatedAsLineBreak()
    {
        // The tokenizer recognises \n (two characters) as a line-break escape.
        var result = NewParser().ParseText(@"A\nB", DefaultSetting());
        Assert.AreEqual(2, result.LineInfos.Length);
    }

    [TestMethod]
    public void ParseText_MultipleNewlines_CorrectLineCount()
    {
        var result = NewParser().ParseText("A\nB\nC\nD", DefaultSetting());
        Assert.AreEqual(4, result.LineInfos.Length);
    }

    [TestMethod]
    public void ParseText_TrailingNewline_ProducesExtraEmptyLastLine()
    {
        var result = NewParser().ParseText("A\n", DefaultSetting());

        Assert.AreEqual(2, result.LineInfos.Length);
        Assert.AreEqual(0, result.LineInfos[1].CharacterInfos.Length,
            "The line after a trailing newline must be empty.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 3 – CleanText correctness  (BUG-2 regressions)
    //
    // Test design note: the line is intentionally finished with the tag still
    // open (no close tag before EOF) so that only the HandleOpenTag serialisation
    // path is exercised.  The close-tag path and the noparse early-return path
    // are NOT affected by BUG-2 (they correctly check `value`).
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>BUG-2 regression: &lt;b&gt; must be emitted as "&lt;b&gt;" not "&lt;b=&gt;".</summary>
    [TestMethod]
    public void ParseText_BoldOpenTag_CleanTextContainsWellFormedTag()
    {
        var result = NewParser().ParseText("<b>hello", DefaultSetting());

        string clean = result.LineInfos[0].CleanText;
        StringAssert.Contains(clean, "<b>", "BUG-2: <b> must not be emitted as <b=>.");
        Assert.IsFalse(clean.Contains("<b=>"), "BUG-2: <b=> is the malformed form produced by the bug.");
    }

    /// <summary>BUG-2: &lt;i&gt; must not become &lt;i=&gt;</summary>
    [TestMethod]
    public void ParseText_ItalicOpenTag_CleanTextContainsWellFormedTag()
    {
        var result = NewParser().ParseText("<i>text", DefaultSetting());

        Assert.IsFalse(result.LineInfos[0].CleanText.Contains("<i=>"), "BUG-2: <i=> is the malformed form.");
        StringAssert.Contains(result.LineInfos[0].CleanText, "<i>");
    }

    /// <summary>BUG-2: &lt;u&gt; must not become &lt;u=&gt;</summary>
    [TestMethod]
    public void ParseText_UnderlineOpenTag_CleanTextContainsWellFormedTag()
    {
        var result = NewParser().ParseText("<u>text", DefaultSetting());

        Assert.IsFalse(result.LineInfos[0].CleanText.Contains("<u=>"));
        StringAssert.Contains(result.LineInfos[0].CleanText, "<u>");
    }

    /// <summary>BUG-2: &lt;s&gt; must not become &lt;s=&gt;</summary>
    [TestMethod]
    public void ParseText_StrikethroughOpenTag_CleanTextContainsWellFormedTag()
    {
        var result = NewParser().ParseText("<s>text", DefaultSetting());

        Assert.IsFalse(result.LineInfos[0].CleanText.Contains("<s=>"));
        StringAssert.Contains(result.LineInfos[0].CleanText, "<s>");
    }

    /// <summary>BUG-2: &lt;sub&gt; must not become &lt;sub=&gt;</summary>
    [TestMethod]
    public void ParseText_SubscriptOpenTag_CleanTextContainsWellFormedTag()
    {
        var result = NewParser().ParseText("<sub>text", DefaultSetting());

        Assert.IsFalse(result.LineInfos[0].CleanText.Contains("<sub=>"));
        StringAssert.Contains(result.LineInfos[0].CleanText, "<sub>");
    }

    /// <summary>BUG-2: &lt;sup&gt; must not become &lt;sup=&gt;</summary>
    [TestMethod]
    public void ParseText_SuperscriptOpenTag_CleanTextContainsWellFormedTag()
    {
        var result = NewParser().ParseText("<sup>text", DefaultSetting());

        Assert.IsFalse(result.LineInfos[0].CleanText.Contains("<sup=>"));
        StringAssert.Contains(result.LineInfos[0].CleanText, "<sup>");
    }

    /// <summary>
    /// Value-bearing open tags MUST still emit the value.
    /// This confirms the BUG-2 fix does not break tags that do have a value.
    /// </summary>
    [TestMethod]
    public void ParseText_ColorOpenTagWithValue_CleanTextContainsEqualSignAndValue()
    {
        var result = NewParser().ParseText("<color=red>text", DefaultSetting());
        StringAssert.Contains(result.LineInfos[0].CleanText, "<color=red>");
    }

    [TestMethod]
    public void ParseText_SizeOpenTagWithValue_CleanTextContainsEqualSignAndValue()
    {
        var result = NewParser().ParseText("<size=24>text", DefaultSetting());
        StringAssert.Contains(result.LineInfos[0].CleanText, "<size=24>");
    }

    /// <summary>
    /// Self-closing tags use the correct value-check branch already;
    /// verify they are also well-formed (no regression from a future fix).
    /// </summary>
    [TestMethod]
    public void ParseText_SpaceSelfCloseTag_CleanTextIsWellFormed()
    {
        var result = NewParser().ParseText("<space=10>text", DefaultSetting());
        StringAssert.Contains(result.LineInfos[0].CleanText, "<space=10>");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 4 – Counter-based style tags
    //   (b, i, u, s, sub, sup, allcaps, lowercase, uppercase)
    //
    // Style is captured per-segment at HandleText() time, so close-tag
    // reversion tests use the pattern "<tag>A</tag>B" and check segs[0]/[1].
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_BoldTag_SegmentIsBold()
    {
        var result = NewParser().ParseText("<b>text</b>", DefaultSetting());
        Assert.IsTrue(SegStyle(result).Bold);
    }

    [TestMethod]
    public void ParseText_BoldCloseTag_StyleRevertsAfterTag()
    {
        var result = NewParser().ParseText("<b>bold</b>plain", DefaultSetting());

        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Bold, "Before </b> must be bold.");
        Assert.IsFalse(result.LineInfos[0].CharacterInfos[1].Style.Bold, "After </b> must not be bold.");
    }

    [TestMethod]
    public void ParseText_NestedBoldTags_StillBoldAfterFirstClose()
    {
        // <b><b>inner</b>middle</b>outer  — Bold counter goes 0→1→2→1→0
        var result = NewParser().ParseText("<b><b>inner</b>middle</b>outer", DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsTrue(segs[0].Style.Bold, "\"inner\" (Bold=2) must be bold.");
        Assert.IsTrue(segs[1].Style.Bold, "\"middle\" (Bold=1) must still be bold.");
        Assert.IsFalse(segs[2].Style.Bold, "\"outer\" (Bold=0) must not be bold.");
    }

    [TestMethod]
    public void ParseText_ExtraCloseBoldTag_DoesNotThrowOrUnderflow()
    {
        // The guard "if (Bold > 0) Bold--" prevents underflow
        var result = NewParser().ParseText("<b>text</b></b></b>extra", DefaultSetting());
        Assert.AreEqual(1, result.LineInfos.Length);
    }

    [TestMethod]
    public void ParseText_ItalicTag_SegmentIsItalic()
    {
        var result = NewParser().ParseText("<i>text</i>", DefaultSetting());
        Assert.IsTrue(SegStyle(result).Italic);
    }

    [TestMethod]
    public void ParseText_ItalicCloseTag_StyleReverts()
    {
        var result = NewParser().ParseText("<i>styled</i>plain", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Italic);
        Assert.IsFalse(result.LineInfos[0].CharacterInfos[1].Style.Italic);
    }

    [TestMethod]
    public void ParseText_UnderlineTag_SegmentIsUnderline()
    {
        var result = NewParser().ParseText("<u>text</u>", DefaultSetting());
        Assert.IsTrue(SegStyle(result).Underline);
    }

    [TestMethod]
    public void ParseText_UnderlineCloseTag_StyleReverts()
    {
        var result = NewParser().ParseText("<u>under</u>plain", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Underline);
        Assert.IsFalse(result.LineInfos[0].CharacterInfos[1].Style.Underline);
    }

    [TestMethod]
    public void ParseText_StrikethroughTag_SegmentIsStrikethrough()
    {
        var result = NewParser().ParseText("<s>text</s>", DefaultSetting());
        Assert.IsTrue(SegStyle(result).Strikethrough);
    }

    [TestMethod]
    public void ParseText_SubscriptTag_SegmentSubscriptIsPositive()
    {
        var result = NewParser().ParseText("<sub>text</sub>", DefaultSetting());
        Assert.IsTrue(SegStyle(result).Subscript > 0);
    }

    [TestMethod]
    public void ParseText_SubscriptCloseTag_SubscriptReverts()
    {
        var result = NewParser().ParseText("<sub>sub</sub>normal", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Subscript > 0);
        Assert.AreEqual(0, result.LineInfos[0].CharacterInfos[1].Style.Subscript);
    }

    [TestMethod]
    public void ParseText_SuperscriptTag_SegmentSuperscriptIsPositive()
    {
        var result = NewParser().ParseText("<sup>text</sup>", DefaultSetting());
        Assert.IsTrue(SegStyle(result).Superscript > 0);
    }

    [TestMethod]
    public void ParseText_SuperscriptCloseTag_SuperscriptReverts()
    {
        var result = NewParser().ParseText("<sup>sup</sup>normal", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Superscript > 0);
        Assert.AreEqual(0, result.LineInfos[0].CharacterInfos[1].Style.Superscript);
    }

    [TestMethod]
    public void ParseText_BoldAndItalicNested_BothStylesActiveOnInnerSegment()
    {
        var result = NewParser().ParseText("<b><i>text</i></b>", DefaultSetting());
        Assert.IsTrue(SegStyle(result).Bold);
        Assert.IsTrue(SegStyle(result).Italic);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 5 – Stack-based style tags
    //   (color, size, align, indent, mark)
    //
    // Test design for LINE-LEVEL styles (align, indent):
    //   The tag is left open so FinishLine() at end-of-input captures the
    //   active value.  Closing the tag before EOF would cause the cache to be
    //   cleared and FinishLine() would re-evaluate with the default.
    // ═════════════════════════════════════════════════════════════════════════

    // ── color ────────────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseText_ColorNamedRed_SegmentColorIsRed()
    {
        var result = NewParser().ParseText("<color=red>text</color>", DefaultSetting());
        Color c = SegStyle(result).Color;
        Assert.AreEqual(255, c.Red);
        Assert.AreEqual(0, c.Green);
        Assert.AreEqual(0, c.Blue);
    }

    [TestMethod]
    public void ParseText_ColorHex6Char_ParsedCorrectly()
    {
        var result = NewParser().ParseText("<color=#1A2B3C>text</color>", DefaultSetting());
        Color c = SegStyle(result).Color;
        Assert.AreEqual(0x1A, c.Red);
        Assert.AreEqual(0x2B, c.Green);
        Assert.AreEqual(0x3C, c.Blue);
        Assert.AreEqual(255, c.Alpha, "Alpha should default to 255 for a 6-char hex colour.");
    }

    [TestMethod]
    public void ParseText_ColorHex8Char_ParsedWithAlpha()
    {
        var result = NewParser().ParseText("<color=#FF0080AA>text</color>", DefaultSetting());
        Color c = SegStyle(result).Color;
        Assert.AreEqual(0xFF, c.Red);
        Assert.AreEqual(0x00, c.Green);
        Assert.AreEqual(0x80, c.Blue);
        Assert.AreEqual(0xAA, c.Alpha);
    }

    [TestMethod]
    public void ParseText_ColorHex3CharShorthand_ExpandedCorrectly()
    {
        // #F00 → FF0000
        var result = NewParser().ParseText("<color=#F00>text</color>", DefaultSetting());
        Color c = SegStyle(result).Color;
        Assert.AreEqual(0xFF, c.Red);
        Assert.AreEqual(0x00, c.Green);
        Assert.AreEqual(0x00, c.Blue);
    }

    [TestMethod]
    [DataRow("green", 0, 128, 0)]
    [DataRow("blue", 0, 0, 255)]
    [DataRow("white", 255, 255, 255)]
    [DataRow("black", 0, 0, 0)]
    [DataRow("yellow", 255, 255, 0)]
    [DataRow("cyan", 0, 255, 255)]
    [DataRow("magenta", 255, 0, 255)]
    [DataRow("orange", 255, 165, 0)]
    [DataRow("purple", 128, 0, 128)]
    [DataRow("grey", 128, 128, 128)]
    [DataRow("gray", 128, 128, 128)]
    public void ParseText_NamedColor_ParsedToCorrectRgb(string name, int r, int g, int b)
    {
        var result = NewParser().ParseText($"<color={name}>x</color>", DefaultSetting());
        Color c = SegStyle(result).Color;
        Assert.AreEqual((byte)r, c.Red, $"Red mismatch for '{name}'.");
        Assert.AreEqual((byte)g, c.Green, $"Green mismatch for '{name}'.");
        Assert.AreEqual((byte)b, c.Blue, $"Blue mismatch for '{name}'.");
    }

    [TestMethod]
    public void ParseText_NestedColors_InnerColorTakesPrecedence()
    {
        // outer=blue, inner=red
        var result = NewParser().ParseText("<color=blue><color=red>inner</color>outer</color>", DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.AreEqual(255, segs[0].Style.Color.Red, "Inner segment must be red.");
        Assert.AreEqual(0, segs[0].Style.Color.Blue, "Inner segment must not be blue.");
        Assert.AreEqual(0, segs[1].Style.Color.Red, "Outer segment must not be red.");
        Assert.AreEqual(255, segs[1].Style.Color.Blue, "Outer segment must be blue.");
    }

    [TestMethod]
    public void ParseText_ColorCloseTag_ReturnsToDefaultColor()
    {
        Color def = TextMeshStyle.Default.CharStyle.Color;
        var result = NewParser().ParseText("<color=red>colored</color>plain", DefaultSetting());
        Color after = result.LineInfos[0].CharacterInfos[1].Style.Color;
        Assert.AreEqual(def.Red, after.Red);
        Assert.AreEqual(def.Green, after.Green);
        Assert.AreEqual(def.Blue, after.Blue);
    }

    [TestMethod]
    public void ParseText_InvalidColorValue_ColorNotChanged()
    {
        Color def = TextMeshStyle.Default.CharStyle.Color;
        var result = NewParser().ParseText("<color=notacolor>text</color>", DefaultSetting());
        Color c = SegStyle(result).Color;
        Assert.AreEqual(def.Red, c.Red, "Invalid colour must leave Red at default.");
        Assert.AreEqual(def.Green, c.Green, "Invalid colour must leave Green at default.");
        Assert.AreEqual(def.Blue, c.Blue, "Invalid colour must leave Blue at default.");
    }

    // ── size ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseText_SizeInPixels_SegmentFontSizeMatches()
    {
        var result = NewParser().ParseText("<size=32>text</size>", DefaultSetting());
        Assert.AreEqual(32f, SegStyle(result).FontSize, 0.001f);
    }

    [TestMethod]
    public void ParseText_SizeInEm_SegmentFontSizeIsRelativeToDefault()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        var result = NewParser().ParseText("<size=2em>text</size>", DefaultSetting());
        Assert.AreEqual(2f * def, SegStyle(result).FontSize, 0.001f);
    }

    [TestMethod]
    public void ParseText_SizeInPercent_SegmentFontSizeIsPercentOfDefault()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        var result = NewParser().ParseText("<size=200%>text</size>", DefaultSetting());
        Assert.AreEqual(def * 2f, SegStyle(result).FontSize, 0.001f);
    }

    [TestMethod]
    public void ParseText_SizeCloseTag_FontSizeReverts()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        var result = NewParser().ParseText("<size=48>large</size>normal", DefaultSetting());
        Assert.AreEqual(48f, result.LineInfos[0].CharacterInfos[0].Style.FontSize, 0.001f);
        Assert.AreEqual(def, result.LineInfos[0].CharacterInfos[1].Style.FontSize, 0.001f);
    }

    [TestMethod]
    public void ParseText_InvalidSizeValue_FontSizeUnchanged()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        var result = NewParser().ParseText("<size=notanumber>text</size>", DefaultSetting());
        Assert.AreEqual(def, SegStyle(result).FontSize, 0.001f);
    }

    // ── align ─────────────────────────────────────────────────────────────────
    // Tags are left open so FinishLine() at EOF captures the active alignment.

    [TestMethod]
    public void ParseText_AlignLeft_LineStyleIsLeft()
    {
        var result = NewParser().ParseText("<align=left>text", DefaultSetting());
        Assert.AreEqual(HintAlignment.Left, LineStyle(result).Alignment);
    }

    [TestMethod]
    public void ParseText_AlignCenter_LineStyleIsCenter()
    {
        var result = NewParser().ParseText("<align=center>text", DefaultSetting());
        Assert.AreEqual(HintAlignment.Center, LineStyle(result).Alignment);
    }

    [TestMethod]
    public void ParseText_AlignRight_LineStyleIsRight()
    {
        var result = NewParser().ParseText("<align=right>text", DefaultSetting());
        Assert.AreEqual(HintAlignment.Right, LineStyle(result).Alignment);
    }

    [TestMethod]
    public void ParseText_AlignJustified_LineStyleIsJustified()
    {
        var result = NewParser().ParseText("<align=justified>text", DefaultSetting());
        Assert.AreEqual(HintAlignment.Justified, LineStyle(result).Alignment);
    }

    [TestMethod]
    public void ParseText_AlignFlush_LineStyleIsFlush()
    {
        var result = NewParser().ParseText("<align=flush>text", DefaultSetting());
        Assert.AreEqual(HintAlignment.Flush, LineStyle(result).Alignment);
    }

    [TestMethod]
    public void ParseText_InvalidAlignValue_AlignmentUnchanged()
    {
        HintAlignment def = TextMeshStyle.Default.LineStyle.Alignment;
        var result = NewParser().ParseText("<align=diagonal>text", DefaultSetting());
        Assert.AreEqual(def, LineStyle(result).Alignment, "Invalid alignment must leave value at default.");
    }

    // ── indent ────────────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseText_IndentInPixels_LineStyleIndentMatches()
    {
        // Tag left open so FinishLine() captures the active indent.
        var result = NewParser().ParseText("<indent=20>text", DefaultSetting());
        Assert.AreEqual(20f, LineStyle(result).Indent, 0.001f);
    }

    // ── mark ──────────────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseText_MarkNamedRed_SegmentMarkColorIsRed()
    {
        var result = NewParser().ParseText("<mark=red>text</mark>", DefaultSetting());
        Assert.IsNotNull(SegStyle(result).Mark);
        Assert.AreEqual(255, SegStyle(result).Mark!.Value.Red);
    }

    [TestMethod]
    public void ParseText_MarkCloseTag_MarkColorReverts()
    {
        Color? def = TextMeshStyle.Default.CharStyle.Mark;
        var result = NewParser().ParseText("<mark=red>marked</mark>plain", DefaultSetting());
        Assert.IsNotNull(result.LineInfos[0].CharacterInfos[0].Style.Mark, "Mark must be set inside tag.");
        Assert.AreEqual(def, result.LineInfos[0].CharacterInfos[1].Style.Mark, "Mark must revert after </mark>.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 6 – Single-value style tags
    //   (cspace, font, font-weight, line-height, line-indent,
    //    margin, margin-left, margin-right, mspace, rotate, voffset, width)
    //
    // For LINE-LEVEL tags the test places a \n BEFORE the close tag so that
    // FinishLine() is called while the tag is still active, then the second
    // line can be used to verify the reset.
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_FontTag_SegmentFontNameSet()
    {
        var result = NewParser().ParseText("<font=Arial>text</font>", DefaultSetting());
        Assert.AreEqual("Arial", SegStyle(result).Font);
    }

    [TestMethod]
    public void ParseText_FontCloseTag_FontReverts()
    {
        string? def = TextMeshStyle.Default.CharStyle.Font;
        var result = NewParser().ParseText("<font=Arial>styled</font>plain", DefaultSetting());
        Assert.AreEqual("Arial", result.LineInfos[0].CharacterInfos[0].Style.Font);
        Assert.AreEqual(def, result.LineInfos[0].CharacterInfos[1].Style.Font);
    }

    [TestMethod]
    public void ParseText_FontWeightTag_SegmentFontWeightSet()
    {
        var result = NewParser().ParseText("<font-weight=700>text</font-weight>", DefaultSetting());
        Assert.AreEqual(700, SegStyle(result).FontWeight);
    }

    [TestMethod]
    public void ParseText_FontWeightCloseTag_FontWeightReverts()
    {
        int? def = TextMeshStyle.Default.CharStyle.FontWeight;
        var result = NewParser().ParseText("<font-weight=700>heavy</font-weight>normal", DefaultSetting());
        Assert.AreEqual(700, result.LineInfos[0].CharacterInfos[0].Style.FontWeight);
        Assert.AreEqual(def, result.LineInfos[0].CharacterInfos[1].Style.FontWeight);
    }

    [TestMethod]
    public void ParseText_LineHeightTag_ActiveOnLine_LineStyleLineHeightSet()
    {
        // Line 0 ends (via \n) while the tag is still open.
        var result = NewParser().ParseText("<line-height=30>text\nafter", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].Style.LineHeight.HasValue, "Line 0 must have a LineHeight value.");
        Assert.AreEqual(30f, result.LineInfos[0].Style.LineHeight!.Value, 0.001f);
    }

    [TestMethod]
    public void ParseText_LineHeightCloseTag_LineHeightResetsOnNextLine()
    {
        // Structure: <tag>A  \n  B</tag>  \n  C
        //   Line 0 finishes while tag is open  → LineHeight = 30
        //   Line 1 finishes after close tag    → LineHeight = null (reset)
        var result = NewParser().ParseText("<line-height=30>A\nB</line-height>\nC", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].Style.LineHeight.HasValue, "Line 0 must have a LineHeight value.");
        Assert.AreEqual(30f, result.LineInfos[0].Style.LineHeight!.Value, 0.001f, "Line 0 must have LineHeight=30.");
        Assert.IsNull(result.LineInfos[1].Style.LineHeight, "Line 1 must have LineHeight=null after close tag.");
    }

    [TestMethod]
    public void ParseText_LineIndentTag_ActiveOnLine_IndentSet()
    {
        var result = NewParser().ParseText("<line-indent=10>text\nafter", DefaultSetting());
        Assert.AreEqual(10f, result.LineInfos[0].Style.Indent, 0.001f);
    }

    [TestMethod]
    public void ParseText_MarginTag_BothMarginsSet()
    {
        var result = NewParser().ParseText("<margin=15>text\nafter", DefaultSetting());
        Assert.AreEqual(15f, result.LineInfos[0].Style.MarginLeft, 0.001f);
        Assert.AreEqual(15f, result.LineInfos[0].Style.MarginRight, 0.001f);
    }

    [TestMethod]
    public void ParseText_MarginLeftTag_OnlyLeftMarginAffected()
    {
        var result = NewParser().ParseText("<margin-left=10>text\nafter", DefaultSetting());
        Assert.AreEqual(10f, result.LineInfos[0].Style.MarginLeft, 0.001f);
        Assert.AreEqual(0f, result.LineInfos[0].Style.MarginRight, 0.001f);
    }

    [TestMethod]
    public void ParseText_MarginRightTag_OnlyRightMarginAffected()
    {
        var result = NewParser().ParseText("<margin-right=10>text\nafter", DefaultSetting());
        Assert.AreEqual(0f, result.LineInfos[0].Style.MarginLeft, 0.001f);
        Assert.AreEqual(10f, result.LineInfos[0].Style.MarginRight, 0.001f);
    }

    [TestMethod]
    public void ParseText_MarginCloseTag_MarginsResetOnNextLine()
    {
        // Line 0 finishes while margin is open; line 1 finishes after close.
        var result = NewParser().ParseText("<margin=15>A\nB</margin>\nC", DefaultSetting());
        Assert.AreEqual(15f, result.LineInfos[0].Style.MarginLeft, 0.001f, "Line 0 must have margin-left=15.");
        Assert.AreEqual(0f, result.LineInfos[1].Style.MarginLeft, 0.001f, "Line 1 must have margin-left=0 after close.");
    }

    [TestMethod]
    public void ParseText_CSpaceTag_SegmentCharSpaceSet()
    {
        var result = NewParser().ParseText("<cspace=3>text</cspace>", DefaultSetting());
        Assert.IsTrue(SegStyle(result).CharSpace.HasValue, "CharSpace must have a value inside <cspace>.");
        Assert.AreEqual(3f, SegStyle(result).CharSpace!.Value, 0.001f);
    }

    [TestMethod]
    public void ParseText_CSpaceCloseTag_CharSpaceReverts()
    {
        float? def = TextMeshStyle.Default.CharStyle.CharSpace;
        var result = NewParser().ParseText("<cspace=3>spaced</cspace>normal", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.CharSpace.HasValue, "CharSpace must have a value inside <cspace>.");
        Assert.AreEqual(3f, result.LineInfos[0].CharacterInfos[0].Style.CharSpace!.Value, 0.001f);
        Assert.AreEqual(def, result.LineInfos[0].CharacterInfos[1].Style.CharSpace);
    }

    [TestMethod]
    public void ParseText_MSpaceTag_SegmentMonospaceSet()
    {
        var result = NewParser().ParseText("<mspace=10>text</mspace>", DefaultSetting());
        Assert.IsTrue(SegStyle(result).Monospace.HasValue, "Monospace must have a value inside <mspace>.");
        Assert.AreEqual(10f, SegStyle(result).Monospace!.Value, 0.001f);
    }

    [TestMethod]
    public void ParseText_MSpaceCloseTag_MonospaceReverts()
    {
        float? def = TextMeshStyle.Default.CharStyle.Monospace;
        var result = NewParser().ParseText("<mspace=10>mono</mspace>normal", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Monospace.HasValue, "Monospace must have a value inside <mspace>.");
        Assert.AreEqual(10f, result.LineInfos[0].CharacterInfos[0].Style.Monospace!.Value, 0.001f);
        Assert.AreEqual(def, result.LineInfos[0].CharacterInfos[1].Style.Monospace);
    }

    [TestMethod]
    public void ParseText_RotateTag_SegmentRotateSet()
    {
        var result = NewParser().ParseText("<rotate=45>text</rotate>", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Rotate.HasValue, "Rotate must have a value inside <rotate>.");
        Assert.AreEqual(45f, SegStyle(result).Rotate!.Value, 0.001f);
    }

    [TestMethod]
    public void ParseText_RotateCloseTag_RotateReverts()
    {
        var result = NewParser().ParseText("<rotate=45>tilted</rotate>normal", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Rotate.HasValue, "Rotate must have a value inside <rotate>.");
        Assert.AreEqual(45f, result.LineInfos[0].CharacterInfos[0].Style.Rotate!.Value, 0.001f);
        Assert.IsNull(result.LineInfos[0].CharacterInfos[1].Style.Rotate, "Rotate must revert to null.");
    }

    [TestMethod]
    public void ParseText_VOffsetTag_SegmentVOffsetSet()
    {
        var result = NewParser().ParseText("<voffset=5>text</voffset>", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.VOffset.HasValue, "VOffset must have a value inside <voffset>.");
        Assert.AreEqual(5f, SegStyle(result).VOffset!.Value, 0.001f);
    }

    [TestMethod]
    public void ParseText_VOffsetCloseTag_VOffsetReverts()
    {
        var result = NewParser().ParseText("<voffset=5>up</voffset>normal", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.VOffset.HasValue, "VOffset must have a value inside <voffset>.");
        Assert.AreEqual(5f, result.LineInfos[0].CharacterInfos[0].Style.VOffset!.Value, 0.001f);
        Assert.IsNull(result.LineInfos[0].CharacterInfos[1].Style.VOffset, "VOffset must revert to null.");
    }

    [TestMethod]
    public void ParseText_WidthTag_LineStyleMaxWidthSet()
    {
        // Tag left open so FinishLine() captures the active width.
        var result = NewParser().ParseText("<width=400>text", DefaultSetting());
        Assert.AreEqual(400f, LineStyle(result).MaxWidth, 0.001f);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 7 – Boolean tags  (nobr, noparse, smallcaps)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_SmallcapsTag_ParserDoesNotThrowAndReturnsLine()
    {
        var result = NewParser().ParseText("<smallcaps>text</smallcaps>", DefaultSetting());
        Assert.AreEqual(1, result.LineInfos.Length, "<smallcaps> must not crash the parser.");
        Assert.IsTrue(result.LineInfos[0].CharacterInfos.Length > 0);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 8 – Noparse mode
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_NoparseMode_InnerTagsDoNotAffectStyle()
    {
        var result = NewParser().ParseText("<noparse><b>text</b></noparse>", DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length);
        bool anyBold = result.LineInfos[0].CharacterInfos.Any(s => s.Style.Bold);
        Assert.IsFalse(anyBold, "Tags inside <noparse> must not apply their style.");
    }

    [TestMethod]
    public void ParseText_NoparseMode_TagsAppearsLiterallyInCleanText()
    {
        // The NoParse early-return path uses the CORRECT !string.IsNullOrEmpty(TagValue)
        // guard, so inner value-less tags like <b> are emitted without the '=' even
        // when BUG-2 is present in the normal open-tag path.
        var result = NewParser().ParseText("<noparse><b>text</b></noparse>", DefaultSetting());
        StringAssert.Contains(result.LineInfos[0].CleanText, "<b>",
            "The literal '<b>' inside <noparse> must appear verbatim in CleanText.");
    }

    [TestMethod]
    public void ParseText_NoparseCloseTag_ResumesNormalTagParsing()
    {
        var result = NewParser().ParseText("<noparse></noparse><b>text</b>", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos.Any(s => s.Style.Bold),
            "After </noparse>, <b> must be parsed normally and apply Bold.");
    }

    [TestMethod]
    public void ParseText_NoparseWithNestedStylingTags_FontSizeUnchanged()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        var result = NewParser().ParseText("<noparse><size=99>text</size></noparse>", DefaultSetting());

        Assert.IsFalse(result.LineInfos[0].CharacterInfos.Any(s => Math.Abs(s.Style.FontSize - def) > 0.01f),
            "Font-size must remain at default; <size> must be suppressed inside <noparse>.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 9 – Self-closing tags  (alpha, space, pos)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_AlphaTagHalf_SegmentAlphaIsApproximatelyHalf()
    {
        // <alpha=#80> → byte 0x80 = 128 → stored as 128/255 ≈ 0.502f
        var result = NewParser().ParseText("<alpha=#80>text</alpha>", DefaultSetting());
        Assert.AreEqual(128f / 255f, SegStyle(result).Alpha!.Value, 0.01f);
    }

    [TestMethod]
    public void ParseText_AlphaTagZero_SegmentAlphaIsZero()
    {
        // <alpha=#00> → fully transparent
        var result = NewParser().ParseText("<alpha=#00>text</alpha>", DefaultSetting());
        Assert.AreEqual(0f, SegStyle(result).Alpha!.Value, 0.01f);
    }

    [TestMethod]
    public void ParseText_AlphaCloseTag_AlphaRevertsToDefault()
    {
        // Default alpha (currentStyle.Alpha == null) → GetCharStyle produces 1f.
        var result = NewParser().ParseText("<alpha=#00>dark</alpha>bright", DefaultSetting());
        Assert.AreEqual(0f, result.LineInfos[0].CharacterInfos[0].Style.Alpha!.Value, 0.01f,
            "Segment inside <alpha=#00> must have alpha=0.");
        Assert.AreEqual(1f, result.LineInfos[0].CharacterInfos[1].Style.Alpha!.Value, 0.01f,
            "Segment after </alpha> must revert to default alpha=1.");
    }

    [TestMethod]
    public void ParseText_SpaceTag_InsertsPlaceholderWithCustomWidth()
    {
        var result = NewParser().ParseText("<space=20>text", DefaultSetting());
        bool hasPlaceholder = result.LineInfos[0].CharacterInfos.Any(s => s.CustomWidth.HasValue);
        Assert.IsTrue(hasPlaceholder, "<space> must produce a TextSegment with a CustomWidth.");
    }

    [TestMethod]
    public void ParseText_SpaceTag_PlaceholderWidthMatchesTagValue()
    {
        var result = NewParser().ParseText("<space=20>text", DefaultSetting());
        TextSegment ph = result.LineInfos[0].CharacterInfos.First(s => s.CustomWidth.HasValue);
        Assert.AreEqual(20f, ph.CustomWidth!.Value, 0.001f);
    }

    [TestMethod]
    public void ParseText_PosTag_InsertsPlaceholderWithCorrectWidth()
    {
        var result = NewParser().ParseText("<pos=50>text", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].CharacterInfos.Any(s => s.CustomWidth.HasValue),
            "<pos> must produce a placeholder with CustomWidth.");
        TextSegment ph = result.LineInfos[0].CharacterInfos.First(s => s.CustomWidth.HasValue);
        Assert.AreEqual(50f, ph.CustomWidth!.Value, 0.001f);
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 10 – IllegalTags
    //   Illegal tags must be stripped from CleanText; their style side-effects
    //   must still be applied (the tag is processed, just not emitted).
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_IllegalTag_NotPresentInCleanText()
    {
        var setting = DefaultSetting(illegalTags: new[] { "b" });
        var result = NewParser().ParseText("<b>text</b>", setting);
        Assert.IsFalse(result.LineInfos[0].CleanText.Contains("<b"),
            "Illegal tag must be stripped from CleanText.");
    }

    [TestMethod]
    public void ParseText_IllegalTag_StyleEffectIsStillApplied()
    {
        var setting = DefaultSetting(illegalTags: new[] { "b" });
        var result = NewParser().ParseText("<b>text</b>", setting);
        Assert.IsTrue(SegStyle(result).Bold,
            "An illegal tag's style side-effect must still be applied even though it is not emitted.");
    }

    [TestMethod]
    public void ParseText_IllegalColorTag_ColorEffectAppliedButTagStripped()
    {
        var setting = DefaultSetting(illegalTags: new[] { "color" });
        var result = NewParser().ParseText("<color=red>text</color>", setting);
        Assert.AreEqual(255, SegStyle(result).Color.Red,
            "Illegal <color=red> must still set the segment colour.");
        Assert.IsFalse(result.LineInfos[0].CleanText.Contains("<color"),
            "Illegal <color> must not appear in CleanText.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 11 – IgnoreTags
    //   Ignored tags pass through verbatim into CleanText; their style effects
    //   must NOT be applied.
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_IgnoreTag_TagAppearsLiterallyInCleanText()
    {
        var setting = DefaultSetting(ignoreTags: new HashSet<string> { "b" });
        var result = NewParser().ParseText("<b>text</b>", setting);
        StringAssert.Contains(result.LineInfos[0].CleanText, "<b>",
            "Ignored tag must pass through as a literal string in CleanText.");
    }

    [TestMethod]
    public void ParseText_IgnoreTag_StyleNotApplied()
    {
        var setting = DefaultSetting(ignoreTags: new HashSet<string> { "b" });
        var result = NewParser().ParseText("<b>text</b>", setting);
        Assert.IsFalse(result.LineInfos[0].CharacterInfos.Any(s => s.Style.Bold),
            "Ignored tag must not apply its style.");
    }

    [TestMethod]
    public void ParseText_IgnoreCloseTag_CloseTagAlsoPassedThrough()
    {
        var setting = DefaultSetting(ignoreTags: new HashSet<string> { "color" });
        var result = NewParser().ParseText("<color=red>text</color>", setting);
        StringAssert.Contains(result.LineInfos[0].CleanText, "</color>",
            "The close tag of an ignored tag must also pass through literally.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 12 – Parameters
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_SingleParameter_CapturedInResultParameters()
    {
        var param = new StubParameter();
        var setting = new RichTextParserSetting(
            TextMeshStyle.Default,
            new[] { Tuple.Create<string, IHintParameter>("foo", param) },
            Array.Empty<string>(),
            new HashSet<string>());

        var result = NewParser().ParseText("{foo}", setting);

        Assert.AreEqual(1, result.Parameters.Length);
        Assert.AreSame(param, result.Parameters[0]);
    }

    [TestMethod]
    public void ParseText_SingleParameter_PlaceholderSegmentIsZeroBasedIndex()
    {
        var setting = new RichTextParserSetting(
            TextMeshStyle.Default,
            new[] { Tuple.Create<string, IHintParameter>("foo", (IHintParameter)new StubParameter()) },
            Array.Empty<string>(),
            new HashSet<string>());

        var result = NewParser().ParseText("{foo}", setting);

        bool hasIndexSeg = result.LineInfos[0].CharacterInfos.Any(s => s.Text == "0");
        Assert.IsTrue(hasIndexSeg,
            "The parameter placeholder must be replaced by its zero-based index text segment.");
    }

    [TestMethod]
    public void ParseText_TwoParameters_CapturedInDeclarationOrder()
    {
        var p0 = new StubParameter();
        var p1 = new StubParameter();
        var setting = new RichTextParserSetting(
            TextMeshStyle.Default,
            new[]
            {
                Tuple.Create<string, IHintParameter>("a", (IHintParameter)p0),
                Tuple.Create<string, IHintParameter>("b", (IHintParameter)p1),
            },
            Array.Empty<string>(),
            new HashSet<string>());

        var result = NewParser().ParseText("{a} {b}", setting);

        Assert.AreEqual(2, result.Parameters.Length);
        Assert.AreSame(p0, result.Parameters[0]);
        Assert.AreSame(p1, result.Parameters[1]);
    }

    [TestMethod]
    public void ParseText_TwoParameters_PlaceholderIndicesAreSequential()
    {
        var setting = new RichTextParserSetting(
            TextMeshStyle.Default,
            new[]
            {
                Tuple.Create<string, IHintParameter>("a", (IHintParameter)new StubParameter()),
                Tuple.Create<string, IHintParameter>("b", (IHintParameter)new StubParameter()),
            },
            Array.Empty<string>(),
            new HashSet<string>());

        var result = NewParser().ParseText("{a}{b}", setting);
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsTrue(segs.Any(s => s.Text == "0"), "First parameter must produce index segment '0'.");
        Assert.IsTrue(segs.Any(s => s.Text == "1"), "Second parameter must produce index segment '1'.");
    }

    [TestMethod]
    public void ParseText_UnrecognisedParameterToken_TreatedAsLiteralText()
    {
        // {unknown} has no matching registered parameter → kept as-is
        var result = NewParser().ParseText("{unknown}", DefaultSetting());
        Assert.AreEqual(1, result.LineInfos.Length, "Unrecognised parameter must not crash.");
        Assert.AreEqual(0, result.Parameters.Length, "No parameters should have been captured.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 13 – Malformed / unknown tag handling
    //   The parser must remain stable and produce output in all cases.
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_UnknownTagName_IgnoredWithNoSideEffects()
    {
        var result = NewParser().ParseText("<thisisnotavalidtag>text</thisisnotavalidtag>", DefaultSetting());
        Assert.AreEqual(1, result.LineInfos.Length, "Unknown tag must not crash.");
        Assert.AreEqual(1, result.LineInfos[0].CharacterInfos.Length);
    }

    [TestMethod]
    public void ParseText_UnclosedTag_StyleActiveUntilEndOfText()
    {
        var result = NewParser().ParseText("<b>unclosed", DefaultSetting());
        Assert.AreEqual(1, result.LineInfos.Length);
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Bold,
            "Unclosed <b> must still apply Bold to following text.");
    }

    [TestMethod]
    public void ParseText_UnmatchedCloseTag_NoExceptionAndLineProduced()
    {
        var result = NewParser().ParseText("</b>text", DefaultSetting());
        Assert.AreEqual(1, result.LineInfos.Length, "Unmatched close tag must not crash.");
    }

    [TestMethod]
    public void ParseText_MultipleExtraCloseTags_StackGuardPreventsUnderflow()
    {
        var result = NewParser().ParseText("<b>text</b></b></b></b>tail", DefaultSetting());
        Assert.AreEqual(1, result.LineInfos.Length, "Multiple extra close tags must not crash.");
    }

    [TestMethod]
    public void ParseText_MalformedTagNoClosingBracket_TreatedAsLiteralText()
    {
        // "<b" without ">" — the tokenizer must not recognise this as a tag
        var result = NewParser().ParseText("<b text", DefaultSetting());
        Assert.AreEqual(1, result.LineInfos.Length, "An unclosed '<' must not crash.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 14 – LineInfo Width / Height sanity
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_PlainText_LineWidthIsPositive()
    {
        var result = NewParser().ParseText("hello", DefaultSetting());
        Assert.IsTrue(result.LineInfos[0].Width > 0f, "Non-empty line must have a positive width.");
    }

    [TestMethod]
    public void ParseText_EmptyLine_LineWidthIsZero()
    {
        var result = NewParser().ParseText(string.Empty, DefaultSetting());
        Assert.AreEqual(0f, result.LineInfos[0].Width, 0.001f, "Empty line must have zero width.");
    }

    [TestMethod]
    public void ParseText_SpacePlaceholder_ContributesToLineWidth()
    {
        float withoutSpace = NewParser().ParseText("text", DefaultSetting()).LineInfos[0].Width;
        float withSpace = NewParser().ParseText("<space=50>text", DefaultSetting()).LineInfos[0].Width;

        Assert.IsTrue(withSpace > withoutSpace,
            "<space=50> must increase the line width compared to plain text.");
    }

    [TestMethod]
    public void ParseText_LineHeightTagActive_LineInfoHeightReflectsIt()
    {
        // When LineStyle.LineHeight is set, LineInfo.Height must return exactly that.
        var result = NewParser().ParseText("<line-height=99>text", DefaultSetting());
        Assert.AreEqual(99f, result.LineInfos[0].Height, 0.001f,
            "LineInfo.Height must equal the active line-height value.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 15 – Thread safety
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    [Timeout(10_000)]
    public async Task ParseText_ConcurrentCallsOnSingleInstance_AllReturnCorrectResults()
    {
        var parser = NewParser();
        var errors = new ConcurrentBag<Exception>();
        int ok = 0;
        const int N = 64;

        await Task.WhenAll(Enumerable.Range(0, N).Select(_ => Task.Run(() =>
        {
            try
            {
                var result = parser.ParseText("<b>line1</b>\n<color=red>line2</color>", DefaultSetting());

                if (result.LineInfos.Length != 2)
                    throw new InvalidOperationException($"Expected 2 lines, got {result.LineInfos.Length}.");
                if (!result.LineInfos[0].CharacterInfos[0].Style.Bold)
                    throw new InvalidOperationException("Expected Bold on line-0 segment.");
                if (result.LineInfos[1].CharacterInfos[0].Style.Color.Red != 255)
                    throw new InvalidOperationException("Expected red colour on line-1 segment.");

                Interlocked.Increment(ref ok);
            }
            catch (Exception ex) { errors.Add(ex); }
        })));

        Assert.AreEqual(0, errors.Count,
            $"Concurrent errors: {string.Join(" | ", errors.Select(e => e.Message))}");
        Assert.AreEqual(N, ok, "All concurrent parses must succeed.");
    }

    [TestMethod]
    [Timeout(10_000)]
    public async Task ParseText_ConcurrentCallsOnDistinctInstances_AllReturnCorrectResults()
    {
        var errors = new ConcurrentBag<Exception>();
        int ok = 0;
        const int N = 64;

        await Task.WhenAll(Enumerable.Range(0, N).Select(_ => Task.Run(() =>
        {
            try
            {
                var result = NewParser().ParseText("A\nB\nC", DefaultSetting());
                if (result.LineInfos.Length != 3)
                    throw new InvalidOperationException($"Expected 3 lines, got {result.LineInfos.Length}.");
                Interlocked.Increment(ref ok);
            }
            catch (Exception ex) { errors.Add(ex); }
        })));

        Assert.AreEqual(0, errors.Count,
            $"Errors on distinct instances: {string.Join(" | ", errors.Select(e => e.Message))}");
        Assert.AreEqual(N, ok, "All concurrent parses on distinct instances must succeed.");
    }
}