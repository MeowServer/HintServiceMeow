namespace HintServiceMeow.Core.Utilities.Parser
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Arguments;
    using HintServiceMeow.Core.Models.Parser;
    using HintServiceMeow.Core.Models.Parser.Style;
    using HintServiceMeow.Core.Models.Parser.ValueObject;
    using HintServiceMeow.Core.Utilities.Pools;
    using HintServiceMeow.Core.Utilities.Tools;

    /// <summary>
    /// Use to calculate the rich text's width.
    /// </summary>
    internal class RichTextParser
    {
        private static Cache<ValueTuple<string, RichTextParserSetting>, RichTextParserResult> parserCache = new Cache<(string, RichTextParserSetting), RichTextParserResult>(200);

        private object parserLock = new object();

        private TextSegmentStyle? charStyleCache;
        private LineStyle? lineStyleAutoWrappedCache;
        private LineStyle? lineStyleNonAutoWrappedCache;

        private List<IParameter> parameters = new List<IParameter>();

        private List<LineInfo> lineInfos = new(16);
        private List<TextSegment> currentLineChars = new(256);

        private Style currentStyle = new Style();

        /* Following field are not cleared in Reset() */
        private TextMeshStyle defaultStyle = TextMeshStyle.Default;
        private string[] illegalTags = [];
        private HashSet<string> ignoreTags = [];
        private StringBuilder? sb;

        public RichTextParserResult ParseText(string? rawText, RichTextParserSetting setting)
        {
            if (rawText == null)
                return new RichTextParserResult(Array.Empty<LineInfo>(), Array.Empty<IParameter>(), setting.ParameterIndex);

            if (parserCache.TryGet((rawText, setting), out RichTextParserResult cachedResult))
            {
                return cachedResult;
            }

            lock (parserLock)
            {
                // Reset
                Reset();
                defaultStyle = setting.DefaultStyle;
                illegalTags = setting.IllegalTags;
                ignoreTags = setting.IgnoreTags;

                sb = StringBuilderPool.Instance.Rent();

                // Tokenize the raw text.
                Tokenizer tokenizer = TokenizerPool.Instance.Rent();
                List<Token> tokens = tokenizer.Tokenize(rawText, setting.Parameters);
                TokenizerPool.Instance.Return(tokenizer);

                // Start handling tokens.
                int parameterIndex = setting.ParameterIndex;

                for (int i = 0; i < tokens.Count; i++)
                {
                    switch (tokens[i].Type)
                    {
                        case RichTextTokenType.Text:
                            HandleText(tokens[i].Text!);
                            break;
                        case RichTextTokenType.OpenTag:
                            HandleOpenTag(tokens[i]);
                            break;
                        case RichTextTokenType.CloseTag:
                            HandleCloseTag(tokens[i]);
                            break;
                        case RichTextTokenType.SelfCloseTag:
                            HandleSelfCloseTag(tokens[i]);
                            break;
                        case RichTextTokenType.Parameter:
                            HandleText($"{{{parameterIndex}}}");
                            parameterIndex++;
                            parameters.Add(tokens[i].Parameter!);
                            break;
                        case RichTextTokenType.LineBreak:
                            FinishLine(defaultStyle, false);
                            break;
                    }
                }

                if (setting.CloseUnclosedTags)
                    CloseUnclosedTag();

                // Finish last line
                FinishLine(defaultStyle, false);

                LineInfo[] lineInfosArray = lineInfos.ToArray();
                IParameter[] parametersArray = parameters.ToArray();

                Reset();

                // Return pool objects
                StringBuilderPool.Instance.Return(sb);
                sb = null;

                return new RichTextParserResult(lineInfosArray, parametersArray, parameterIndex);
            }
        }

        #region Parsing Helpers

        /// <summary>
        /// Try to parse a <see cref="HintAlignment"/> from a tag value string.
        /// Supported values: left, center, right, justified, flush.
        /// </summary>
        private static bool TryParseAlignment(string? value, out HintAlignment alignment)
        {
            alignment = default;

            if (string.IsNullOrEmpty(value))
                return false;

            switch (value)
            {
                case "left":
                    alignment = HintAlignment.Left;
                    return true;
                case "center":
                    alignment = HintAlignment.Center;
                    return true;
                case "right":
                    alignment = HintAlignment.Right;
                    return true;
                case "justified":
                    alignment = HintAlignment.Justified;
                    return true;
                case "flush":
                    alignment = HintAlignment.Flush;
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Try to parse a hex alpha value. Supports format: #XX (e.g., "#FF", "#80").
        /// </summary>
        private static bool TryParseAlpha(string? value, out byte alpha)
        {
            alpha = 255;

            if (string.IsNullOrEmpty(value))
                return false;

            // Strip leading '#' if present
            string hex = value!.StartsWith("#") ? value.Substring(1) : value;

            if (hex.Length == 2)
            {
                return byte.TryParse(hex, System.Globalization.NumberStyles.HexNumber, null, out alpha);
            }

            return false;
        }

        /// <summary>
        /// Try to parse a <see cref="Color"/> from a tag value string.
        /// Supports named colors (red, green, blue, etc.) and hex formats (#RGB, #RRGGBB, #RRGGBBAA).
        /// </summary>
        private static bool TryParseColor(string? value, out Color color)
        {
            color = default;

            if (string.IsNullOrEmpty(value))
                return false;

            // Try named colors first
            switch (value)
            {
                case "red": color = new Color(255, 0, 0, 255); return true;
                case "green": color = new Color(0, 128, 0, 255); return true;
                case "blue": color = new Color(0, 0, 255, 255); return true;
                case "white": color = new Color(255, 255, 255, 255); return true;
                case "black": color = new Color(0, 0, 0, 255); return true;
                case "yellow": color = new Color(255, 255, 0, 255); return true;
                case "cyan": color = new Color(0, 255, 255, 255); return true;
                case "magenta": color = new Color(255, 0, 255, 255); return true;
                case "orange": color = new Color(255, 165, 0, 255); return true;
                case "purple": color = new Color(128, 0, 128, 255); return true;
                case "grey":
                case "gray": color = new Color(128, 128, 128, 255); return true;
            }

            // Try hex format
            string hex = value!.StartsWith("#") ? value.Substring(1) : value;

            try
            {
                byte r, g, b, a = 255;

                if (hex.Length == 6) // RRGGBB
                {
                    r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    color = new Color(r, g, b, a);
                    return true;
                }
                else if (hex.Length == 8) // RRGGBBAA
                {
                    r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
                    g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
                    b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
                    a = byte.Parse(hex.Substring(6, 2), System.Globalization.NumberStyles.HexNumber);
                    color = new Color(r, g, b, a);
                    return true;
                }
                else if (hex.Length == 3) // RGB shorthand
                {
                    r = byte.Parse(new string(hex[0], 2), System.Globalization.NumberStyles.HexNumber);
                    g = byte.Parse(new string(hex[1], 2), System.Globalization.NumberStyles.HexNumber);
                    b = byte.Parse(new string(hex[2], 2), System.Globalization.NumberStyles.HexNumber);
                    color = new Color(r, g, b, a);
                    return true;
                }
            }
            catch
            {
                // Parse failed
            }

            return false;
        }

        /// <summary>
        /// Try to parse a float value from a string, stripping any unit suffixes.
        /// Used for tags whose value is always a plain number (e.g., rotate, font-weight).
        /// </summary>
        private static bool TryParseFloat(string? value, out float result)
        {
            result = 0f;

            if (string.IsNullOrEmpty(value))
                return false;

            string trimmed = value!.Trim();

            return float.TryParse(trimmed, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out result);
        }

        /// <summary>
        /// Core method: parse a string like "12px", "1.5em", "50%", or "24" into
        /// a <see cref="MeasuredValue"/> with the correct <see cref="MeasureUnit"/>.
        /// </summary>
        private static bool TryParseToMeasuredValue(string? value, out MeasuredValue result)
        {
            result = default;

            if (string.IsNullOrEmpty(value))
                return false;

            float numericValue;
            MeasureUnit unit;

            if (value!.EndsWith("px"))
            {
                unit = MeasureUnit.Pixel;
                value = value.Substring(0, value.Length - 2);
            }
            else if (value.EndsWith("em"))
            {
                unit = MeasureUnit.FontUnit;
                value = value.Substring(0, value.Length - 2);
            }
            else if (value.EndsWith("%"))
            {
                unit = MeasureUnit.Percentage;
                value = value.Substring(0, value.Length - 1);
            }
            else
            {
                // No unit suffix — default to pixels.
                unit = MeasureUnit.Pixel;
            }

            if (!float.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out numericValue))
                return false;

            result = new MeasuredValue
            {
                Value = numericValue,
                Unit = unit,
            };
            return true;
        }

        /// <summary>
        /// Attempts to parse a measured value string and convert it to pixel units.
        /// </summary>
        /// <param name="value">The measured value to parse, such as a string representing a length or size. Can be null.</param>
        /// <param name="fontSize">The font size, in pixels, used for relative unit conversions. Can be null if not applicable.</param>
        /// <param name="targetValue">An optional target value, in pixels, used for certain relative conversions. Can be null if not required.</param>
        /// <param name="result">When this method returns, contains the parsed value in pixels if the conversion succeeds; otherwise, null.</param>
        /// <returns>true if the value was successfully parsed and converted to pixels; otherwise, false.</returns>
        private static bool TryParseToPixels(string? value, float? fontSize, float? targetValue, out float? result)
        {
            result = 0f;

            if (!TryParseToMeasuredValue(value, out MeasuredValue measured))
                return false;

            return measured.TryGetPixels(fontSize, targetValue, out result);
        }

        #endregion

        private void CloseUnclosedTag()
        {
            // NoParse must be closed first, otherwise subsequent closing tags would be treated as text
            if (currentStyle.NoParse)
            {
                currentStyle.NoParse = false;
                if (!illegalTags.Contains("noparse"))
                    sb!.Append("</noparse>");
            }

            // ── Stack-based tags ──────────────────────────────────────
            for (int i = 0; i < currentStyle.Alignment.Count; i++)
            {
                if (!illegalTags.Contains("align"))
                    sb!.Append("</align>");
            }

            currentStyle.Alignment.Clear();

            for (int i = 0; i < currentStyle.Color.Count; i++)
            {
                if (!illegalTags.Contains("color"))
                    sb!.Append("</color>");
            }

            currentStyle.Color.Clear();

            for (int i = 0; i < currentStyle.Indent.Count; i++)
            {
                if (!illegalTags.Contains("indent"))
                    sb!.Append("</indent>");
            }

            currentStyle.Indent.Clear();

            for (int i = 0; i < currentStyle.Mark.Count; i++)
            {
                if (!illegalTags.Contains("mark"))
                    sb!.Append("</mark>");
            }

            currentStyle.Mark.Clear();

            for (int i = 0; i < currentStyle.FontSize.Count; i++)
            {
                if (!illegalTags.Contains("size"))
                    sb!.Append("</size>");
            }

            currentStyle.FontSize.Clear();

            // ── Counter-based tags ────────────────────────────────────
            for (int i = 0; i < currentStyle.AllCaps; i++)
            {
                if (!illegalTags.Contains("allcaps"))
                    sb!.Append("</allcaps>");
            }

            currentStyle.AllCaps = 0;

            for (int i = 0; i < currentStyle.Bold; i++)
            {
                if (!illegalTags.Contains("b"))
                    sb!.Append("</b>");
            }

            currentStyle.Bold = 0;

            for (int i = 0; i < currentStyle.Italic; i++)
            {
                if (!illegalTags.Contains("i"))
                    sb!.Append("</i>");
            }

            currentStyle.Italic = 0;

            for (int i = 0; i < currentStyle.Lowercase; i++)
            {
                if (!illegalTags.Contains("lowercase"))
                    sb!.Append("</lowercase>");
            }

            currentStyle.Lowercase = 0;

            for (int i = 0; i < currentStyle.Strikethrough; i++)
            {
                if (!illegalTags.Contains("s"))
                    sb!.Append("</s>");
            }

            currentStyle.Strikethrough = 0;

            for (int i = 0; i < currentStyle.Subscript; i++)
            {
                if (!illegalTags.Contains("sub"))
                    sb!.Append("</sub>");
            }

            currentStyle.Subscript = 0;

            for (int i = 0; i < currentStyle.Superscript; i++)
            {
                if (!illegalTags.Contains("sup"))
                    sb!.Append("</sup>");
            }

            currentStyle.Superscript = 0;

            for (int i = 0; i < currentStyle.Underline; i++)
            {
                if (!illegalTags.Contains("u"))
                    sb!.Append("</u>");
            }

            currentStyle.Underline = 0;

            for (int i = 0; i < currentStyle.Uppercase; i++)
            {
                if (!illegalTags.Contains("uppercase"))
                    sb!.Append("</uppercase>");
            }

            currentStyle.Uppercase = 0;

            // ── Single-value tags ─────────────────────────────────────
            if (currentStyle.Alpha.HasValue)
            {
                currentStyle.Alpha = null;
                if (!illegalTags.Contains("alpha"))
                    sb!.Append("</alpha>");
            }

            if (currentStyle.CharSpace.HasValue)
            {
                currentStyle.CharSpace = null;
                if (!illegalTags.Contains("cspace"))
                    sb!.Append("</cspace>");
            }

            if (currentStyle.Font != null)
            {
                currentStyle.Font = null;
                if (!illegalTags.Contains("font"))
                    sb!.Append("</font>");
            }

            if (currentStyle.FontWeight.HasValue)
            {
                currentStyle.FontWeight = null;
                if (!illegalTags.Contains("font-weight"))
                    sb!.Append("</font-weight>");
            }

            if (currentStyle.LineHeight.HasValue)
            {
                currentStyle.LineHeight = null;
                if (!illegalTags.Contains("line-height"))
                    sb!.Append("</line-height>");
            }

            if (currentStyle.LineIndent.HasValue)
            {
                currentStyle.LineIndent = null;
                if (!illegalTags.Contains("line-indent"))
                    sb!.Append("</line-indent>");
            }

            if (currentStyle.MarginLeft.HasValue)
            {
                currentStyle.MarginLeft = null;
                if (!illegalTags.Contains("margin-left"))
                    sb!.Append("</margin-left>");
            }

            if (currentStyle.MarginRight.HasValue)
            {
                currentStyle.MarginRight = null;
                if (!illegalTags.Contains("margin-right"))
                    sb!.Append("</margin-right>");
            }

            if (currentStyle.Monospace.HasValue)
            {
                currentStyle.Monospace = null;
                if (!illegalTags.Contains("mspace"))
                    sb!.Append("</mspace>");
            }

            if (currentStyle.Rotate.HasValue)
            {
                currentStyle.Rotate = null;
                if (!illegalTags.Contains("rotate"))
                    sb!.Append("</rotate>");
            }

            if (currentStyle.VOffset.HasValue)
            {
                currentStyle.VOffset = null;
                if (!illegalTags.Contains("voffset"))
                    sb!.Append("</voffset>");
            }

            if (currentStyle.Width.HasValue)
            {
                currentStyle.Width = null;
                if (!illegalTags.Contains("width"))
                    sb!.Append("</width>");
            }

            // ── Boolean tags ──────────────────────────────────────────
            if (currentStyle.NoBreak)
            {
                currentStyle.NoBreak = false;
                if (!illegalTags.Contains("nobr"))
                    sb!.Append("</nobr>");
            }

            if (currentStyle.Smallcap)
            {
                currentStyle.Smallcap = false;
                if (!illegalTags.Contains("smallcaps"))
                    sb!.Append("</smallcaps>");
            }

            // Clear caches since styles have been reset
            ClearCharStyleCache();
            ClearLineStyleCache();
        }

        private void HandleText(string text)
        {
            sb!.Append(text);

            if (charStyleCache == null)
            {
                charStyleCache = currentStyle.GetCharStyle(defaultStyle);
            }

            float totalWidthWithFontSize = 0f;
            for (int i = 0; i < text.Length; i++)
            {
                totalWidthWithFontSize += FontTool.Instance.GetCharWidth(text[i], charStyleCache.FontSize);
            }

            float actualWidth = charStyleCache.GetWidth(totalWidthWithFontSize, text.Length);

            currentLineChars.Add(new TextSegment(text, actualWidth, charStyleCache));
        }

        private void AddPlaceholder(float width)
        {
            if (charStyleCache == null)
            {
                charStyleCache = currentStyle.GetCharStyle(defaultStyle);
            }

            TextSegment charInfo = new TextSegment(" ", width, charStyleCache);
            currentLineChars.Add(charInfo);
        }

        private void ClearLineStyleCache()
        {
            lineStyleAutoWrappedCache = null;
            lineStyleNonAutoWrappedCache = null;
        }

        private void ClearCharStyleCache()
        {
            charStyleCache = null;
        }

        private void Reset()
        {
            parameters.Clear();
            lineInfos.Clear();
            currentLineChars.Clear();

            currentStyle.Clear();

            ClearLineStyleCache();
            ClearCharStyleCache();
        }

        #region Tag Handlers

        private void HandleOpenTag(Token token)
        {
            if (currentStyle.NoParse
                || ignoreTags.Contains(token.TagName!))
            {
                sb!.Append('<').Append(token.TagName);
                if (!string.IsNullOrEmpty(token.TagValue))
                {
                    sb.Append('=').Append(token.TagValue);
                }

                sb.Append('>');
                return;
            }

            string tagName = token.TagName ?? string.Empty;
            string? value = token.TagValue;

            switch (tagName)
            {
                /* ── Stack-based tags ────────────────────────────────────── */
                case "align":
                    if (TryParseAlignment(value, out HintAlignment alignment))
                        currentStyle.Alignment.Push(alignment);
                    ClearLineStyleCache();
                    break;

                case "color":
                    if (TryParseColor(value, out Color color))
                        currentStyle.Color.Push(color);
                    ClearCharStyleCache();
                    break;

                case "indent":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? indentVal))
                        currentStyle.Indent.Push(indentVal.Value);
                    ClearLineStyleCache();
                    break;

                case "mark":
                    if (TryParseColor(value, out Color markColor))
                        currentStyle.Mark.Push(markColor);
                    ClearCharStyleCache();
                    break;

                case "size":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.CharStyle.FontSize, out float? sizeVal))
                        currentStyle.FontSize.Push(sizeVal.Value);
                    ClearCharStyleCache();
                    break;

                /* ── Counter-based tags ──────────────────────────────────── */
                case "allcaps":
                    currentStyle.AllCaps++;
                    ClearCharStyleCache();
                    break;

                case "b":
                    currentStyle.Bold++;
                    ClearCharStyleCache();
                    break;

                case "i":
                    currentStyle.Italic++;
                    ClearCharStyleCache();
                    break;

                case "lowercase":
                    currentStyle.Lowercase++; ClearCharStyleCache();
                    break;

                case "s":
                    currentStyle.Strikethrough++; ClearCharStyleCache();
                    break;

                case "sub":
                    currentStyle.Subscript++; ClearCharStyleCache();
                    break;

                case "sup":
                    currentStyle.Superscript++; ClearCharStyleCache();
                    break;

                case "u":
                    currentStyle.Underline++; ClearCharStyleCache();
                    break;

                case "uppercase":
                    currentStyle.Uppercase++; ClearCharStyleCache();
                    break;

                /* ── Single-value tags ───────────────────────────────────── */
                case "cspace":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), null, out float? cspaceVal))
                        currentStyle.CharSpace = cspaceVal;
                    ClearCharStyleCache();
                    break;

                case "font":
                    currentStyle.Font = value;
                    ClearCharStyleCache();
                    break;

                case "font-weight":
                    if (int.TryParse(value, out int fontWeightVal))
                        currentStyle.FontWeight = fontWeightVal;
                    ClearCharStyleCache();
                    break;

                case "line-height": // TODO: Check if should use actual line-height instead of font size
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.CharStyle.GetHeight(), out float? lineHeightVal))
                        currentStyle.LineHeight = lineHeightVal;
                    ClearLineStyleCache();
                    break;

                case "line-indent":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? lineIndentVal))
                        currentStyle.LineIndent = lineIndentVal;
                    ClearLineStyleCache();
                    break;

                case "margin":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? marginVal))
                    {
                        currentStyle.MarginLeft = marginVal;
                        currentStyle.MarginRight = marginVal;
                    }

                    ClearLineStyleCache();
                    break;

                case "margin-left":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? marginLeftVal))
                        currentStyle.MarginLeft = marginLeftVal;
                    ClearLineStyleCache();
                    break;

                case "margin-right":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? marginRightVal))
                        currentStyle.MarginRight = marginRightVal;
                    ClearLineStyleCache();
                    break;

                case "mspace":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), null, out float? mspaceVal))
                        currentStyle.Monospace = mspaceVal;
                    ClearCharStyleCache();
                    break;

                case "rotate":
                    if (TryParseFloat(value, out float rotateVal))
                        currentStyle.Rotate = rotateVal;
                    ClearCharStyleCache();
                    break;

                case "voffset":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), null, out float? voffsetVal))
                        currentStyle.VOffset = voffsetVal;
                    ClearCharStyleCache();
                    break;

                case "width":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? widthVal))
                        currentStyle.Width = widthVal;
                    ClearLineStyleCache();
                    break;

                /* ── Boolean tags ────────────────────────────────────────── */
                case "nobr":
                    currentStyle.NoBreak = true;
                    break;

                case "noparse":
                    currentStyle.NoParse = true;
                    break;

                case "smallcaps":
                    currentStyle.Smallcap = true;
                    ClearCharStyleCache();
                    break;

                // ── Ignored / unimplemented tags ──────────────────────────
                // <a>, <gradient>, <link>, <style>, <sprite>
                default:
                    break;
            }

            if (!illegalTags.Contains(tagName))
            {
                sb!.Append('<').Append(tagName);

                if (!string.IsNullOrEmpty(value))
                {
                    sb.Append('=').Append(value);
                }

                sb.Append('>');
            }
        }

        private void HandleCloseTag(Token token)
        {
            if ((currentStyle.NoParse && token.TagName != "noparse")
                || ignoreTags.Contains(token.TagName!))
            {
                sb!.Append("</").Append(token.TagName).Append('>');
                return;
            }

            string tagName = token.TagName ?? string.Empty;

            switch (tagName)
            {
                // ── Stack-based tags ──────────────────────────────────────
                case "align":
                    if (currentStyle.Alignment.Count > 0)
                        currentStyle.Alignment.Pop();
                    ClearLineStyleCache();
                    break;

                case "color":
                    if (currentStyle.Color.Count > 0)
                        currentStyle.Color.Pop();
                    ClearCharStyleCache();
                    break;

                case "indent":
                    if (currentStyle.Indent.Count > 0)
                        currentStyle.Indent.Pop();
                    ClearLineStyleCache();
                    break;

                case "mark":
                    if (currentStyle.Mark.Count > 0)
                        currentStyle.Mark.Pop();
                    ClearCharStyleCache();
                    break;

                case "size":
                    if (currentStyle.FontSize.Count > 0)
                        currentStyle.FontSize.Pop();
                    ClearCharStyleCache();
                    break;

                // ── Counter-based tags ────────────────────────────────────
                case "allcaps":
                    if (currentStyle.AllCaps > 0)
                        currentStyle.AllCaps--;
                    ClearCharStyleCache();
                    break;

                case "b":
                    if (currentStyle.Bold > 0)
                        currentStyle.Bold--;
                    ClearCharStyleCache();
                    break;

                case "i":
                    if (currentStyle.Italic > 0)
                        currentStyle.Italic--;
                    ClearCharStyleCache();
                    break;

                case "lowercase":
                    if (currentStyle.Lowercase > 0)
                        currentStyle.Lowercase--;
                    ClearCharStyleCache();
                    break;

                case "s":
                    if (currentStyle.Strikethrough > 0)
                        currentStyle.Strikethrough--;
                    ClearCharStyleCache();
                    break;

                case "sub":
                    if (currentStyle.Subscript > 0)
                        currentStyle.Subscript--;
                    ClearCharStyleCache();
                    break;

                case "sup":
                    if (currentStyle.Superscript > 0)
                        currentStyle.Superscript--;
                    ClearCharStyleCache();
                    break;

                case "u":
                    if (currentStyle.Underline > 0)
                        currentStyle.Underline--;
                    ClearCharStyleCache();
                    break;

                case "uppercase":
                    if (currentStyle.Uppercase > 0)
                        currentStyle.Uppercase--;
                    ClearCharStyleCache();
                    break;

                // ── Single-value tags ─────────────────────────────────────
                case "alpha":
                    currentStyle.Alpha = null; ClearCharStyleCache();
                    break;

                case "cspace":
                    currentStyle.CharSpace = null; ClearCharStyleCache();
                    break;

                case "font":
                    currentStyle.Font = null; ClearCharStyleCache();
                    break;

                case "font-weight":
                    currentStyle.FontWeight = null; ClearCharStyleCache();
                    break;

                case "line-height":
                    currentStyle.LineHeight = null; ClearLineStyleCache();
                    break;

                case "line-indent":
                    currentStyle.LineIndent = null; ClearLineStyleCache();
                    break;

                case "margin":
                    currentStyle.MarginLeft = null;
                    currentStyle.MarginRight = null; ClearLineStyleCache();
                    break;

                case "margin-left":
                    currentStyle.MarginLeft = null; ClearLineStyleCache();
                    break;

                case "margin-right":
                    currentStyle.MarginRight = null; ClearLineStyleCache();
                    break;

                case "mspace":
                    currentStyle.Monospace = null; ClearCharStyleCache();
                    break;

                case "rotate":
                    currentStyle.Rotate = null; ClearCharStyleCache();
                    break;

                case "voffset":
                    currentStyle.VOffset = null; ClearCharStyleCache();
                    break;

                case "width":
                    currentStyle.Width = null; ClearLineStyleCache();
                    break;

                // ── Boolean tags ──────────────────────────────────────────
                case "nobr":
                    currentStyle.NoBreak = false;
                    break;

                case "noparse":
                    currentStyle.NoParse = false;
                    break;

                case "smallcaps":
                    currentStyle.Smallcap = false; ClearCharStyleCache();
                    break;

                // ── Ignored / unimplemented tags ──────────────────────────
                default:
                    break;
            }

            if (!illegalTags.Contains(tagName))
                sb!.Append("</").Append(tagName).Append('>');
        }

        private void HandleSelfCloseTag(Token token)
        {
            if (currentStyle.NoParse
                || ignoreTags.Contains(token.TagName!))
            {
                sb!.Append('<').Append(token.TagName);
                if (!string.IsNullOrEmpty(token.TagValue))
                {
                    sb.Append('=').Append(token.TagValue);
                }

                sb.Append("/>");
                return;
            }

            string tagName = token.TagName ?? string.Empty;
            string value = token.TagValue ?? string.Empty;

            switch (tagName)
            {
                case "br":
                    // In case it comes through as a self-close token instead of LineBreak
                    FinishLine(defaultStyle, false);
                    break;

                case "alpha":
                    if (TryParseAlpha(value, out byte alphaVal))
                        currentStyle.Alpha = alphaVal;
                    ClearCharStyleCache();
                    break;

                case "pos":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? posVal))
                    {
                        if (posVal.HasValue)
                            AddPlaceholder(posVal.Value);
                    }

                    break;

                case "space":
                    if (TryParseToPixels(value, currentStyle.GetActualSize(defaultStyle.CharStyle.FontSize), defaultStyle.Width, out float? spaceVal))
                    {
                        if (spaceVal.HasValue)
                            AddPlaceholder(spaceVal.Value);
                    }

                    break;

                default:
                    // Other self-closing tags (pos, space, sprite, etc.) are currently not supported.
                    break;
            }

            if (!illegalTags.Contains(tagName))
            {
                sb!.Append('<').Append(tagName);
                if (!string.IsNullOrEmpty(value))
                {
                    sb.Append('=').Append(value);
                }

                sb.Append('>');
            }
        }

        #endregion

        private void FinishLine(TextMeshStyle defaultStyle, bool isAutoWrapped) // TODO: isAutoWrapped should be determined by previous line's reason of breaking, but not how this line break
        {
            LineStyle lineStyle;
            if (isAutoWrapped)
            {
                if (lineStyleAutoWrappedCache == null)
                {
                    lineStyleAutoWrappedCache = currentStyle.GetLineStyle(defaultStyle, true);
                }

                lineStyle = lineStyleAutoWrappedCache;
            }
            else
            {
                if (lineStyleNonAutoWrappedCache == null)
                {
                    lineStyleNonAutoWrappedCache = currentStyle.GetLineStyle(defaultStyle, false);
                }

                lineStyle = lineStyleNonAutoWrappedCache;
            }

            lineInfos.Add(new LineInfo(currentLineChars.ToArray(), lineStyle, sb!.ToString()));

            currentLineChars.Clear();
            sb.Clear();
        }

        private class Style
        {
            #region Tag handling data
            /* https://docs.unity3d.com/Manual/UIE-supported-tags.html */
            /* Skip <a> */

            /// <summary>
            /// Gets the stack of hint alignments used for layout or rendering purposes.
            /// </summary>
            public Stack<HintAlignment> Alignment { get; } = new(8);

            /// <summary>
            /// Gets or sets the number of active AllCaps tags, which indicates whether the text should be rendered in all capital letters.
            /// </summary>
            public int AllCaps { get; set; } = 0;

            /// <summary>
            /// Gets or sets the alpha channel value for the color. Unit in 0-255, where 0 is fully transparent and 255 is fully opaque.
            /// </summary>
            /// <remarks>A null value indicates that the alpha channel is in its default value. The alpha
            /// channel determines the transparency of the color, where 0 is fully transparent and 255 is fully
            /// opaque.</remarks>
            public byte? Alpha { get; set; } = null;

            /// <summary>
            /// Gets or sets the number of active Bold tags, which indicates whether the text should be rendered in bold style.
            /// </summary>
            public int Bold { get; set; } = 0;

            /* Skip <br> */

            /// <summary>
            /// Gets the stack of colors used for rendering text.
            /// </summary>
            public Stack<Color> Color { get; } = new(8);

            /// <summary>
            /// Gets or sets the additional spacing, in pixels, to apply between characters when rendering text.
            /// Null if the character spacing is in its default value. 
            /// </summary>
            public float? CharSpace { get; set; } = null;

            /// <summary>
            /// GEts or sets the name of the font to be used for rendering text. Null if the font is in its default value.
            /// </summary>
            public string? Font { get; set; } = null;

            /// <summary>
            /// Gets or sets the font weight to apply to the content. A higher value typically results in a bolder
            /// appearance.
            /// </summary>
            public int? FontWeight { get; set; } = null; // TODO: Check if this is a stack tag

            /* Skip gradient */

            /// <summary>
            /// Gets or sets the number of active Italic tags, which indicates whether the text should be rendered in italic style.
            /// </summary>
            public int Italic { get; set; } = 0;

            /// <summary>
            /// Gets the stack of measured indentation values. Unit in pixels.
            /// </summary>
            public Stack<float> Indent { get; } = new(8);

            /// <summary>
            /// Gets or sets the line height. Unit in pixels.
            /// </summary>
            public float? LineHeight { get; set; } = null;

            /// <summary>
            /// Gets or sets the amount of indentation applied to each line, in pixels.
            /// </summary>
            public float? LineIndent { get; set; } = null;

            /// <summary>
            /// Gets or sets the number of active Lowercase tags, which indicates whether the text should be rendered in lowercase letters.
            /// </summary>
            public int Lowercase { get; set; } = 0;

            /// <summary>
            /// Gets or sets the left margin value for the element, in pixel units.
            /// </summary>
            public float? MarginLeft { get; set; } = null;

            /// <summary>
            /// Gets or sets the right margin value for the layout, in pixel units.
            /// </summary>
            public float? MarginRight { get; set; } = null;

            /// <summary>
            /// Gets the stack of colors used for marking text.
            /// </summary>
            public Stack<Color> Mark { get; } = new(8);

            /// <summary>
            /// Gets or sets the monospace font size to be used for rendering text, in pixels.
            /// </summary>
            public float? Monospace { get; set; } = null;

            public bool NoBreak { get; set; } = false;

            public bool NoParse { get; set; } = false;

            /* Skip <pos> */

            public float? Rotate { get; set; } = null;

            public int Strikethrough { get; set; } = 0;

            public Stack<float> FontSize { get; set; } = new(8);

            public bool Smallcap { get; set; }

            /* Skip <space> */
            /* Skip <sprite> */
            /* Skip <style> */

            public int Subscript { get; set; } = 0;

            public int Superscript { get; set; } = 0;

            public int Underline { get; set; } = 0;

            public int Uppercase { get; set; } = 0;

            public float? VOffset { get; set; } = null;

            public float? Width { get; set; } = null;

            /* Skip <link> */
            #endregion

            public void Clear()
            {
                // Reset stacks
                Alignment.Clear();
                Color.Clear();
                Indent.Clear();
                Mark.Clear();
                FontSize.Clear();

                // Reset counters
                AllCaps = 0;
                Bold = 0;
                Italic = 0;
                Lowercase = 0;
                Strikethrough = 0;
                Subscript = 0;
                Superscript = 0;
                Underline = 0;
                Uppercase = 0;

                // Reset bools
                NoBreak = false;
                NoParse = false;
                Smallcap = false;

                // Reset nullable
                Alpha = null;
                CharSpace = null;
                Font = null;
                LineHeight = null;
                LineIndent = null;
                MarginLeft = null;
                MarginRight = null;
                Monospace = null;
                Rotate = null;
                VOffset = null;
                Width = null;
                FontWeight = null;
            }

            public float GetActualSize(float defaultSize)
            {
                if (FontSize.Count > 0)
                    return FontSize.Peek();
                else
                    return defaultSize;
            }

            public TextSegmentStyle GetCharStyle(TextMeshStyle defaultStyle)
            {
                float currentFontSize = FontSize.Count > 0 ? FontSize.Peek() : defaultStyle.CharStyle.FontSize;
                Color currentColor = Color.Count > 0 ? Color.Peek() : defaultStyle.CharStyle.Color;
                float? charSpace = CharSpace ?? defaultStyle.CharStyle.CharSpace;
                float? monoSpace = Monospace ?? defaultStyle.CharStyle.Monospace;

                return new TextSegmentStyle(
                    fontSize: currentFontSize,
                    color: currentColor,
                    alpha: Alpha.HasValue ? (Alpha.Value / 255f) : 1f,
                    bold: Bold > 0 ? true : defaultStyle.CharStyle.Bold,
                    italic: Italic > 0 ? true : defaultStyle.CharStyle.Italic,
                    underline: Underline > 0 ? true : defaultStyle.CharStyle.Underline,
                    strikethrough: Strikethrough > 0 ? true : defaultStyle.CharStyle.Strikethrough,
                    superscript: Superscript > 0 ? Superscript : defaultStyle.CharStyle.Superscript,
                    subscript: Subscript > 0 ? Subscript : defaultStyle.CharStyle.Subscript,
                    vOffset: VOffset ?? defaultStyle.CharStyle.VOffset,
                    rotate: Rotate ?? defaultStyle.CharStyle.Rotate,
                    charSpace: charSpace,
                    monospace: monoSpace,
                    mark: Mark.Count > 0 ? Mark.Peek() : defaultStyle.CharStyle.Mark,
                    font: Font ?? defaultStyle.CharStyle.Font,
                    fontWeight: FontWeight ?? defaultStyle.CharStyle.FontWeight);
            }

            public LineStyle GetLineStyle(TextMeshStyle defaultStyle, bool isAutoWrapped)
            {
                float marginLeftValue = MarginLeft ?? defaultStyle.LineStyle.MarginLeft;
                float marginRightValue = MarginRight ?? defaultStyle.LineStyle.MarginRight;
                float? lineHeightValue = LineHeight ?? defaultStyle.LineStyle.LineHeight;
                float lineIndentValue = LineIndent ?? defaultStyle.LineStyle.Indent;

                HintAlignment alignment = Alignment.Count > 0 ? Alignment.Peek() : defaultStyle.LineStyle.Alignment;
                float actualIndent;
                if (Indent.Count == 0 && LineIndent == null) // Default value
                {
                    actualIndent = defaultStyle.LineStyle.Indent;
                }
                else
                {
                    actualIndent = Indent.Count > 0 ? Indent.Peek() : defaultStyle.LineStyle.Indent;
                    if (!isAutoWrapped)
                    {
                        actualIndent += LineIndent == null ? 0f : LineIndent.Value;
                    }
                }

                return new LineStyle(
                    alignment: alignment,
                    lineHeight: lineHeightValue,
                    indent: actualIndent,
                    marginLeft: marginLeftValue,
                    marginRight: marginRightValue,
                    maxWidth: Width ?? defaultStyle.LineStyle.MaxWidth);
            }
        }
    }
}