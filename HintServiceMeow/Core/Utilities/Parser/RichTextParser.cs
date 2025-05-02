using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Utilities.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HintServiceMeow.Core.Utilities.Parser
{
    internal static class RegexPatterns
    {
        public static readonly string SizeTagRegexPattern = @"<size=(\d+)(px|%)?>";
        public static readonly string LineHeightTagRegexPattern = @"<line-height=([\d\.]+)(px|%|em)?>";
        public static readonly string PosTagRegexPattern = @"<pos=([+-]?\d+(px)?)>";
        public static readonly string VOffsetTagRegexPattern = @"<voffset=([+-]?\d+(px)?)>";
        public static readonly string AlignTagRegexPattern = @"<align=(left|center|right)>|</align>";
    }

    internal static class Tags
    {
        private static readonly object Lock = new();

        private static readonly HashSet<string> AllTags = new()
        {
            "align", "allcaps", "alpha", "b", "color", "cspace", "font", "font-weight",
            "gradient", "i", "indent", "line-height", "line-indent", "link", "lowercase",
            "margin", "mark", "mspace", "nobr", "noparse", "page", "pos", "rotate", "s",
            "size", "smallcaps", "space", "sprite", "strikethrough", "style", "sub", "sup",
            "u", "uppercase", "voffset", "width"
        };

        public static bool IsTag(string tag)
        {
            return IsEndTag(tag) || IsStartTag(tag);
        }

        public static bool IsEndTag(string tag)
        {
            if (string.IsNullOrEmpty(tag.Trim()))
                return false;

            tag = tag.ToLower();

            lock (Lock)
            {
                if (tag.StartsWith("</"))
                {
                    string tagName = tag
                        .Substring(2, tag.Length - 3);

                    return AllTags.Contains(tagName);
                }

                return false;
            }
        }

        public static bool IsStartTag(string tag)
        {
            if (string.IsNullOrEmpty(tag.Trim()))
                return false;

            tag = tag.ToLower();

            lock (Lock)
            {
                if (tag.StartsWith("<") && !tag.StartsWith("</"))
                {
                    string tagName = tag
                        .Substring(1, tag.Length - 2)
                        .Split('=')
                        .First();

                    return AllTags.Contains(tagName);
                }

                return false;
            }
        }
    }

    /// <summary>
    /// Use to calculate the rich text's width
    /// </summary>
    internal class RichTextParser
    {
        private const float DefaultFontSize = 40f;

        private static readonly Cache<ValueTuple<string, float, HintAlignment>, IReadOnlyList<LineInfo>> Cache = new(1000);

        //Lock
        private readonly object _lock = new();

        //Handling
        private int _index = 0;
        private readonly StringBuilder _currentRawLineText = new(100);

        //Current line status. Only apply to a single line
        private float _pos = 0;
        private float _lineHeight = float.MinValue;
        private bool _hasLineHeight = false;

        //Line status
        private HintAlignment _currentLineAlignment = HintAlignment.Center; //Used since line alignment apply to entire line but can affect multiple line
        private readonly Stack<HintAlignment> _hintAlignmentStack = new();

        //Character status
        private float _vOffset = 0;
        private TextStyle _style = TextStyle.Normal;
        private readonly Stack<float> _fontSizeStack = new();
        private readonly List<CaseStyle> _caseStyleStack = new();
        private readonly List<ScriptStyle> _scriptStyles = new();

        public IReadOnlyList<LineInfo> ParseText(string text, int size = 20, HintAlignment alignment = HintAlignment.Center)
        {
            if (text is null)
                return new List<LineInfo>();

            ValueTuple<string, float, HintAlignment> cacheKey = ValueTuple.Create(text, size, alignment);

            //Check cache
            if (Cache.TryGet(cacheKey, out IReadOnlyList<LineInfo> cachedResult))
            {
                return new List<LineInfo>(cachedResult);
            }

            //Replace linebreak
            text = text
                .Replace("<br>", "\n")
                .Replace("\\n", "\n");

            List<LineInfo> lines = new();
            List<CharacterInfo> currentChInfos = new();

            lock (_lock)
            {
                ClearStatus();

                if (alignment != HintAlignment.Center)
                    _hintAlignmentStack.Push(alignment);

                _fontSizeStack.Push(size);
                _caseStyleStack.Add(CaseStyle.Smallcaps);

                int lastIndex = 0;

                while (_index < text.Length)
                {
                    if (lastIndex <= _index)
                    {
                        _currentRawLineText.Append(text.Substring(lastIndex, _index - lastIndex + 1));
                        lastIndex = _index + 1;
                    }

                    // Check if the character is the start of a tag
                    if (CheckTag(text))
                    {
                        continue;
                    }

                    float overflowValue = 0; //Temporarily remove overflow detection since it is not working properly
                    //float currentWidth = !currentChInfos.Any() ? 0 : currentChInfos.Sum(x => x.Width);
                    //float overflowValue = _currentLineAlignment switch
                    //{
                    //    HintAlignment.Center => currentWidth / 2 + _pos,
                    //    HintAlignment.Left => -1200 + currentWidth + _pos,
                    //    HintAlignment.Right => 1200 + _pos,
                    //    _ => 0,
                    //};

                    //Try change line
                    if (text[_index] == '\n' || overflowValue > 1200)
                    {
                        if (text[_index] == '\n')
                            currentChInfos.Add(GetChInfo('\n'));//Add \n if the line break at \n

                        //Create new line info
                        lines.Add(GetLineInfo(currentChInfos, _currentLineAlignment));

                        //Clear character list
                        currentChInfos.Clear();

                        //Set default alignment for next line
                        _currentLineAlignment = _hintAlignmentStack.Any() ? _hintAlignmentStack.Peek() : HintAlignment.Center;

                        //Clear line status
                        ClearLineStatus();

                        //Goto next character
                        _index++;

                        continue;
                    }

                    currentChInfos.Add(GetChInfo(text[_index]));
                    _index++;
                }

                if (lastIndex <= _index)
                {
                    int cutLength = text.Length - lastIndex;
                    if (cutLength > 0)
                        _currentRawLineText.Append(text.Substring(lastIndex, cutLength));
                }

                if (currentChInfos.Count != 0)
                    lines.Add(GetLineInfo(currentChInfos, _currentLineAlignment));

                currentChInfos.Clear();
            }

            Cache.Add(cacheKey, new List<LineInfo>(lines).AsReadOnly());

            return new List<LineInfo>(lines).AsReadOnly();
        }

        private LineInfo GetLineInfo(List<CharacterInfo> chs, HintAlignment align)
        {
            string rawText = _currentRawLineText.ToString();
            _currentRawLineText.Clear();

            LineInfo line = new(
                chs.AsReadOnly(),
                align,
                _lineHeight,
                _hasLineHeight,
                _pos,
                rawText
                );

            return line;
        }

        private CharacterInfo GetChInfo(char ch)
        {
            //Get size
            float currentFontSize = _fontSizeStack.Count > 0 ? (int)_fontSizeStack.Peek() : DefaultFontSize;

            //Case style
            switch (_caseStyleStack.LastOrDefault())
            {
                case CaseStyle.Lowercase:
                    ch = char.ToLower(ch);
                    break;
                case CaseStyle.Uppercase:
                    ch = char.ToUpper(ch);
                    break;
                case CaseStyle.Allcaps:
                    ch = char.ToUpper(ch);
                    break;
                case CaseStyle.Smallcaps:
                    if (char.IsLetter(ch))
                    {
                        currentFontSize *= 0.8f;
                        ch = char.ToUpper(ch);
                    }
                    break;
                default: //Default to smallcap
                    if (char.IsLetter(ch))
                    {
                        currentFontSize *= 0.8f;
                        ch = char.ToUpper(ch);
                    }
                    break;
            }

            //Get height with v-offset
            float chHeight = currentFontSize + _vOffset;

            //Get character size
            float chWidth = FontTool.GetCharWidth(ch, currentFontSize, _style);

            //Script style
            if (_scriptStyles.Contains(ScriptStyle.Superscript))
            {
                chWidth *= (float)Math.Pow(0.5, _scriptStyles.Count(x => x == ScriptStyle.Superscript));
            }

            if (_scriptStyles.Contains(ScriptStyle.Subscript))
            {
                chWidth *= (float)Math.Pow(0.5, _scriptStyles.Count(x => x == ScriptStyle.Subscript));
            }

            CharacterInfo chInfo = new(
                ch,
                currentFontSize,
                chWidth,
                chHeight,
                _vOffset);

            return chInfo;
        }

        private void ClearStatus()
        {
            _index = 0;
            _currentRawLineText.Clear();

            _currentLineAlignment = HintAlignment.Center;
            _hintAlignmentStack.Clear();

            _pos = 0;
            _lineHeight = float.MinValue;
            _hasLineHeight = false;

            _vOffset = 0;
            _style = TextStyle.Normal;
            _fontSizeStack.Clear();
            _caseStyleStack.Clear();
            _scriptStyles.Clear();
        }

        private void ClearLineStatus()//Clear status that applies to current line
        {
            _pos = 0;
            _lineHeight = float.MinValue;
            _hasLineHeight = false;
        }

        private bool CheckTag(string text)
        {
            string tag = string.Empty;
            bool isTag = false;

            //Try cut tag
            if (text[_index] == '<')
            {
                int tagEndIndex = text.IndexOf('>', _index);

                if (tagEndIndex != -1)
                {
                    tag = text.Substring(_index, tagEndIndex - _index + 1);
                    isTag = Tags.IsTag(tag);

                    if (isTag)
                        _index = tagEndIndex + 1;//Move cursor to the end of the tag
                }
            }

            //Try handle tag
            if (tag != string.Empty && isTag)
                TryHandleTag(tag);

            //Return whether the text is the start of the tag
            return tag != string.Empty && isTag;
        }

        private bool TryHandleTag(string tag)
        {
            if (string.IsNullOrEmpty(tag))
                return false;

            tag = tag.ToLower();

            //Handle end tag
            if (tag.StartsWith("</"))
            {
                int index;

                switch (tag)
                {
                    case "</b>":
                        _style &= ~TextStyle.Bold;
                        return true;
                    case "</i>":
                        _style &= ~TextStyle.Italic;
                        return true;
                    case "</size>":
                        if (_fontSizeStack.Count > 0)
                            _fontSizeStack.Pop();
                        return true;
                    case "</voffset>":
                        _vOffset = 0;
                        return true;
                    case "</align>":
                        if (_hintAlignmentStack.Count > 0)
                            _hintAlignmentStack.Pop();
                        return true;
                    case "</lowercase>":
                        index = _caseStyleStack.LastIndexOf(CaseStyle.Lowercase);
                        if (index != -1)
                            _caseStyleStack.RemoveAt(index);
                        return true;
                    case "</uppercase>":
                        index = _caseStyleStack.LastIndexOf(CaseStyle.Uppercase);
                        if (index != -1)
                            _caseStyleStack.RemoveAt(index);
                        return true;
                    case "</allcaps>":
                        index = _caseStyleStack.LastIndexOf(CaseStyle.Allcaps);
                        if (index != -1)
                            _caseStyleStack.RemoveAt(index);
                        return true;
                    case "</smallcaps>":
                        index = _caseStyleStack.LastIndexOf(CaseStyle.Smallcaps);
                        if (index != -1)
                            _caseStyleStack.RemoveAt(index);
                        return true;
                    case "</sup>":
                        index = _scriptStyles.LastIndexOf(ScriptStyle.Superscript);
                        if (index != -1)
                            _scriptStyles.RemoveAt(index);
                        return true;
                    case "</sub>":
                        index = _scriptStyles.LastIndexOf(ScriptStyle.Subscript);
                        if (index != -1)
                            _scriptStyles.RemoveAt(index);
                        return true;
                }

                return false;
            }

            //Handle start tag
            if (tag.StartsWith("<size") && TryParseSize(tag, out float size))
            {
                _fontSizeStack.Push(size);

                return true;
            }

            if (tag.StartsWith("<line-height") && TryParseLineHeight(tag, out float lineHeight))
            {
                _hasLineHeight = true;
                _lineHeight = lineHeight;

                return true;
            }

            if (tag.StartsWith("<pos") && TryParsePos(tag, out float pos))
            {
                this._pos = pos;

                return true;
            }

            if (tag.StartsWith("<voffset") && TryParseVOffset(tag, out float vOffset))
            {
                this._vOffset = vOffset;

                return true;
            }

            if (tag.StartsWith("<align") && TryParseAlign(tag, out HintAlignment align))
            {
                _hintAlignmentStack.Push(align);
                _currentLineAlignment = align;

                return true;
            }

            if (tag == "<b>")
            {
                _style |= TextStyle.Bold;

                return true;
            }

            if (tag == "<i>")
            {
                _style |= TextStyle.Italic;

                return true;
            }

            if (tag == "<lowercase>")
            {
                _caseStyleStack.Add(CaseStyle.Lowercase);

                return true;
            }

            if (tag == "<uppercase>")
            {
                _caseStyleStack.Add(CaseStyle.Uppercase);

                return true;
            }

            if (tag == "<allcaps>")
            {
                _caseStyleStack.Add(CaseStyle.Allcaps);

                return true;
            }

            if (tag == "<smallcaps>")
            {
                _caseStyleStack.Add(CaseStyle.Smallcaps);

                return true;
            }

            if (tag == "<sup>")
            {
                _scriptStyles.Add(ScriptStyle.Superscript);

                return true;
            }

            if (tag == "<sub>")
            {
                _scriptStyles.Add(ScriptStyle.Subscript);

                return true;
            }

            return false;
        }

        private bool TryParseLineHeight(string tag, out float lineHeight)
        {
            Match lineHeightMatch = Regex.Match(tag, RegexPatterns.LineHeightTagRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (lineHeightMatch.Success)
            {
                float value = float.Parse(lineHeightMatch.Groups[1].Value);
                string unit = lineHeightMatch.Groups[2].Value;

                lineHeight = unit.ToLower() switch
                {
                    "px" => value,
                    "%" => DefaultFontSize * value / 100f,
                    "em" => DefaultFontSize * value,
                    _ => value
                };

                return true;
            }

            lineHeight = -1;
            return false;
        }

        private bool TryParseSize(string tag, out float size)
        {
            Match sizeMatch = Regex.Match(tag, RegexPatterns.SizeTagRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (sizeMatch.Success)
            {
                int value = int.Parse(sizeMatch.Groups[1].Value);
                string unit = sizeMatch.Groups[2].Value;

                size = unit.ToLower() switch
                {
                    "px" => value,
                    "%" => DefaultFontSize * value / 100f,
                    _ => value
                };

                return true;
            }

            size = -1;
            return false;
        }

        private bool TryParsePos(string tag, out float pos)
        {
            Match posMatch = Regex.Match(tag, RegexPatterns.PosTagRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (posMatch.Success)
            {
                int value = int.Parse(posMatch.Groups[1].Value);
                //string unit = posMatch.Groups[2].Value;

                //TODO: add unit support
                pos = value;

                return true;
            }

            pos = -1;
            return false;
        }

        private bool TryParseVOffset(string tag, out float vOffset)
        {
            Match vOffsetMatch = Regex.Match(tag, RegexPatterns.VOffsetTagRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (vOffsetMatch.Success)
            {
                int value = int.Parse(vOffsetMatch.Groups[1].Value);
                //string unit = vOffsetMatch.Groups[2].Value;

                //TODO: add unit support
                vOffset = value;

                return true;
            }

            vOffset = -1;
            return false;
        }

        private bool TryParseAlign(string tag, out HintAlignment align)
        {
            Match match = Regex.Match(tag, RegexPatterns.AlignTagRegexPattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (match.Success && System.Enum.TryParse(match.Groups[1].Value, true, out HintAlignment alignment))
            {
                align = alignment;

                return true;
            }

            align = HintAlignment.Center;
            return false;
        }
    }
}