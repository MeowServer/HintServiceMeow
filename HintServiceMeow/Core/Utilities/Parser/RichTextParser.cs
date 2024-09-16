﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Utilities.Tools;

namespace HintServiceMeow.Core.Utilities.Parser
{
    internal static class Regexs
    {
        public static readonly string SizeTagRegex = @"<size=(\d+)(px|%)?>";
        public static readonly string LineHeightTagRegex = @"<line-height=([\d\.]+)(px|%|em)?>";
        public static readonly string PosTagRegex = @"<pos=([+-]?\d+(px)?)>";
        public static readonly string VOffsetTagRegex = @"<voffset=([+-]?\d+(px)?)>";
        public static readonly string AlignTagRegex = @"<align=(left|center|right)>|</align>";
    }

    internal static class Tags
    {
        private static readonly object Lock = new object();

        private static readonly HashSet<string> AllTags = new HashSet<string>
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
            if(string.IsNullOrEmpty(tag.Trim()))
                return false;

            tag = tag.ToLower();

            lock (Lock)
            {
                if (tag.StartsWith("</"))
                {
                    string tagName = tag
                        .Substring(2, tag.Length - 3)
                        .Split('=')
                        .First();
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
        private static readonly ConcurrentDictionary<string, IReadOnlyList<LineInfo>> Cache = new ConcurrentDictionary<string, IReadOnlyList<LineInfo>>();

        //Lock
        private readonly object _lock = new object();

        //Handling
        private int _index = 0;
        private readonly StringBuilder _currentRawLineText = new StringBuilder(100);

        //Current line status. Only apply to a single line
        private float _pos = 0;
        private float _lineHeight = float.MinValue;
        private bool _hasLineHeight = false;

        //Line status
        private HintAlignment _currentLineAlignment = HintAlignment.Center; //Used since line alignment apply to entire line but can affect multiple line
        private readonly Stack<HintAlignment> _hintAlignmentStack = new Stack<HintAlignment>();

        //Character status
        private float _vOffset = 0;
        private TextStyle _style = TextStyle.Normal;
        private readonly Stack<float> _fontSizeStack = new Stack<float>();
        private readonly List<CaseStyle> _caseStyleStack = new List<CaseStyle>();
        private readonly List<ScriptStyle> _scriptStyles = new List<ScriptStyle>();

        public IReadOnlyList<LineInfo> ParseText(string text, int size = 20)
        {
            //Check cache
            if(Cache.TryGetValue(text, out var cachedResult))
            {
                return cachedResult;
            }

            ClearStatus();

            _fontSizeStack.Push(size);
            _caseStyleStack.Add(CaseStyle.Smallcaps);

            List<LineInfo> lines = new List<LineInfo>();
            List<CharacterInfo> currentChInfos = new List<CharacterInfo>();

            int lastIndex = 0;

            lock (_lock)
            {
                while (_index < text.Length)
                {
                    if(lastIndex <= _index)
                    {
                        _currentRawLineText.Append(text.Substring(lastIndex, _index - lastIndex + 1));
                        lastIndex = _index + 1;
                    }

                    // Check if the character is the start of a tag
                    if (CheckTag(text))
                    {
                        continue;
                    }

                    if (!currentChInfos.IsEmpty())
                    {
                        float currentWidth = currentChInfos.Sum(x => x.Width);
                        float leftX = - currentWidth / 2 + _pos;
                        float rightX = currentWidth / 2 + _pos;

                        //Try change line 
                        if (text[_index] == '\n' || leftX < -1200 || rightX > 1200)
                        {
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
                    }
                    

                    

                    currentChInfos.Add(GetChInfo(text[_index]));
                    _index++;
                }

                if (lastIndex <= _index)
                {
                    var cutLength = text.Length - lastIndex;
                    if(cutLength > 0)
                        _currentRawLineText.Append(text.Substring(lastIndex, cutLength));
                }

                lines.Add(GetLineInfo(currentChInfos, _currentLineAlignment));
                currentChInfos.Clear();
            }

            Cache[text] = new List<LineInfo>(lines).AsReadOnly();
            Task.Run(() =>Task.Delay(10000).ContinueWith(_ => Cache.TryRemove(text, out var _)));//Remove cache after 10 seconds

            return new List<LineInfo>(lines).AsReadOnly();
        }

        private LineInfo GetLineInfo(List<CharacterInfo> chs, HintAlignment align)
        {
            var rawText = _currentRawLineText.ToString();
            _currentRawLineText.Clear();

            var line = new LineInfo(
                chs,
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
            float currentFontSize = _fontSizeStack.Count > 0 ? (int)_fontSizeStack.Peek() : 40;

            //Case style
            switch(_caseStyleStack.LastOrDefault())
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
            var chWidth = FontTool.GetCharSize(ch, currentFontSize, _style);

            //Script style
            if (_scriptStyles.Contains(ScriptStyle.Superscript))
            {
                chWidth *= (float)Math.Pow(0.5, _scriptStyles.Count(x => x == ScriptStyle.Superscript));
            }
            else if (_scriptStyles.Contains(ScriptStyle.Subscript))
            {
                chWidth *= (float)Math.Pow(0.5, _scriptStyles.Count(x => x == ScriptStyle.Subscript));
            }

            var chInfo = new CharacterInfo(
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
                var tagEndIndex = text.IndexOf('>', _index);

                if (tagEndIndex != -1)
                {
                    tag = text.Substring(_index, tagEndIndex - _index + 1);
                    isTag = Tags.IsTag(tag);

                    if(isTag)
                        _index = tagEndIndex + 1;//Move cursor to the end of the tag
                }
            }

            //Try handle tag
            if(tag != string.Empty && isTag)
                TryHandleTag(tag);

            //Return whether the text is the start of the tag
            return isTag;
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
                        if(index != -1)
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
            if (tag.StartsWith("<size") && TryParserSize(tag, out var size))
            {
                _fontSizeStack.Push(size);

                return true;
            }

            if (tag.StartsWith("<line-height") && TryParserLineHeight(tag, out var lineHeight))
            {
                _hasLineHeight = true;
                _lineHeight = lineHeight;

                return true;
            }

            if (tag.StartsWith("<pos") && TryParsePos(tag, out var pos))
            {
                this._pos = pos;

                return true;
            }

            if (tag.StartsWith("<voffset") && TryParseVOffset(tag, out var vOffset))
            {
                this._vOffset = vOffset;

                return true;
            }

            if (tag.StartsWith("<align") && TryParseAlign(tag, out var align))
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

        private bool TryParserLineHeight(string tag, out float lineHeight)
        {
            var lineHeightMatch = Regex.Match(tag, Regexs.LineHeightTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (lineHeightMatch.Success)
            {
                var value = float.Parse(lineHeightMatch.Groups[1].Value);
                var unit = lineHeightMatch.Groups[2].Value;

                switch (unit.ToLower())
                {
                    case "px":
                        lineHeight = value;
                        break;
                    case "%":
                        lineHeight = 40f * value / 100f;
                        break;
                    case "em":
                        lineHeight = 40f * value;
                        break;
                    default:
                        lineHeight = value;
                        break;
                }

                return true;
            }

            lineHeight = -1;
            return false;
        }

        private bool TryParserSize(string tag, out float size)
        {
            var sizeMatch = Regex.Match(tag, Regexs.SizeTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (sizeMatch.Success)
            {
                var value = int.Parse(sizeMatch.Groups[1].Value);
                var unit = sizeMatch.Groups[2].Value;

                switch (unit.ToLower())
                {
                    case "px":
                        size = value;
                        break;
                    case "%":
                        size = 40f * value / 100f;
                        break;
                    default:
                        size = value;
                        break;
                }

                return true;
            }

            size = -1;
            return false;
        }

        private bool TryParsePos(string tag, out float pos)
        {
            var posMatch = Regex.Match(tag, Regexs.PosTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (posMatch.Success)
            {
                var value = int.Parse(posMatch.Groups[1].Value);
                var unit = posMatch.Groups[2].Value;

                switch (unit.ToLower())
                {
                    default:
                        pos = value;
                        break;
                }

                return true;
            }

            pos = -1;
            return false;
        }

        private bool TryParseVOffset(string tag, out float vOffset)
        {
            var vOffsetMatch = Regex.Match(tag, Regexs.VOffsetTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (vOffsetMatch.Success)
            {
                var value = int.Parse(vOffsetMatch.Groups[1].Value);
                var unit = vOffsetMatch.Groups[2].Value;

                switch (unit.ToLower())
                {
                    default:
                        vOffset = value;
                        break;
                }

                return true;
            }

            vOffset = -1;
            return false;
        }

        private bool TryParseAlign(string tag, out HintAlignment align)
        {
            var match = Regex.Match(tag, Regexs.AlignTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (match.Success && System.Enum.TryParse(match.Groups[1].Value, true, out HintAlignment alignment))
            {
                align = alignment;

                return true;
            }

            align = HintAlignment.Center;
            return false;
        }
    }

    internal class CharacterInfo
    {
        public char Character { get; }
        public float FontSize { get; }
        public float Width { get; }
        public float Height { get; }
        public float VOffset { get; }

        public CharacterInfo(char character, float fontSize, float width, float height, float vOffset)
        {
            Character = character;
            FontSize = fontSize;
            Width = width;
            Height = height;
            VOffset = vOffset;
        }
    }

    internal class LineInfo
    {
        public IReadOnlyList<CharacterInfo> Characters { get; }
        public HintAlignment Alignment { get; }
        public float LineHeight { get; }
        public bool HasLineHeight { get; }
        public float Pos { get; }

        public string RawText { get; }

        public float Width {
            get
            {
                if (Characters is null || Characters.IsEmpty())
                    return 0;

                return Characters.Sum(c => c.Width);
            }
        }
        public float Height
        {
            get
            {
                if (Characters is null || Characters.IsEmpty())
                    return 0;

                if (HasLineHeight)
                {
                    return LineHeight;
                }

                return Characters.Max(c => c.Height);
            }
        }

        public LineInfo(List<CharacterInfo> characters, HintAlignment alignment, float lineHeight, bool hasLineHeight, float pos, string rawText)
        {
            if (characters is null)
                throw new ArgumentNullException(nameof(characters), "Characters cannot be null.");

            Characters = new List<CharacterInfo>(characters).AsReadOnly();

            Alignment = alignment;
            LineHeight = lineHeight;
            HasLineHeight = hasLineHeight;
            Pos = pos;

            RawText = rawText;
        }
    }
}