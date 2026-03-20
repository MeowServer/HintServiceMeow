namespace HintServiceMeow.Core.Interface
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.Hints;
    using HintServiceMeow.Core.Models.Parser;

    internal interface ICoordinateTools
    {
        float GetYCoordinate(Hint hint, HintVerticalAlign to);

        float GetYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to);

        float GetYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to);

        float GetCurrentYCoordinate(Hint hint, HintVerticalAlign to);

        float GetCurrentYCoordinate(Hint hint, HintVerticalAlign from, HintVerticalAlign to);

        float GetCurrentYCoordinate(float rawYCoordinate, float textHeight, HintVerticalAlign from, HintVerticalAlign to);

        float GetEdgeOffset(float xyRatio, HintAlignment alignment);
        
        float GetXCoordinateWithAlignment(Hint hint);

        float GetXCoordinateWithAlignment(Hint hint, HintAlignment alignment);

        float GetTextWidth(AbstractHint hint);

        float GetTextWidth(string text, int fontSize, HintAlignment align = HintAlignment.Center);

        float GetTextHeight(AbstractHint hint);

        float GetTextHeight(string text, int fontSize, float lineHeight);

        LineInfo[] GetLineInfos(string text, int fontSize, HintAlignment align = HintAlignment.Center);
    }
}
