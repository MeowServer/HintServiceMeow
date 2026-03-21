// ─────────────────────────────────────────────────────────────────────────────
// RichTextParserTests.cs  –  comprehensive + verbose diagnostic logging
//
// Every Assert failure message includes a full dump of the actual
// RichTextParserResult so that failures are self-explanatory in the test log.
//
// ══ KNOWN BUGS UNDER TEST ═══════════════════════════════════════════════════
//
//  BUG-1  ParseText calls Reset() BEFORE lineInfos.ToArray(), so every call
//         returns empty LineInfo[].
//
//  BUG-2  HandleOpenTag checks !IsNullOrEmpty(tagName) instead of value, so
//         value-less tags like <b> are serialised as "<b=>" in CleanText.
//
// ─────────────────────────────────────────────────────────────────────────────

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
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
    // ═════════════════════════════════════════════════════════════════════════
    // ① Diagnostic dump helpers
    //    Dump() converts a RichTextParserResult into a human-readable block
    //    that is embedded in every Assert failure message.
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Produces a full, human-readable representation of a
    /// <see cref="RichTextParserResult"/> for use in Assert failure messages.
    /// </summary>
    private static string Dump(RichTextParserResult r)
    {
        var sb = new StringBuilder();
        sb.AppendLine();
        sb.AppendLine("══ RichTextParserResult dump ══════════════════════════════════");
        sb.AppendLine($"  LineInfos.Length  : {r.LineInfos.Length}");
        sb.AppendLine($"  Parameters.Length : {r.Parameters.Length}");
        sb.AppendLine($"  ParameterIndex    : {r.ParameterIndex}");

        for (int li = 0; li < r.LineInfos.Length; li++)
        {
            LineInfo line = r.LineInfos[li];
            sb.AppendLine($"  ── Line[{li}] ─────────────────────────────────────────────");
            sb.AppendLine($"     CleanText          : \"{line.CleanText}\"");
            sb.AppendLine($"     Width              : {line.Width:F3}");
            sb.AppendLine($"     Height             : {line.Height:F3}");
            sb.AppendLine($"     CharacterInfos.Len : {line.CharacterInfos.Length}");
            sb.AppendLine($"     Style.Alignment    : {line.Style.Alignment}");
            sb.AppendLine($"     Style.Indent       : {line.Style.Indent:F3}");
            sb.AppendLine($"     Style.MarginLeft   : {line.Style.MarginLeft:F3}");
            sb.AppendLine($"     Style.MarginRight  : {line.Style.MarginRight:F3}");
            sb.AppendLine($"     Style.LineHeight   : {(line.Style.LineHeight.HasValue ? line.Style.LineHeight.Value.ToString("F3") : "null")}");
            sb.AppendLine($"     Style.MaxWidth     : {line.Style.MaxWidth:F3}");

            for (int si = 0; si < line.CharacterInfos.Length; si++)
            {
                TextSegment seg = line.CharacterInfos[si];
                TextSegmentStyle s = seg.Style;
                sb.AppendLine($"     Seg[{si}] Text=\"{seg.Text}\"  Width={seg.Width:F3}  Height={seg.Height:F3}");
                sb.AppendLine($"            FontSize={s.FontSize:F2}  Bold={s.Bold}  Italic={s.Italic}  Underline={s.Underline}  Strikethrough={s.Strikethrough}");
                sb.AppendLine($"            Subscript={s.Subscript}  Superscript={s.Superscript}  Smallcaps(n/a on seg)");
                sb.AppendLine($"            Color=({s.Color.Red},{s.Color.Green},{s.Color.Blue},{s.Color.Alpha})  Alpha={NullableF(s.Alpha)}");
                sb.AppendLine($"            Mark={DumpColor(s.Mark)}  Font={s.Font ?? "null"}  FontWeight={s.FontWeight?.ToString() ?? "null"}");
                sb.AppendLine($"            CharSpace={NullableF(s.CharSpace)}  Monospace={NullableF(s.Monospace)}  VOffset={NullableF(s.VOffset)}  Rotate={NullableF(s.Rotate)}");
            }
        }

        sb.AppendLine("══════════════════════════════════════════════════════════════");
        return sb.ToString();
    }

    private static string NullableF(float? v) => v.HasValue ? v.Value.ToString("F3") : "null";

    private static string DumpColor(Color? c)
    {
        if (!c.HasValue) return "null";
        return $"({c.Value.Red},{c.Value.Green},{c.Value.Blue},{c.Value.Alpha})";
    }

    // ═════════════════════════════════════════════════════════════════════════
    // ② Factory helpers
    // ═════════════════════════════════════════════════════════════════════════

    private static RichTextParser NewParser() => new RichTextParser();

    private static RichTextParserSetting DefaultSetting(
        string[]? illegalTags = null,
        HashSet<string>? ignoreTags = null,
        TextMeshStyle? style = null) =>
        new RichTextParserSetting(
            style ?? TextMeshStyle.Default,
            Array.Empty<Tuple<string, IParameter>>(),
            illegalTags ?? Array.Empty<string>(),
            ignoreTags ?? new HashSet<string>(),
            false);

    private static TextSegmentStyle SegStyle(RichTextParserResult r, int line = 0, int seg = 0)
        => r.LineInfos[line].CharacterInfos[seg].Style;

    private static LineStyle LineStyle(RichTextParserResult r, int line = 0)
        => r.LineInfos[line].Style;

    // ═════════════════════════════════════════════════════════════════════════
    // ③ Stub IHintParameter
    // ═════════════════════════════════════════════════════════════════════════

    private sealed class StubParameter : IParameter
    {
        public global::Hints.HintParameter GetScpslHintParameter() =>
            throw new NotSupportedException("Stub only.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 1 – Basic output structure  (BUG-1 regressions)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_PlainText_ReturnsSingleLine()
    {
        const string input = "hello";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\n" +
            $"Expected 1 line but got {result.LineInfos.Length}. " +
            $"(BUG-1: Reset() must not clear lineInfos before ToArray())" +
            Dump(result));
    }

    [TestMethod]
    public void ParseText_PlainText_SingleSegmentHasCorrectText()
    {
        const string input = "hello";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos[0].CharacterInfos.Length,
            $"Input: \"{input}\"\nExpected 1 segment." + Dump(result));

        Assert.AreEqual("hello", result.LineInfos[0].CharacterInfos[0].Text,
            $"Input: \"{input}\"\nSegment text mismatch." + Dump(result));
    }

    [TestMethod]
    public void ParseText_EmptyString_ReturnsOneLineWithNoSegments()
    {
        const string input = "";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\nExpected 1 line." + Dump(result));

        Assert.AreEqual(0, result.LineInfos[0].CharacterInfos.Length,
            $"Input: \"{input}\"\nEmpty string line must have 0 segments." + Dump(result));
    }

    [TestMethod]
    public void ParseText_CalledTwice_EachCallReturnsIndependentResults()
    {
        var parser = NewParser();

        var r1 = parser.ParseText("first", DefaultSetting());
        var r2 = parser.ParseText("second", DefaultSetting());

        Assert.AreEqual(1, r1.LineInfos.Length,
            "First call must return 1 line.\n" + Dump(r1));
        Assert.AreEqual(1, r2.LineInfos.Length,
            "Second call must return 1 line.\n" + Dump(r2));
        Assert.AreEqual("first", r1.LineInfos[0].CharacterInfos[0].Text,
            "First call text mismatch.\n" + Dump(r1));
        Assert.AreEqual("second", r2.LineInfos[0].CharacterInfos[0].Text,
            "Second call text mismatch.\n" + Dump(r2));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 2 – Line breaks
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_NewlineCharacter_ProducesTwoLines()
    {
        const string input = "A\nB";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(2, result.LineInfos.Length,
            $"Input: \"{input}\"\nExpected 2 lines." + Dump(result));
    }

    [TestMethod]
    public void ParseText_NewlineCharacter_EachLineContainsCorrectText()
    {
        const string input = "line1\nline2";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual("line1", result.LineInfos[0].CharacterInfos[0].Text,
            $"Input: \"{input}\"\nLine[0] segment text mismatch." + Dump(result));
        Assert.AreEqual("line2", result.LineInfos[1].CharacterInfos[0].Text,
            $"Input: \"{input}\"\nLine[1] segment text mismatch." + Dump(result));
    }

    [TestMethod]
    public void ParseText_BrTag_TreatedAsLineBreak()
    {
        const string input = "A<br>B";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(2, result.LineInfos.Length,
            $"Input: \"{input}\"\n<br> must produce a line break." + Dump(result));
    }

    [TestMethod]
    public void ParseText_EscapeSequenceBackslashN_TreatedAsLineBreak()
    {
        const string input = @"A\nB";   // two-character sequence \ n
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(2, result.LineInfos.Length,
            $"Input: \"{input}\"\n\\n escape must be treated as a line break." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MultipleNewlines_CorrectLineCount()
    {
        const string input = "A\nB\nC\nD";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(4, result.LineInfos.Length,
            $"Input: \"{input}\"\nExpected 4 lines." + Dump(result));
    }

    [TestMethod]
    public void ParseText_TrailingNewline_ProducesExtraEmptyLastLine()
    {
        const string input = "A\n";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(2, result.LineInfos.Length,
            $"Input: \"{input}\"\nTrailing newline must produce 2 lines." + Dump(result));
        Assert.AreEqual(0, result.LineInfos[1].CharacterInfos.Length,
            $"Input: \"{input}\"\nLast line (after trailing newline) must be empty." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 3 – CleanText correctness  (BUG-2 regressions)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_BoldOpenTag_CleanTextContainsWellFormedTag()
    {
        const string input = "<b>hello";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        Assert.IsFalse(clean.Contains("<b=>"),
            $"Input: \"{input}\"\nBUG-2: found malformed \"<b=>\" in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
        StringAssert.Contains(clean, "<b>",
            $"Input: \"{input}\"\nExpected \"<b>\" in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_ItalicOpenTag_CleanTextContainsWellFormedTag()
    {
        const string input = "<i>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        Assert.IsFalse(clean.Contains("<i=>"),
            $"Input: \"{input}\"\nBUG-2: found malformed \"<i=>\".\nCleanText=\"{clean}\"" + Dump(result));
        StringAssert.Contains(clean, "<i>",
            $"Input: \"{input}\"\nExpected \"<i>\" in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_UnderlineOpenTag_CleanTextContainsWellFormedTag()
    {
        const string input = "<u>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        Assert.IsFalse(clean.Contains("<u=>"),
            $"Input: \"{input}\"\nBUG-2: found malformed \"<u=>\".\nCleanText=\"{clean}\"" + Dump(result));
        StringAssert.Contains(clean, "<u>",
            $"Input: \"{input}\"\nExpected \"<u>\".\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_StrikethroughOpenTag_CleanTextContainsWellFormedTag()
    {
        const string input = "<s>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        Assert.IsFalse(clean.Contains("<s=>"),
            $"Input: \"{input}\"\nBUG-2: found \"<s=>\".\nCleanText=\"{clean}\"" + Dump(result));
        StringAssert.Contains(clean, "<s>",
            $"Input: \"{input}\"\nExpected \"<s>\".\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_SubscriptOpenTag_CleanTextContainsWellFormedTag()
    {
        const string input = "<sub>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        Assert.IsFalse(clean.Contains("<sub=>"),
            $"Input: \"{input}\"\nBUG-2: found \"<sub=>\".\nCleanText=\"{clean}\"" + Dump(result));
        StringAssert.Contains(clean, "<sub>",
            $"Input: \"{input}\"\nExpected \"<sub>\".\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_SuperscriptOpenTag_CleanTextContainsWellFormedTag()
    {
        const string input = "<sup>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        Assert.IsFalse(clean.Contains("<sup=>"),
            $"Input: \"{input}\"\nBUG-2: found \"<sup=>\".\nCleanText=\"{clean}\"" + Dump(result));
        StringAssert.Contains(clean, "<sup>",
            $"Input: \"{input}\"\nExpected \"<sup>\".\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_ColorOpenTagWithValue_CleanTextContainsEqualSignAndValue()
    {
        const string input = "<color=red>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        StringAssert.Contains(clean, "<color=red>",
            $"Input: \"{input}\"\nValue-bearing tag must emit \"=value\".\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_SizeOpenTagWithValue_CleanTextContainsEqualSignAndValue()
    {
        const string input = "<size=24>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        StringAssert.Contains(clean, "<size=24>",
            $"Input: \"{input}\"\nExpected \"<size=24>\".\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_SpaceSelfCloseTag_CleanTextIsWellFormed()
    {
        const string input = "<space=10>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        StringAssert.Contains(clean, "<space=10>",
            $"Input: \"{input}\"\nExpected \"<space=10>\".\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 4 – Counter-based style tags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_BoldTag_SegmentIsBold()
    {
        const string input = "<b>text</b>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.IsTrue(SegStyle(result).Bold,
            $"Input: \"{input}\"\nExpected Seg[0].Bold=true." + Dump(result));
    }

    [TestMethod]
    public void ParseText_BoldCloseTag_StyleRevertsAfterTag()
    {
        const string input = "<b>bold</b>plain";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsTrue(segs[0].Style.Bold,
            $"Input: \"{input}\"\nSeg[0] (\"bold\") must be Bold=true." + Dump(result));
        Assert.IsFalse(segs[1].Style.Bold,
            $"Input: \"{input}\"\nSeg[1] (\"plain\") must be Bold=false after </b>." + Dump(result));
    }

    [TestMethod]
    public void ParseText_NestedBoldTags_StillBoldAfterFirstClose()
    {
        const string input = "<b><b>inner</b>middle</b>outer";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsTrue(segs[0].Style.Bold,
            $"Input: \"{input}\"\nSeg[0] \"inner\" (Bold=2) must be bold." + Dump(result));
        Assert.IsTrue(segs[1].Style.Bold,
            $"Input: \"{input}\"\nSeg[1] \"middle\" (Bold=1) must still be bold." + Dump(result));
        Assert.IsFalse(segs[2].Style.Bold,
            $"Input: \"{input}\"\nSeg[2] \"outer\" (Bold=0) must not be bold." + Dump(result));
    }

    [TestMethod]
    public void ParseText_ExtraCloseBoldTag_DoesNotThrowOrUnderflow()
    {
        const string input = "<b>text</b></b></b>extra";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\nParser must not crash on extra close tags." + Dump(result));
    }

    [TestMethod]
    public void ParseText_ItalicTag_SegmentIsItalic()
    {
        const string input = "<i>text</i>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.IsTrue(SegStyle(result).Italic,
            $"Input: \"{input}\"\nExpected Seg[0].Italic=true." + Dump(result));
    }

    [TestMethod]
    public void ParseText_ItalicCloseTag_StyleReverts()
    {
        const string input = "<i>styled</i>plain";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsTrue(segs[0].Style.Italic,
            $"Input: \"{input}\"\nSeg[0] must be italic." + Dump(result));
        Assert.IsFalse(segs[1].Style.Italic,
            $"Input: \"{input}\"\nSeg[1] must NOT be italic after </i>." + Dump(result));
    }

    [TestMethod]
    public void ParseText_UnderlineTag_SegmentIsUnderline()
    {
        const string input = "<u>text</u>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.IsTrue(SegStyle(result).Underline,
            $"Input: \"{input}\"\nExpected Seg[0].Underline=true." + Dump(result));
    }

    [TestMethod]
    public void ParseText_UnderlineCloseTag_StyleReverts()
    {
        const string input = "<u>under</u>plain";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsTrue(segs[0].Style.Underline,
            $"Input: \"{input}\"\nSeg[0] must have Underline=true." + Dump(result));
        Assert.IsFalse(segs[1].Style.Underline,
            $"Input: \"{input}\"\nSeg[1] must have Underline=false after </u>." + Dump(result));
    }

    [TestMethod]
    public void ParseText_StrikethroughTag_SegmentIsStrikethrough()
    {
        const string input = "<s>text</s>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.IsTrue(SegStyle(result).Strikethrough,
            $"Input: \"{input}\"\nExpected Seg[0].Strikethrough=true." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SubscriptTag_SegmentSubscriptIsPositive()
    {
        const string input = "<sub>text</sub>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.IsTrue(SegStyle(result).Subscript > 0,
            $"Input: \"{input}\"\nExpected Seg[0].Subscript>0, got {SegStyle(result).Subscript}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SubscriptCloseTag_SubscriptReverts()
    {
        const string input = "<sub>sub</sub>normal";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsTrue(segs[0].Style.Subscript > 0,
            $"Input: \"{input}\"\nSeg[0] must have Subscript>0, got {segs[0].Style.Subscript}." + Dump(result));
        Assert.AreEqual(0, segs[1].Style.Subscript,
            $"Input: \"{input}\"\nSeg[1] must have Subscript=0 after </sub>, got {segs[1].Style.Subscript}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SuperscriptTag_SegmentSuperscriptIsPositive()
    {
        const string input = "<sup>text</sup>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.IsTrue(SegStyle(result).Superscript > 0,
            $"Input: \"{input}\"\nExpected Seg[0].Superscript>0, got {SegStyle(result).Superscript}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SuperscriptCloseTag_SuperscriptReverts()
    {
        const string input = "<sup>sup</sup>normal";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsTrue(segs[0].Style.Superscript > 0,
            $"Input: \"{input}\"\nSeg[0] must have Superscript>0, got {segs[0].Style.Superscript}." + Dump(result));
        Assert.AreEqual(0, segs[1].Style.Superscript,
            $"Input: \"{input}\"\nSeg[1] must have Superscript=0 after </sup>, got {segs[1].Style.Superscript}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_BoldAndItalicNested_BothStylesActiveOnInnerSegment()
    {
        const string input = "<b><i>text</i></b>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.IsTrue(SegStyle(result).Bold,
            $"Input: \"{input}\"\nExpected Seg[0].Bold=true." + Dump(result));
        Assert.IsTrue(SegStyle(result).Italic,
            $"Input: \"{input}\"\nExpected Seg[0].Italic=true." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 5 – Stack-based style tags
    // ═════════════════════════════════════════════════════════════════════════

    // ── color ────────────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseText_ColorNamedRed_SegmentColorIsRed()
    {
        const string input = "<color=red>text</color>";
        var result = NewParser().ParseText(input, DefaultSetting());
        Color c = SegStyle(result).Color;

        Assert.AreEqual(255, c.Red,
            $"Input: \"{input}\"\nExpected Color.Red=255, got {c.Red}." + Dump(result));
        Assert.AreEqual(0, c.Green,
            $"Input: \"{input}\"\nExpected Color.Green=0, got {c.Green}." + Dump(result));
        Assert.AreEqual(0, c.Blue,
            $"Input: \"{input}\"\nExpected Color.Blue=0, got {c.Blue}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_ColorHex6Char_ParsedCorrectly()
    {
        const string input = "<color=#1A2B3C>text</color>";
        var result = NewParser().ParseText(input, DefaultSetting());
        Color c = SegStyle(result).Color;

        Assert.AreEqual((byte)0x1A, c.Red,
            $"Input: \"{input}\"\nExpected Red=0x1A({0x1A}), got {c.Red}." + Dump(result));
        Assert.AreEqual((byte)0x2B, c.Green,
            $"Input: \"{input}\"\nExpected Green=0x2B({0x2B}), got {c.Green}." + Dump(result));
        Assert.AreEqual((byte)0x3C, c.Blue,
            $"Input: \"{input}\"\nExpected Blue=0x3C({0x3C}), got {c.Blue}." + Dump(result));
        Assert.AreEqual((byte)255, c.Alpha,
            $"Input: \"{input}\"\nExpected Alpha=255 for 6-char hex, got {c.Alpha}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_ColorHex8Char_ParsedWithAlpha()
    {
        const string input = "<color=#FF0080AA>text</color>";
        var result = NewParser().ParseText(input, DefaultSetting());
        Color c = SegStyle(result).Color;

        Assert.AreEqual((byte)0xFF, c.Red, $"Input: \"{input}\"\nRed mismatch, got {c.Red}." + Dump(result));
        Assert.AreEqual((byte)0x00, c.Green, $"Input: \"{input}\"\nGreen mismatch, got {c.Green}." + Dump(result));
        Assert.AreEqual((byte)0x80, c.Blue, $"Input: \"{input}\"\nBlue mismatch, got {c.Blue}." + Dump(result));
        Assert.AreEqual((byte)0xAA, c.Alpha, $"Input: \"{input}\"\nAlpha mismatch, got {c.Alpha}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_ColorHex3CharShorthand_ExpandedCorrectly()
    {
        const string input = "<color=#F00>text</color>";
        var result = NewParser().ParseText(input, DefaultSetting());
        Color c = SegStyle(result).Color;

        Assert.AreEqual((byte)0xFF, c.Red,
            $"Input: \"{input}\"\n#F00 → Red=0xFF, got {c.Red}." + Dump(result));
        Assert.AreEqual((byte)0x00, c.Green,
            $"Input: \"{input}\"\n#F00 → Green=0x00, got {c.Green}." + Dump(result));
        Assert.AreEqual((byte)0x00, c.Blue,
            $"Input: \"{input}\"\n#F00 → Blue=0x00, got {c.Blue}." + Dump(result));
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
        string input = $"<color={name}>x</color>";
        var result = NewParser().ParseText(input, DefaultSetting());
        Color c = SegStyle(result).Color;

        Assert.AreEqual((byte)r, c.Red,
            $"Input: \"{input}\"\nRed mismatch for '{name}': expected {r}, got {c.Red}." + Dump(result));
        Assert.AreEqual((byte)g, c.Green,
            $"Input: \"{input}\"\nGreen mismatch for '{name}': expected {g}, got {c.Green}." + Dump(result));
        Assert.AreEqual((byte)b, c.Blue,
            $"Input: \"{input}\"\nBlue mismatch for '{name}': expected {b}, got {c.Blue}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_NestedColors_InnerColorTakesPrecedence()
    {
        const string input = "<color=blue><color=red>inner</color>outer</color>";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.AreEqual(255, segs[0].Style.Color.Red,
            $"Input: \"{input}\"\nSeg[0] \"inner\" must be red (Red=255), got Red={segs[0].Style.Color.Red}." + Dump(result));
        Assert.AreEqual(0, segs[0].Style.Color.Blue,
            $"Input: \"{input}\"\nSeg[0] \"inner\" must not be blue (Blue=0), got Blue={segs[0].Style.Color.Blue}." + Dump(result));
        Assert.AreEqual(0, segs[1].Style.Color.Red,
            $"Input: \"{input}\"\nSeg[1] \"outer\" must not be red (Red=0), got Red={segs[1].Style.Color.Red}." + Dump(result));
        Assert.AreEqual(255, segs[1].Style.Color.Blue,
            $"Input: \"{input}\"\nSeg[1] \"outer\" must be blue (Blue=255), got Blue={segs[1].Style.Color.Blue}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_ColorCloseTag_ReturnsToDefaultColor()
    {
        Color def = TextMeshStyle.Default.CharStyle.Color;
        const string input = "<color=red>colored</color>plain";
        var result = NewParser().ParseText(input, DefaultSetting());
        Color after = result.LineInfos[0].CharacterInfos[1].Style.Color;

        Assert.AreEqual(def.Red, after.Red,
            $"Input: \"{input}\"\nAfter </color> Red must revert to {def.Red}, got {after.Red}." + Dump(result));
        Assert.AreEqual(def.Green, after.Green,
            $"Input: \"{input}\"\nAfter </color> Green must revert to {def.Green}, got {after.Green}." + Dump(result));
        Assert.AreEqual(def.Blue, after.Blue,
            $"Input: \"{input}\"\nAfter </color> Blue must revert to {def.Blue}, got {after.Blue}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_InvalidColorValue_ColorNotChanged()
    {
        Color def = TextMeshStyle.Default.CharStyle.Color;
        const string input = "<color=notacolor>text</color>";
        var result = NewParser().ParseText(input, DefaultSetting());
        Color c = SegStyle(result).Color;

        Assert.AreEqual(def.Red, c.Red,
            $"Input: \"{input}\"\nInvalid color must leave Red={def.Red}, got {c.Red}." + Dump(result));
        Assert.AreEqual(def.Green, c.Green,
            $"Input: \"{input}\"\nInvalid color must leave Green={def.Green}, got {c.Green}." + Dump(result));
        Assert.AreEqual(def.Blue, c.Blue,
            $"Input: \"{input}\"\nInvalid color must leave Blue={def.Blue}, got {c.Blue}." + Dump(result));
    }

    // ── size ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseText_SizeInPixels_SegmentFontSizeMatches()
    {
        const string input = "<size=32>text</size>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float fs = SegStyle(result).FontSize;

        Assert.AreEqual(32f, fs, 0.001f,
            $"Input: \"{input}\"\nExpected FontSize=32, got {fs}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SizeInEm_SegmentFontSizeIsRelativeToDefault()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        const string input = "<size=2em>text</size>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float fs = SegStyle(result).FontSize;

        Assert.AreEqual(2f * def, fs, 0.001f,
            $"Input: \"{input}\"\nExpected FontSize={2f * def} (2em of {def}), got {fs}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SizeInPercent_SegmentFontSizeIsPercentOfDefault()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        const string input = "<size=200%>text</size>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float fs = SegStyle(result).FontSize;

        Assert.AreEqual(def * 2f, fs, 0.001f,
            $"Input: \"{input}\"\nExpected FontSize={def * 2f} (200% of {def}), got {fs}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SizeCloseTag_FontSizeReverts()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        const string input = "<size=48>large</size>normal";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.AreEqual(48f, segs[0].Style.FontSize, 0.001f,
            $"Input: \"{input}\"\nSeg[0] must have FontSize=48, got {segs[0].Style.FontSize}." + Dump(result));
        Assert.AreEqual(def, segs[1].Style.FontSize, 0.001f,
            $"Input: \"{input}\"\nSeg[1] must revert to FontSize={def}, got {segs[1].Style.FontSize}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_InvalidSizeValue_FontSizeUnchanged()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        const string input = "<size=notanumber>text</size>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float fs = SegStyle(result).FontSize;

        Assert.AreEqual(def, fs, 0.001f,
            $"Input: \"{input}\"\nInvalid size must leave FontSize={def}, got {fs}." + Dump(result));
    }

    // ── align ─────────────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseText_AlignLeft_LineStyleIsLeft()
    {
        const string input = "<align=left>text";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(HintAlignment.Left, LineStyle(result).Alignment,
            $"Input: \"{input}\"\nExpected Alignment=Left, got {LineStyle(result).Alignment}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_AlignCenter_LineStyleIsCenter()
    {
        const string input = "<align=center>text";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(HintAlignment.Center, LineStyle(result).Alignment,
            $"Input: \"{input}\"\nExpected Alignment=Center, got {LineStyle(result).Alignment}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_AlignRight_LineStyleIsRight()
    {
        const string input = "<align=right>text";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(HintAlignment.Right, LineStyle(result).Alignment,
            $"Input: \"{input}\"\nExpected Alignment=Right, got {LineStyle(result).Alignment}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_AlignJustified_LineStyleIsJustified()
    {
        const string input = "<align=justified>text";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(HintAlignment.Justified, LineStyle(result).Alignment,
            $"Input: \"{input}\"\nExpected Alignment=Justified, got {LineStyle(result).Alignment}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_AlignFlush_LineStyleIsFlush()
    {
        const string input = "<align=flush>text";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(HintAlignment.Flush, LineStyle(result).Alignment,
            $"Input: \"{input}\"\nExpected Alignment=Flush, got {LineStyle(result).Alignment}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_InvalidAlignValue_AlignmentUnchanged()
    {
        HintAlignment def = TextMeshStyle.Default.LineStyle.Alignment;
        const string input = "<align=diagonal>text";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(def, LineStyle(result).Alignment,
            $"Input: \"{input}\"\nInvalid align must leave Alignment={def}, got {LineStyle(result).Alignment}." + Dump(result));
    }

    // ── indent ────────────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseText_IndentInPixels_LineStyleIndentMatches()
    {
        const string input = "<indent=20>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        float indent = LineStyle(result).Indent;

        Assert.AreEqual(20f, indent, 0.001f,
            $"Input: \"{input}\"\nExpected Indent=20, got {indent}." + Dump(result));
    }

    // ── mark ──────────────────────────────────────────────────────────────────

    [TestMethod]
    public void ParseText_MarkNamedRed_SegmentMarkColorIsRed()
    {
        const string input = "<mark=red>text</mark>";
        var result = NewParser().ParseText(input, DefaultSetting());
        Color? mark = SegStyle(result).Mark;

        Assert.IsNotNull(mark,
            $"Input: \"{input}\"\nMark must be non-null." + Dump(result));
        Assert.AreEqual(255, mark!.Value.Red,
            $"Input: \"{input}\"\nMark.Red must be 255, got {mark.Value.Red}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MarkCloseTag_MarkColorReverts()
    {
        Color? def = TextMeshStyle.Default.CharStyle.Mark;
        const string input = "<mark=red>marked</mark>plain";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsNotNull(segs[0].Style.Mark,
            $"Input: \"{input}\"\nSeg[0] Mark must be set inside <mark>." + Dump(result));
        Assert.AreEqual(def, segs[1].Style.Mark,
            $"Input: \"{input}\"\nSeg[1] Mark must revert to {DumpColor(def)}, got {DumpColor(segs[1].Style.Mark)}." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 6 – Single-value style tags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_FontTag_SegmentFontNameSet()
    {
        const string input = "<font=Arial>text</font>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual("Arial", SegStyle(result).Font,
            $"Input: \"{input}\"\nExpected Font=\"Arial\", got \"{SegStyle(result).Font}\"." + Dump(result));
    }

    [TestMethod]
    public void ParseText_FontCloseTag_FontReverts()
    {
        string? def = TextMeshStyle.Default.CharStyle.Font;
        const string input = "<font=Arial>styled</font>plain";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.AreEqual("Arial", segs[0].Style.Font,
            $"Input: \"{input}\"\nSeg[0] Font must be \"Arial\", got \"{segs[0].Style.Font}\"." + Dump(result));
        Assert.AreEqual(def, segs[1].Style.Font,
            $"Input: \"{input}\"\nSeg[1] Font must revert to \"{def}\", got \"{segs[1].Style.Font}\"." + Dump(result));
    }

    [TestMethod]
    public void ParseText_FontWeightTag_SegmentFontWeightSet()
    {
        const string input = "<font-weight=700>text</font-weight>";
        var result = NewParser().ParseText(input, DefaultSetting());
        int? fw = SegStyle(result).FontWeight;

        Assert.AreEqual(700, fw,
            $"Input: \"{input}\"\nExpected FontWeight=700, got {fw}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_FontWeightCloseTag_FontWeightReverts()
    {
        int? def = TextMeshStyle.Default.CharStyle.FontWeight;
        const string input = "<font-weight=700>heavy</font-weight>normal";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.AreEqual(700, segs[0].Style.FontWeight,
            $"Input: \"{input}\"\nSeg[0] FontWeight must be 700, got {segs[0].Style.FontWeight}." + Dump(result));
        Assert.AreEqual(def, segs[1].Style.FontWeight,
            $"Input: \"{input}\"\nSeg[1] FontWeight must revert to {def}, got {segs[1].Style.FontWeight}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_LineHeightTag_ActiveOnLine_LineStyleLineHeightSet()
    {
        const string input = "<line-height=30>text\nafter";
        var result = NewParser().ParseText(input, DefaultSetting());
        float? lh = result.LineInfos[0].Style.LineHeight;

        Assert.IsNotNull(lh,
            $"Input: \"{input}\"\nLine[0] LineHeight must not be null." + Dump(result));
        Assert.AreEqual(30f, lh!.Value, 0.001f,
            $"Input: \"{input}\"\nLine[0] LineHeight must be 30, got {lh.Value}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_LineHeightCloseTag_LineHeightResetsOnNextLine()
    {
        const string input = "<line-height=30>A\nB</line-height>\nC";
        var result = NewParser().ParseText(input, DefaultSetting());

        float? lh0 = result.LineInfos[0].Style.LineHeight;
        Assert.IsNotNull(lh0,
            $"Input: \"{input}\"\nLine[0] LineHeight must not be null." + Dump(result));
        Assert.AreEqual(30f, lh0!.Value, 0.001f,
            $"Input: \"{input}\"\nLine[0] LineHeight must be 30, got {lh0.Value}." + Dump(result));
        Assert.IsNull(result.LineInfos[1].Style.LineHeight,
            $"Input: \"{input}\"\nLine[1] LineHeight must be null after </line-height>, got {result.LineInfos[1].Style.LineHeight}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_LineIndentTag_ActiveOnLine_IndentSet()
    {
        const string input = "<line-indent=10>text\nafter";
        var result = NewParser().ParseText(input, DefaultSetting());
        float indent = result.LineInfos[0].Style.Indent;

        Assert.AreEqual(10f, indent, 0.001f,
            $"Input: \"{input}\"\nLine[0] Indent must be 10, got {indent}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MarginTag_BothMarginsSet()
    {
        const string input = "<margin=15>text\nafter";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(15f, result.LineInfos[0].Style.MarginLeft, 0.001f,
            $"Input: \"{input}\"\nLine[0] MarginLeft must be 15, got {result.LineInfos[0].Style.MarginLeft}." + Dump(result));
        Assert.AreEqual(15f, result.LineInfos[0].Style.MarginRight, 0.001f,
            $"Input: \"{input}\"\nLine[0] MarginRight must be 15, got {result.LineInfos[0].Style.MarginRight}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MarginLeftTag_OnlyLeftMarginAffected()
    {
        const string input = "<margin-left=10>text\nafter";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(10f, result.LineInfos[0].Style.MarginLeft, 0.001f,
            $"Input: \"{input}\"\nMarginLeft must be 10, got {result.LineInfos[0].Style.MarginLeft}." + Dump(result));
        Assert.AreEqual(0f, result.LineInfos[0].Style.MarginRight, 0.001f,
            $"Input: \"{input}\"\nMarginRight must be 0 (unchanged), got {result.LineInfos[0].Style.MarginRight}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MarginRightTag_OnlyRightMarginAffected()
    {
        const string input = "<margin-right=10>text\nafter";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(0f, result.LineInfos[0].Style.MarginLeft, 0.001f,
            $"Input: \"{input}\"\nMarginLeft must be 0 (unchanged), got {result.LineInfos[0].Style.MarginLeft}." + Dump(result));
        Assert.AreEqual(10f, result.LineInfos[0].Style.MarginRight, 0.001f,
            $"Input: \"{input}\"\nMarginRight must be 10, got {result.LineInfos[0].Style.MarginRight}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MarginCloseTag_MarginsResetOnNextLine()
    {
        const string input = "<margin=15>A\nB</margin>\nC";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(15f, result.LineInfos[0].Style.MarginLeft, 0.001f,
            $"Input: \"{input}\"\nLine[0] MarginLeft must be 15, got {result.LineInfos[0].Style.MarginLeft}." + Dump(result));
        Assert.AreEqual(0f, result.LineInfos[1].Style.MarginLeft, 0.001f,
            $"Input: \"{input}\"\nLine[1] MarginLeft must reset to 0, got {result.LineInfos[1].Style.MarginLeft}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_CSpaceTag_SegmentCharSpaceSet()
    {
        const string input = "<cspace=3>text</cspace>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float? cs = SegStyle(result).CharSpace;

        Assert.IsNotNull(cs,
            $"Input: \"{input}\"\nCharSpace must not be null." + Dump(result));
        Assert.AreEqual(3f, cs!.Value, 0.001f,
            $"Input: \"{input}\"\nExpected CharSpace=3, got {cs.Value}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_CSpaceCloseTag_CharSpaceReverts()
    {
        float? def = TextMeshStyle.Default.CharStyle.CharSpace;
        const string input = "<cspace=3>spaced</cspace>normal";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsNotNull(segs[0].Style.CharSpace,
            $"Input: \"{input}\"\nSeg[0] CharSpace must not be null." + Dump(result));
        Assert.AreEqual(3f, segs[0].Style.CharSpace!.Value, 0.001f,
            $"Input: \"{input}\"\nSeg[0] CharSpace must be 3, got {segs[0].Style.CharSpace.Value}." + Dump(result));
        Assert.AreEqual(def, segs[1].Style.CharSpace,
            $"Input: \"{input}\"\nSeg[1] CharSpace must revert to {def}, got {segs[1].Style.CharSpace}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MSpaceTag_SegmentMonospaceSet()
    {
        const string input = "<mspace=10>text</mspace>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float? ms = SegStyle(result).Monospace;

        Assert.IsNotNull(ms,
            $"Input: \"{input}\"\nMonospace must not be null." + Dump(result));
        Assert.AreEqual(10f, ms!.Value, 0.001f,
            $"Input: \"{input}\"\nExpected Monospace=10, got {ms.Value}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MSpaceCloseTag_MonospaceReverts()
    {
        float? def = TextMeshStyle.Default.CharStyle.Monospace;
        const string input = "<mspace=10>mono</mspace>normal";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsNotNull(segs[0].Style.Monospace,
            $"Input: \"{input}\"\nSeg[0] Monospace must not be null." + Dump(result));
        Assert.AreEqual(10f, segs[0].Style.Monospace!.Value, 0.001f,
            $"Input: \"{input}\"\nSeg[0] Monospace must be 10, got {segs[0].Style.Monospace.Value}." + Dump(result));
        Assert.AreEqual(def, segs[1].Style.Monospace,
            $"Input: \"{input}\"\nSeg[1] Monospace must revert to {def}, got {segs[1].Style.Monospace}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_RotateTag_SegmentRotateSet()
    {
        const string input = "<rotate=45>text</rotate>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float? rot = SegStyle(result).Rotate;

        Assert.IsNotNull(rot,
            $"Input: \"{input}\"\nRotate must not be null." + Dump(result));
        Assert.AreEqual(45f, rot!.Value, 0.001f,
            $"Input: \"{input}\"\nExpected Rotate=45, got {rot.Value}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_RotateCloseTag_RotateReverts()
    {
        const string input = "<rotate=45>tilted</rotate>normal";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsNotNull(segs[0].Style.Rotate,
            $"Input: \"{input}\"\nSeg[0] Rotate must not be null." + Dump(result));
        Assert.AreEqual(45f, segs[0].Style.Rotate!.Value, 0.001f,
            $"Input: \"{input}\"\nSeg[0] Rotate must be 45, got {segs[0].Style.Rotate.Value}." + Dump(result));
        Assert.IsNull(segs[1].Style.Rotate,
            $"Input: \"{input}\"\nSeg[1] Rotate must revert to null, got {segs[1].Style.Rotate}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_VOffsetTag_SegmentVOffsetSet()
    {
        const string input = "<voffset=5>text</voffset>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float? vo = SegStyle(result).VOffset;

        Assert.IsNotNull(vo,
            $"Input: \"{input}\"\nVOffset must not be null." + Dump(result));
        Assert.AreEqual(5f, vo!.Value, 0.001f,
            $"Input: \"{input}\"\nExpected VOffset=5, got {vo.Value}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_VOffsetCloseTag_VOffsetReverts()
    {
        const string input = "<voffset=5>up</voffset>normal";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.IsNotNull(segs[0].Style.VOffset,
            $"Input: \"{input}\"\nSeg[0] VOffset must not be null." + Dump(result));
        Assert.AreEqual(5f, segs[0].Style.VOffset!.Value, 0.001f,
            $"Input: \"{input}\"\nSeg[0] VOffset must be 5, got {segs[0].Style.VOffset.Value}." + Dump(result));
        Assert.IsNull(segs[1].Style.VOffset,
            $"Input: \"{input}\"\nSeg[1] VOffset must revert to null, got {segs[1].Style.VOffset}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_WidthTag_LineStyleMaxWidthSet()
    {
        const string input = "<width=400>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        float mw = LineStyle(result).MaxWidth;

        Assert.AreEqual(400f, mw, 0.001f,
            $"Input: \"{input}\"\nExpected MaxWidth=400, got {mw}." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 7 – Boolean tags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_SmallcapsTag_ParserDoesNotThrowAndReturnsLine()
    {
        const string input = "<smallcaps>text</smallcaps>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\n<smallcaps> must not crash." + Dump(result));
        Assert.IsTrue(result.LineInfos[0].CharacterInfos.Length > 0,
            $"Input: \"{input}\"\nAt least one segment expected." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 8 – Noparse mode
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_NoparseMode_InnerTagsDoNotAffectStyle()
    {
        const string input = "<noparse><b>text</b></noparse>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\nExpected 1 line." + Dump(result));

        bool anyBold = result.LineInfos[0].CharacterInfos.Any(s => s.Style.Bold);
        Assert.IsFalse(anyBold,
            $"Input: \"{input}\"\nNo segment should be Bold inside <noparse>." + Dump(result));
    }

    [TestMethod]
    public void ParseText_NoparseMode_TagsAppearsLiterallyInCleanText()
    {
        const string input = "<noparse><b>text</b></noparse>";
        var result = NewParser().ParseText(input, DefaultSetting());
        string clean = result.LineInfos[0].CleanText;

        StringAssert.Contains(clean, "<b>",
            $"Input: \"{input}\"\nLiteral \"<b>\" must appear in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_NoparseCloseTag_ResumesNormalTagParsing()
    {
        const string input = "<noparse></noparse><b>text</b>";
        var result = NewParser().ParseText(input, DefaultSetting());

        bool anyBold = result.LineInfos[0].CharacterInfos.Any(s => s.Style.Bold);
        Assert.IsTrue(anyBold,
            $"Input: \"{input}\"\nAfter </noparse>, <b> must apply Bold." + Dump(result));
    }

    [TestMethod]
    public void ParseText_NoparseWithNestedStylingTags_FontSizeUnchanged()
    {
        float def = TextMeshStyle.Default.CharStyle.FontSize;
        const string input = "<noparse><size=99>text</size></noparse>";
        var result = NewParser().ParseText(input, DefaultSetting());

        bool anyWrongSize = result.LineInfos[0].CharacterInfos
            .Any(s => Math.Abs(s.Style.FontSize - def) > 0.01f);

        Assert.IsFalse(anyWrongSize,
            $"Input: \"{input}\"\nFontSize must remain {def} inside <noparse>." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 9 – Self-closing tags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_AlphaTagHalf_SegmentAlphaIsApproximatelyHalf()
    {
        const string input = "<alpha=#80>text</alpha>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float? alpha = SegStyle(result).Alpha;

        Assert.AreEqual(128f / 255f, alpha!.Value, 0.01f,
            $"Input: \"{input}\"\nExpected Alpha≈{128f / 255f:F4}, got {alpha}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_AlphaTagZero_SegmentAlphaIsZero()
    {
        const string input = "<alpha=#00>text</alpha>";
        var result = NewParser().ParseText(input, DefaultSetting());
        float? alpha = SegStyle(result).Alpha;

        Assert.AreEqual(0f, alpha!.Value, 0.01f,
            $"Input: \"{input}\"\nExpected Alpha=0, got {alpha}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_AlphaCloseTag_AlphaRevertsToDefault()
    {
        const string input = "<alpha=#00>dark</alpha>bright";
        var result = NewParser().ParseText(input, DefaultSetting());
        var segs = result.LineInfos[0].CharacterInfos;

        Assert.AreEqual(0f, segs[0].Style.Alpha!.Value, 0.01f,
            $"Input: \"{input}\"\nSeg[0] Alpha must be 0, got {segs[0].Style.Alpha}." + Dump(result));
        Assert.AreEqual(1f, segs[1].Style.Alpha!.Value, 0.01f,
            $"Input: \"{input}\"\nSeg[1] Alpha must revert to 1, got {segs[1].Style.Alpha}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SpaceTag_InsertsPlaceholderSegment()
    {
        // <space=20> must insert a whitespace placeholder whose Width equals the tag value.
        // TextSegment.CustomWidth has been removed; Width is now stored directly.
        // We identify the placeholder as the segment with Text==" " and Width==20.
        const string input = "<space=20>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        bool hasPlaceholder = result.LineInfos[0].CharacterInfos
            .Any(s => s.Text == " " && Math.Abs(s.Width - 20f) < 0.001f);

        Assert.IsTrue(hasPlaceholder,
            $"Input: \"{input}\"\n<space=20> must produce a placeholder segment with Text=\" \" and Width=20." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SpaceTag_PlaceholderWidthMatchesTagValue()
    {
        const string input = "<space=20>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        TextSegment ph = result.LineInfos[0].CharacterInfos
            .First(s => s.Text == " " && Math.Abs(s.Width - 20f) < 0.001f);

        Assert.AreEqual(20f, ph.Width, 0.001f,
            $"Input: \"{input}\"\nPlaceholder Width must be 20, got {ph.Width}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_PosTag_InsertsPlaceholderWithCorrectWidth()
    {
        const string input = "<pos=50>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        bool hasPlaceholder = result.LineInfos[0].CharacterInfos
            .Any(s => s.Text == " " && Math.Abs(s.Width - 50f) < 0.001f);

        Assert.IsTrue(hasPlaceholder,
            $"Input: \"{input}\"\n<pos=50> must produce a placeholder segment with Text=\" \" and Width=50." + Dump(result));

        TextSegment ph = result.LineInfos[0].CharacterInfos
            .First(s => s.Text == " " && Math.Abs(s.Width - 50f) < 0.001f);
        Assert.AreEqual(50f, ph.Width, 0.001f,
            $"Input: \"{input}\"\nPlaceholder Width must be 50, got {ph.Width}." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 10 – IllegalTags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_IllegalTag_NotPresentInCleanText()
    {
        var setting = DefaultSetting(illegalTags: new[] { "b" });
        const string input = "<b>text</b>";
        var result = NewParser().ParseText(input, setting);
        string clean = result.LineInfos[0].CleanText;

        Assert.IsFalse(clean.Contains("<b"),
            $"Input: \"{input}\"\nIllegal tag <b> must be stripped from CleanText.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_IllegalTag_StyleEffectIsStillApplied()
    {
        var setting = DefaultSetting(illegalTags: new[] { "b" });
        const string input = "<b>text</b>";
        var result = NewParser().ParseText(input, setting);

        Assert.IsTrue(SegStyle(result).Bold,
            $"Input: \"{input}\"\nIllegal <b> must still apply Bold even though it's stripped from CleanText." + Dump(result));
    }

    [TestMethod]
    public void ParseText_IllegalColorTag_ColorEffectAppliedButTagStripped()
    {
        var setting = DefaultSetting(illegalTags: new[] { "color" });
        const string input = "<color=red>text</color>";
        var result = NewParser().ParseText(input, setting);
        string clean = result.LineInfos[0].CleanText;

        Assert.AreEqual(255, SegStyle(result).Color.Red,
            $"Input: \"{input}\"\nIllegal <color=red> must still set Color.Red=255, got {SegStyle(result).Color.Red}." + Dump(result));
        Assert.IsFalse(clean.Contains("<color"),
            $"Input: \"{input}\"\nIllegal <color> must not appear in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 11 – IgnoreTags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_IgnoreTag_TagAppearsLiterallyInCleanText()
    {
        var setting = DefaultSetting(ignoreTags: new HashSet<string> { "b" });
        const string input = "<b>text</b>";
        var result = NewParser().ParseText(input, setting);
        string clean = result.LineInfos[0].CleanText;

        StringAssert.Contains(clean, "<b>",
            $"Input: \"{input}\"\nIgnored <b> must appear literally in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void ParseText_IgnoreTag_StyleNotApplied()
    {
        var setting = DefaultSetting(ignoreTags: new HashSet<string> { "b" });
        const string input = "<b>text</b>";
        var result = NewParser().ParseText(input, setting);

        Assert.IsFalse(result.LineInfos[0].CharacterInfos.Any(s => s.Style.Bold),
            $"Input: \"{input}\"\nIgnored <b> must NOT apply Bold." + Dump(result));
    }

    [TestMethod]
    public void ParseText_IgnoreCloseTag_CloseTagAlsoPassedThrough()
    {
        var setting = DefaultSetting(ignoreTags: new HashSet<string> { "color" });
        const string input = "<color=red>text</color>";
        var result = NewParser().ParseText(input, setting);
        string clean = result.LineInfos[0].CleanText;

        StringAssert.Contains(clean, "</color>",
            $"Input: \"{input}\"\nClose tag of ignored tag must pass through literally.\nCleanText=\"{clean}\"" + Dump(result));
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
            new[] { Tuple.Create<string, IParameter>("foo", param) },
            Array.Empty<string>(),
            new HashSet<string>(),
            false);

        const string input = "{foo}";
        var result = NewParser().ParseText(input, setting);

        Assert.AreEqual(1, result.Parameters.Length,
            $"Input: \"{input}\"\nExpected 1 parameter captured, got {result.Parameters.Length}." + Dump(result));
        Assert.AreSame(param, result.Parameters[0],
            $"Input: \"{input}\"\nCaptured parameter instance mismatch." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SingleParameter_PlaceholderSegmentIsZeroBasedIndex()
    {
        var setting = new RichTextParserSetting(
            TextMeshStyle.Default,
            new[] { Tuple.Create<string, IParameter>("foo", (IParameter)new StubParameter()) },
            Array.Empty<string>(),
            new HashSet<string>(),
            false);

        const string input = "{foo}";
        var result = NewParser().ParseText(input, setting);

        bool hasIndexSeg = result.LineInfos[0].CharacterInfos.Any(s => s.Text == "{0}");
        string allTexts = string.Join(", ", result.LineInfos[0].CharacterInfos.Select(s => $"\"{s.Text}\""));
        Assert.IsTrue(hasIndexSeg,
            $"Input: \"{input}\"\nExpected a segment with Text=\"{{0}}\". Actual segments: [{allTexts}]." + Dump(result));
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
                Tuple.Create<string, IParameter>("a", (IParameter)p0),
                Tuple.Create<string, IParameter>("b", (IParameter)p1),
            },
            Array.Empty<string>(),
            new HashSet<string>(),
            false);

        const string input = "{a} {b}";
        var result = NewParser().ParseText(input, setting);

        Assert.AreEqual(2, result.Parameters.Length,
            $"Input: \"{input}\"\nExpected 2 parameters, got {result.Parameters.Length}." + Dump(result));
        Assert.AreSame(p0, result.Parameters[0],
            $"Input: \"{input}\"\nParameters[0] instance mismatch." + Dump(result));
        Assert.AreSame(p1, result.Parameters[1],
            $"Input: \"{input}\"\nParameters[1] instance mismatch." + Dump(result));
    }

    [TestMethod]
    public void ParseText_TwoParameters_PlaceholderIndicesAreSequential()
    {
        var setting = new RichTextParserSetting(
            TextMeshStyle.Default,
            new[]
            {
                Tuple.Create<string, IParameter>("a", (IParameter)new StubParameter()),
                Tuple.Create<string, IParameter>("b", (IParameter)new StubParameter()),
            },
            Array.Empty<string>(),
            new HashSet<string>(),
            false);

        const string input = "{a}{b}";
        var result = NewParser().ParseText(input, setting);
        var segs = result.LineInfos[0].CharacterInfos;
        string allTexts = string.Join(", ", segs.Select(s => $"\"{s.Text}\""));

        Assert.IsTrue(segs.Any(s => s.Text == "{0}"),
            $"Input: \"{input}\"\nExpected segment \"{{0}}\". Segments: [{allTexts}]." + Dump(result));
        Assert.IsTrue(segs.Any(s => s.Text == "{1}"),
            $"Input: \"{input}\"\nExpected segment \"{{1}}\". Segments: [{allTexts}]." + Dump(result));
    }

    [TestMethod]
    public void ParseText_UnrecognisedParameterToken_TreatedAsLiteralText()
    {
        const string input = "{unknown}";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\nUnrecognised parameter must not crash." + Dump(result));
        Assert.AreEqual(0, result.Parameters.Length,
            $"Input: \"{input}\"\nExpected 0 captured parameters, got {result.Parameters.Length}." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 13 – Malformed / unknown tag handling
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_UnknownTagName_IgnoredWithNoSideEffects()
    {
        const string input = "<thisisnotavalidtag>text</thisisnotavalidtag>";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\nUnknown tag must not crash; expected 1 line." + Dump(result));
        Assert.AreEqual(1, result.LineInfos[0].CharacterInfos.Length,
            $"Input: \"{input}\"\nExpected 1 text segment." + Dump(result));
    }

    [TestMethod]
    public void ParseText_UnclosedTag_StyleActiveUntilEndOfText()
    {
        const string input = "<b>unclosed";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\nExpected 1 line." + Dump(result));
        Assert.IsTrue(result.LineInfos[0].CharacterInfos[0].Style.Bold,
            $"Input: \"{input}\"\nUnclosed <b> must apply Bold to following text." + Dump(result));
    }

    [TestMethod]
    public void ParseText_UnmatchedCloseTag_NoExceptionAndLineProduced()
    {
        const string input = "</b>text";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\nUnmatched close tag must not crash." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MultipleExtraCloseTags_StackGuardPreventsUnderflow()
    {
        const string input = "<b>text</b></b></b></b>tail";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\nMultiple extra close tags must not crash." + Dump(result));
    }

    [TestMethod]
    public void ParseText_MalformedTagNoClosingBracket_TreatedAsLiteralText()
    {
        const string input = "<b text";
        var result = NewParser().ParseText(input, DefaultSetting());

        Assert.AreEqual(1, result.LineInfos.Length,
            $"Input: \"{input}\"\nUnclosed '<' must not crash." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 14 – LineInfo Width / Height sanity
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void ParseText_PlainText_LineWidthIsPositive()
    {
        const string input = "hello";
        var result = NewParser().ParseText(input, DefaultSetting());
        float w = result.LineInfos[0].Width;

        Assert.IsTrue(w > 0f,
            $"Input: \"{input}\"\nNon-empty line must have Width>0, got {w}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_EmptyLine_LineWidthIsZero()
    {
        const string input = "";
        var result = NewParser().ParseText(input, DefaultSetting());
        float w = result.LineInfos[0].Width;

        Assert.AreEqual(0f, w, 0.001f,
            $"Input: \"{input}\"\nEmpty line must have Width=0, got {w}." + Dump(result));
    }

    [TestMethod]
    public void ParseText_SpacePlaceholder_ContributesToLineWidth()
    {
        const string inputWithout = "text";
        const string inputWith = "<space=50>text";

        float withoutSpace = NewParser().ParseText(inputWithout, DefaultSetting()).LineInfos[0].Width;
        float withSpace = NewParser().ParseText(inputWith, DefaultSetting()).LineInfos[0].Width;

        Assert.IsTrue(withSpace > withoutSpace,
            $"Input: \"{inputWith}\" vs \"{inputWithout}\"\n" +
            $"Width with <space=50> ({withSpace:F3}) must exceed width without ({withoutSpace:F3}).");
    }

    [TestMethod]
    public void ParseText_LineHeightTagActive_LineInfoHeightReflectsIt()
    {
        const string input = "<line-height=99>text";
        var result = NewParser().ParseText(input, DefaultSetting());
        float h = result.LineInfos[0].Height;

        Assert.AreEqual(99f, h, 0.001f,
            $"Input: \"{input}\"\nLineInfo.Height must equal active line-height=99, got {h}." + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 15 – Thread safety
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    [Timeout(10_000)]
    public async Task ParseText_ConcurrentCallsOnSingleInstance_AllReturnCorrectResults()
    {
        var parser = NewParser();
        var errors = new ConcurrentBag<string>();
        int ok = 0;
        const int N = 64;

        await Task.WhenAll(Enumerable.Range(0, N).Select(_ => Task.Run(() =>
        {
            try
            {
                var result = parser.ParseText("<b>line1</b>\n<color=red>line2</color>", DefaultSetting());

                if (result.LineInfos.Length != 2)
                {
                    errors.Add($"Expected 2 lines, got {result.LineInfos.Length}.\n{Dump(result)}");
                    return;
                }
                if (!result.LineInfos[0].CharacterInfos[0].Style.Bold)
                {
                    errors.Add($"Expected Bold on Line[0].Seg[0].\n{Dump(result)}");
                    return;
                }
                if (result.LineInfos[1].CharacterInfos[0].Style.Color.Red != 255)
                {
                    errors.Add($"Expected Color.Red=255 on Line[1].Seg[0], got {result.LineInfos[1].CharacterInfos[0].Style.Color.Red}.\n{Dump(result)}");
                    return;
                }
                Interlocked.Increment(ref ok);
            }
            catch (Exception ex)
            {
                errors.Add($"Exception: {ex.GetType().Name}: {ex.Message}");
            }
        })));

        Assert.AreEqual(0, errors.Count,
            $"{errors.Count} concurrent task(s) failed:\n" + string.Join("\n---\n", errors.Take(5)));
        Assert.AreEqual(N, ok, $"Expected {N} successful parses, got {ok}.");
    }

    [TestMethod]
    [Timeout(10_000)]
    public async Task ParseText_ConcurrentCallsOnDistinctInstances_AllReturnCorrectResults()
    {
        var errors = new ConcurrentBag<string>();
        int ok = 0;
        const int N = 64;

        await Task.WhenAll(Enumerable.Range(0, N).Select(_ => Task.Run(() =>
        {
            try
            {
                var result = NewParser().ParseText("A\nB\nC", DefaultSetting());
                if (result.LineInfos.Length != 3)
                {
                    errors.Add($"Expected 3 lines, got {result.LineInfos.Length}.\n{Dump(result)}");
                    return;
                }
                Interlocked.Increment(ref ok);
            }
            catch (Exception ex)
            {
                errors.Add($"Exception: {ex.GetType().Name}: {ex.Message}");
            }
        })));

        Assert.AreEqual(0, errors.Count,
            $"{errors.Count} distinct-instance concurrent task(s) failed:\n" + string.Join("\n---\n", errors.Take(5)));
        Assert.AreEqual(N, ok, $"Expected {N} successful parses, got {ok}.");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 16 – Cache
    // ═════════════════════════════════════════════════════════════════════════
    [TestMethod]
    public async Task ParseText_HasCachedResult_ReturnCached()
    {
        RichTextParser parser = NewParser();

        Stopwatch stopwatch = Stopwatch.StartNew();
        parser.ParseText("cached - *IASDHFOIPA#RG(CGB97uAG#R", DefaultSetting()); // warm up cache
        TimeSpan timeUncached = stopwatch.Elapsed;
        stopwatch.Restart();
        parser.ParseText("cached - *IASDHFOIPA#RG(CGB97uAG#R", DefaultSetting()); // should hit cache
        TimeSpan timeCached = stopwatch.Elapsed;

        Assert.IsTrue(timeUncached > timeCached,
            $"Expected cached parse to be faster, got uncached {timeUncached} ms while cached {timeCached} ms");
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Section 17 – Close Unclosed Tags
    // ═════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Creates a setting with CloseUnclosedTags = true (the feature under test).
    /// </summary>
    private static RichTextParserSetting ClosingSetting(
        string[]? illegalTags = null,
        HashSet<string>? ignoreTags = null,
        TextMeshStyle? style = null)
    {
        var setting = new RichTextParserSetting(
            style ?? TextMeshStyle.Default,
            Array.Empty<Tuple<string, IParameter>>(),
            illegalTags ?? Array.Empty<string>(),
            ignoreTags ?? new HashSet<string>(),
            true);
        return setting;
    }

    /// <summary>
    /// Creates a setting with CloseUnclosedTags = false (baseline / control).
    /// </summary>
    private static RichTextParserSetting NonClosingSetting(
        string[]? illegalTags = null,
        HashSet<string>? ignoreTags = null) =>
        new RichTextParserSetting(
            TextMeshStyle.Default,
            Array.Empty<Tuple<string, IParameter>>(),
            illegalTags ?? Array.Empty<string>(),
            ignoreTags ?? new HashSet<string>(),
            false);

    /// <summary>
    /// Returns the CleanText of the last line (where close tags are emitted).
    /// </summary>
    private static string LastCleanText(RichTextParserResult r)
        => r.LineInfos[r.LineInfos.Length - 1].CleanText;

    // ═════════════════════════════════════════════════════════════════════════
    // Counter-based tags: single unclosed
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_Bold_CleanTextContainsCloseTag()
    {
        const string input = "<b>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</b>",
            $"Input: \"{input}\"\nUnclosed <b> must be closed in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Italic_CleanTextContainsCloseTag()
    {
        const string input = "<i>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</i>",
            $"Input: \"{input}\"\nUnclosed <i> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Underline_CleanTextContainsCloseTag()
    {
        const string input = "<u>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</u>",
            $"Input: \"{input}\"\nUnclosed <u> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Strikethrough_CleanTextContainsCloseTag()
    {
        const string input = "<s>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</s>",
            $"Input: \"{input}\"\nUnclosed <s> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Subscript_CleanTextContainsCloseTag()
    {
        const string input = "<sub>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</sub>",
            $"Input: \"{input}\"\nUnclosed <sub> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Superscript_CleanTextContainsCloseTag()
    {
        const string input = "<sup>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</sup>",
            $"Input: \"{input}\"\nUnclosed <sup> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_AllCaps_CleanTextContainsCloseTag()
    {
        const string input = "<allcaps>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</allcaps>",
            $"Input: \"{input}\"\nUnclosed <allcaps> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Lowercase_CleanTextContainsCloseTag()
    {
        const string input = "<lowercase>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</lowercase>",
            $"Input: \"{input}\"\nUnclosed <lowercase> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Uppercase_CleanTextContainsCloseTag()
    {
        const string input = "<uppercase>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</uppercase>",
            $"Input: \"{input}\"\nUnclosed <uppercase> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Counter-based tags: nested (multiple close tags)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_DoubleBold_TwoCloseTagsEmitted()
    {
        const string input = "<b><b>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        int count = CountOccurrences(clean, "</b>");
        Assert.AreEqual(2, count,
            $"Input: \"{input}\"\nExpected 2× </b>, found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_TripleItalic_ThreeCloseTagsEmitted()
    {
        const string input = "<i><i><i>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        int count = CountOccurrences(clean, "</i>");
        Assert.AreEqual(3, count,
            $"Input: \"{input}\"\nExpected 3× </i>, found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_PartiallyClosedBold_OnlyRemainingClosed()
    {
        // Open 3, close 1 manually → 2 should be auto-closed
        const string input = "<b><b><b>text</b>";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        // Total </b> = 1 (manual) + 2 (auto) = 3
        int count = CountOccurrences(clean, "</b>");
        Assert.AreEqual(3, count,
            $"Input: \"{input}\"\nExpected 3× </b> total (1 manual + 2 auto), found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Stack-based tags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_Color_CleanTextContainsCloseTag()
    {
        const string input = "<color=red>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</color>",
            $"Input: \"{input}\"\nUnclosed <color> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_NestedColors_TwoCloseTagsEmitted()
    {
        const string input = "<color=red><color=blue>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        int count = CountOccurrences(clean, "</color>");
        Assert.AreEqual(2, count,
            $"Input: \"{input}\"\nExpected 2× </color>, found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Size_CleanTextContainsCloseTag()
    {
        const string input = "<size=32>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</size>",
            $"Input: \"{input}\"\nUnclosed <size> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_NestedSizes_TwoCloseTagsEmitted()
    {
        const string input = "<size=24><size=48>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        int count = CountOccurrences(clean, "</size>");
        Assert.AreEqual(2, count,
            $"Input: \"{input}\"\nExpected 2× </size>, found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Align_CleanTextContainsCloseTag()
    {
        const string input = "<align=left>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</align>",
            $"Input: \"{input}\"\nUnclosed <align> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Indent_CleanTextContainsCloseTag()
    {
        const string input = "<indent=20>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</indent>",
            $"Input: \"{input}\"\nUnclosed <indent> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Mark_CleanTextContainsCloseTag()
    {
        const string input = "<mark=red>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</mark>",
            $"Input: \"{input}\"\nUnclosed <mark> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Single-value tags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_CSpace_CleanTextContainsCloseTag()
    {
        const string input = "<cspace=3>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</cspace>",
            $"Input: \"{input}\"\nUnclosed <cspace> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Font_CleanTextContainsCloseTag()
    {
        const string input = "<font=Arial>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</font>",
            $"Input: \"{input}\"\nUnclosed <font> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_FontWeight_CleanTextContainsCloseTag()
    {
        const string input = "<font-weight=700>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</font-weight>",
            $"Input: \"{input}\"\nUnclosed <font-weight> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_LineHeight_CleanTextContainsCloseTag()
    {
        const string input = "<line-height=30>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</line-height>",
            $"Input: \"{input}\"\nUnclosed <line-height> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_LineIndent_CleanTextContainsCloseTag()
    {
        const string input = "<line-indent=10>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</line-indent>",
            $"Input: \"{input}\"\nUnclosed <line-indent> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Margin_ClosesAsMarginLeftAndRight()
    {
        // <margin=15> sets both MarginLeft and MarginRight.
        // CloseUnclosedTag closes them individually as </margin-left> and </margin-right>.
        const string input = "<margin=15>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</margin-left>",
            $"Input: \"{input}\"\nUnclosed <margin> must close margin-left.\nCleanText=\"{clean}\"" + Dump(result));
        StringAssert.Contains(clean, "</margin-right>",
            $"Input: \"{input}\"\nUnclosed <margin> must close margin-right.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_MarginLeft_CleanTextContainsCloseTag()
    {
        const string input = "<margin-left=10>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</margin-left>",
            $"Input: \"{input}\"\nUnclosed <margin-left> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_MarginRight_CleanTextContainsCloseTag()
    {
        const string input = "<margin-right=10>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</margin-right>",
            $"Input: \"{input}\"\nUnclosed <margin-right> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_MSpace_CleanTextContainsCloseTag()
    {
        const string input = "<mspace=10>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</mspace>",
            $"Input: \"{input}\"\nUnclosed <mspace> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Rotate_CleanTextContainsCloseTag()
    {
        const string input = "<rotate=45>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</rotate>",
            $"Input: \"{input}\"\nUnclosed <rotate> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_VOffset_CleanTextContainsCloseTag()
    {
        const string input = "<voffset=5>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</voffset>",
            $"Input: \"{input}\"\nUnclosed <voffset> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Width_CleanTextContainsCloseTag()
    {
        const string input = "<width=400>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</width>",
            $"Input: \"{input}\"\nUnclosed <width> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Alpha_CleanTextContainsCloseTag()
    {
        const string input = "<alpha=#80>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</alpha>",
            $"Input: \"{input}\"\nUnclosed <alpha> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Boolean tags
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_Smallcaps_CleanTextContainsCloseTag()
    {
        const string input = "<smallcaps>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</smallcaps>",
            $"Input: \"{input}\"\nUnclosed <smallcaps> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_NoBr_CleanTextContainsCloseTag()
    {
        const string input = "<nobr>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</nobr>",
            $"Input: \"{input}\"\nUnclosed <nobr> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Noparse_CleanTextContainsCloseTag()
    {
        const string input = "<noparse>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        StringAssert.Contains(clean, "</noparse>",
            $"Input: \"{input}\"\nUnclosed <noparse> must be closed.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Noparse closed first (critical ordering)
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_NoparseWithBoldInside_BoldAlsoClosedInOutput()
    {
        // <noparse> must be closed FIRST, so subsequent </b> is emitted
        // as a real close tag and not treated as literal text.
        const string input = "<b><noparse>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        // </noparse> must appear before </b>
        int noparsePos = clean.IndexOf("</noparse>");
        int boldPos = clean.IndexOf("</b>");

        Assert.IsTrue(noparsePos >= 0,
            $"Input: \"{input}\"\n</noparse> must be present.\nCleanText=\"{clean}\"" + Dump(result));
        Assert.IsTrue(boldPos >= 0,
            $"Input: \"{input}\"\n</b> must be present.\nCleanText=\"{clean}\"" + Dump(result));
        Assert.IsTrue(noparsePos < boldPos,
            $"Input: \"{input}\"\n</noparse> (pos={noparsePos}) must come before </b> (pos={boldPos}).\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_NoparseWithColorInside_ColorAlsoClosedInOutput()
    {
        const string input = "<color=red><noparse>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        int noparsePos = clean.IndexOf("</noparse>");
        int colorPos = clean.IndexOf("</color>");

        Assert.IsTrue(noparsePos >= 0 && colorPos >= 0,
            $"Input: \"{input}\"\nBoth </noparse> and </color> must be present.\nCleanText=\"{clean}\"" + Dump(result));
        Assert.IsTrue(noparsePos < colorPos,
            $"Input: \"{input}\"\n</noparse> must come before </color>.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Already-closed tags: no extra close emitted
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_AlreadyClosedBold_NoExtraCloseTag()
    {
        const string input = "<b>text</b>";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        int count = CountOccurrences(clean, "</b>");
        Assert.AreEqual(1, count,
            $"Input: \"{input}\"\nAlready-closed <b> should have exactly 1× </b>, found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_AlreadyClosedColor_NoExtraCloseTag()
    {
        const string input = "<color=red>text</color>";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        int count = CountOccurrences(clean, "</color>");
        Assert.AreEqual(1, count,
            $"Input: \"{input}\"\nAlready-closed <color> should have exactly 1× </color>, found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_AlreadyClosedSize_NoExtraCloseTag()
    {
        const string input = "<size=32>text</size>";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        int count = CountOccurrences(clean, "</size>");
        Assert.AreEqual(1, count,
            $"Input: \"{input}\"\nAlready-closed <size> should have exactly 1× </size>, found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_FullyBalancedInput_NoAdditionalCloseTags()
    {
        const string input = "<b><i><color=red>text</color></i></b>";

        var resultClose = NewParser().ParseText(input, ClosingSetting());
        var resultNoClose = NewParser().ParseText(input, NonClosingSetting());

        string cleanClose = LastCleanText(resultClose);
        string cleanNoClose = LastCleanText(resultNoClose);

        Assert.AreEqual(cleanNoClose, cleanClose,
            $"Input: \"{input}\"\nFully balanced input should produce identical CleanText regardless of CloseUnclosedTags.\n" +
            $"  CloseUnclosed=true:  \"{cleanClose}\"\n" +
            $"  CloseUnclosed=false: \"{cleanNoClose}\"" + Dump(resultClose));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Disabled: CloseUnclosedTags=false
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_Disabled_UnclosedBoldNotAutoClosedInCleanText()
    {
        const string input = "<b>text";
        var result = NewParser().ParseText(input, NonClosingSetting());
        string clean = LastCleanText(result);

        int count = CountOccurrences(clean, "</b>");
        Assert.AreEqual(0, count,
            $"Input: \"{input}\"\nWith CloseUnclosedTags=false, no auto </b> should appear, found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_Disabled_UnclosedColorNotAutoClosedInCleanText()
    {
        const string input = "<color=red>text";
        var result = NewParser().ParseText(input, NonClosingSetting());
        string clean = LastCleanText(result);

        int count = CountOccurrences(clean, "</color>");
        Assert.AreEqual(0, count,
            $"Input: \"{input}\"\nWith CloseUnclosedTags=false, no auto </color> should appear, found {count}.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // IllegalTags interaction
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_IllegalBold_CloseTagNotEmittedInCleanText()
    {
        // If <b> is illegal, the open tag is stripped from CleanText,
        // and the auto-close should also be stripped.
        var setting = ClosingSetting(illegalTags: new[] { "b" });
        const string input = "<b>text";
        var result = NewParser().ParseText(input, setting);
        string clean = LastCleanText(result);

        Assert.IsFalse(clean.Contains("</b>"),
            $"Input: \"{input}\"\nIllegal <b> auto-close must NOT appear in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_IllegalColor_CloseTagNotEmittedButStyleReset()
    {
        var setting = ClosingSetting(illegalTags: new[] { "color" });
        const string input = "<color=red>text";
        var result = NewParser().ParseText(input, setting);
        string clean = LastCleanText(result);

        // Close tag stripped from output
        Assert.IsFalse(clean.Contains("</color>"),
            $"Input: \"{input}\"\nIllegal <color> auto-close must NOT appear in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Multi-line scenario
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_TagOpenedBeforeNewline_ClosedOnLastLine()
    {
        const string input = "<b>line1\nline2";
        var result = NewParser().ParseText(input, ClosingSetting());
        string lastClean = LastCleanText(result);

        StringAssert.Contains(lastClean, "</b>",
            $"Input: \"{input}\"\nAuto-close </b> must appear on last line's CleanText.\nLastCleanText=\"{lastClean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Combined stress test
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_ManyMixedUnclosedTags_AllClosedInCleanText()
    {
        const string input = "<b><i><u><color=red><size=32><align=left><indent=10><cspace=3><font=Arial><voffset=5>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        string[] expectedCloseTags = new[]
        {
            "</b>", "</i>", "</u>", "</color>", "</size>",
            "</align>", "</indent>", "</cspace>", "</font>", "</voffset>"
        };

        foreach (string tag in expectedCloseTags)
        {
            StringAssert.Contains(clean, tag,
                $"Input: \"{input}\"\nMissing auto-close \"{tag}\" in CleanText.\nCleanText=\"{clean}\"" + Dump(result));
        }
    }

    [TestMethod]
    public void CloseUnclosed_CounterAndStackMixed_CorrectCounts()
    {
        // 2× <b>, 1× <color>, 2× <size>
        const string input = "<b><b><color=red><size=24><size=48>text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        Assert.AreEqual(2, CountOccurrences(clean, "</b>"),
            $"Input: \"{input}\"\nExpected 2× </b>.\nCleanText=\"{clean}\"" + Dump(result));
        Assert.AreEqual(1, CountOccurrences(clean, "</color>"),
            $"Input: \"{input}\"\nExpected 1× </color>.\nCleanText=\"{clean}\"" + Dump(result));
        Assert.AreEqual(2, CountOccurrences(clean, "</size>"),
            $"Input: \"{input}\"\nExpected 2× </size>.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Single-value tags already closed: no duplicate
    // ═════════════════════════════════════════════════════════════════════════

    [TestMethod]
    public void CloseUnclosed_FontAlreadyClosed_NoExtraCloseTag()
    {
        const string input = "<font=Arial>text</font>";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        Assert.AreEqual(1, CountOccurrences(clean, "</font>"),
            $"Input: \"{input}\"\nAlready-closed <font> should have exactly 1× </font>.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_VOffsetAlreadyClosed_NoExtraCloseTag()
    {
        const string input = "<voffset=5>text</voffset>";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        Assert.AreEqual(1, CountOccurrences(clean, "</voffset>"),
            $"Input: \"{input}\"\nAlready-closed <voffset> should have exactly 1× </voffset>.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // Plain text / no tags: nothing extra emitted
    [TestMethod]
    public void CloseUnclosed_PlainText_CleanTextHasNoCloseTags()
    {
        const string input = "just plain text";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        Assert.IsFalse(clean.Contains("</"),
            $"Input: \"{input}\"\nPlain text should have no close tags.\nCleanText=\"{clean}\"" + Dump(result));
    }

    [TestMethod]
    public void CloseUnclosed_EmptyInput_CleanTextIsEmpty()
    {
        const string input = "";
        var result = NewParser().ParseText(input, ClosingSetting());
        string clean = LastCleanText(result);

        Assert.AreEqual("", clean,
            $"Input: \"\"\nEmpty input CleanText must be empty.\nCleanText=\"{clean}\"" + Dump(result));
    }

    // ═════════════════════════════════════════════════════════════════════════
    // Utility
    // ═════════════════════════════════════════════════════════════════════════

    private static int CountOccurrences(string text, string pattern)
    {
        int count = 0;
        int idx = 0;
        while ((idx = text.IndexOf(pattern, idx, StringComparison.Ordinal)) >= 0)
        {
            count++;
            idx += pattern.Length;
        }
        return count;
    }
}