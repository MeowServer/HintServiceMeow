using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models;
using HintServiceMeow.Core.Models.Hints;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Interface
{
    internal interface ICoordinateTools
    {
        float GetYCoordinate(Hint hint, HintVerticalAlign to);
        float GetYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to);
        float GetYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to);
        float GetXCoordinateWithAlignment(Hint hint);
        float GetXCoordinateWithAlignment(Hint hint, HintAlignment alignment);
        float GetTextWidth(AbstractHint hint);
        float GetTextWidth(string text, int fontSize, HintAlignment align = HintAlignment.Center);
        float GetTextHeight(AbstractHint hint);
        float GetTextHeight(string text, int fontSize, float lineHeight);
        IReadOnlyList<LineInfo> GetLineInfos(string text, int fontSize, HintAlignment align = HintAlignment.Center);
    }
}
