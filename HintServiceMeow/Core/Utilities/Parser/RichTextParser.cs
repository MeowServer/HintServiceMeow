using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Exiled.API.Features;
using HintServiceMeow.Core.Enum;

using static HintServiceMeow.Core.Utilities.Tools.FontTool;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Use to calculate the rich text's width and height.
    /// Get string size, get 
    /// </summary>
    internal class RichTextParser
    {
        private static readonly ConcurrentDictionary<string, List<LineInfo>> _cache = new ConcurrentDictionary<string, List<LineInfo>>();

        private readonly object _lock = new object();
        private readonly Stack<float> _fontSizeStack = new Stack<float>();
        private readonly Stack<HintAlignment> _hintAlignmentStack = new Stack<HintAlignment>();

        private float _pos = 0;

        private float _lineHeight = float.MinValue;
        private bool _hasLineHeight = false;

        private float _vOffset = 0;

        private FontStyle _style = FontStyle.Regular;

        private static readonly string SizeTagRegex = @"<size=(\d+)(px|%)?>";
        private static readonly string LineHeightTagRegex = @"<line-height=([\d\.]+)(px|%|em)?>";
        private static readonly string PosTagRegex = @"<pos=([+-]?\d+(px)?)>";
        private static readonly string VOffsetTagRegex = @"<voffset=([+-]?\d+(px)?)>";
        private static readonly string AlignTagRegex = @"<align=(left|center|right)>|</align>";

        public List<LineInfo> ParseText(string text, int size = 20)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            if(_cache.TryGetValue(text, out var cachedResult))
            {
                return new List<LineInfo>(cachedResult);
            }

            _fontSizeStack.Clear();
            _pos = 0;
            _lineHeight = float.MinValue;
            _hasLineHeight = false;
            _vOffset = 0;
            _style = FontStyle.Regular;

            _fontSizeStack.Push(size);

            List<LineInfo> lines = new List<LineInfo>();

            lock (_lock)
            {
                var currentLine = new LineInfo();

                int index = 0;
                while (index < text.Length)
                {
                    // Check if the character is a tag
                    if (text[index] == '<')
                    {
                        var tagEndIndex = text.IndexOf('>', index);

                        if (tagEndIndex != -1)
                        {
                            var tag = text.Substring(index, tagEndIndex - index + 1);

                            if (TryHandleTag(tag))
                            {
                                index = tagEndIndex + 1;
                                continue;
                            }
                        }
                    }

                    if (text[index] == '\n' || currentLine.Width >= 2400) // if change line to auto change line
                    {
                        currentLine.Pos = _pos;
                        currentLine.LineHeight = _lineHeight;
                        currentLine.HasLineHeight = _hasLineHeight;
                        currentLine.Alignment = _hintAlignmentStack.Count > 0 ? _hintAlignmentStack.Peek() : HintAlignment.Center;

                        _pos = 0;
                        _lineHeight = float.MinValue;
                        _hasLineHeight = false;

                        lines.Add(currentLine);
                        currentLine = new LineInfo();
                        index++;
                        continue;
                    }

                    //Parse character into a CharacterInfo
                    char ch = text[index];
                    float currentFontSize = _fontSizeStack.Count > 0 ? (int)_fontSizeStack.Peek() : size;

                    if (char.IsLower(ch))
                    {
                        currentFontSize = currentFontSize * 16f / 20f;
                        ch = char.ToUpper(ch);
                    }

                    var currentSize = GetCharSize(ch, currentFontSize, _style);

                    var chInfo = new CharacterInfo()
                    {
                        Character = ch,
                        FontSize = currentFontSize,
                        Width = currentSize.Width,
                        Height = currentFontSize,
                        VOffset = _vOffset
                    };

                    currentLine.Characters.Add(chInfo);

                    index++;
                }

                currentLine.Pos = _pos;
                currentLine.LineHeight = _lineHeight;
                currentLine.HasLineHeight = _hasLineHeight;

                lines.Add(currentLine);
            }

            _cache[text] = new List<LineInfo>(lines);
            Task.Run(() =>Task.Delay(10000).ContinueWith(_ => _cache.TryRemove(text, out var _)));

            return lines;
        }

        private bool TryHandleTag(string tag)
        {
            tag = tag.ToLower();

            if (tag.StartsWith("</"))
            {
                switch (tag)
                {
                    case "</b>":
                        _style &= ~FontStyle.Bold;
                        break;
                    case "</i>":
                        _style &= ~FontStyle.Italic;
                        break;
                    case "</size>":
                        if(_fontSizeStack.Count > 0)
                            _fontSizeStack.Pop();
                        break;
                    case "</voffset>":
                        _vOffset = 0;
                        break;
                    case "</align>":
                        if (_hintAlignmentStack.Count > 0)
                            _hintAlignmentStack.Pop();
                        break;
                }

                return true;
            }

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

            if(tag.StartsWith("<voffset") && TryParseVOffset(tag, out var vOffset))
            {
                this._vOffset = vOffset;

                return true;
            }

            if(tag.StartsWith("<align") && TryParseAlign(tag, out var align))
            {
                _hintAlignmentStack.Push(align);

                return true;
            }

            if (tag == "<b>")
            {
                _style |= FontStyle.Bold;

                return true;
            }

            if (tag == "<i>")
            {
                _style |= FontStyle.Italic;

                return true;
            }

            return false;
        }

        private bool TryParserLineHeight(string tag, out float lineHeight)
        {
            var lineHeightMatch = Regex.Match(tag, LineHeightTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            var sizeMatch = Regex.Match(tag, SizeTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            var posMatch = Regex.Match(tag, PosTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            var vOffsetMatch = Regex.Match(tag, VOffsetTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
            var match = Regex.Match(tag, AlignTagRegex, RegexOptions.IgnoreCase | RegexOptions.Compiled);

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
        public char Character { get; set; }

        public float FontSize { get; set; }

        public float Width { get; set; }
        public float Height { get; set; }

        public float VOffset { get; set; }

        public CharacterInfo()
        {
        }
    }

    internal class LineInfo
    {
        public List<CharacterInfo> Characters { get; set; } = new List<CharacterInfo>();

        public HintAlignment Alignment { get; set; } = HintAlignment.Center;

        public float LineHeight { get; set; } = float.MinValue;
        public bool HasLineHeight { get; set; } = false;

        public float Pos { get; set; } = 0;

        public float Width => Characters.Count != 0 ? Characters.Sum(x => x.Width) : 0;

        public float Height => HasLineHeight ? LineHeight : Characters.Count != 0 ? Characters.Max(x => x.Height) : 0;
    }
}
