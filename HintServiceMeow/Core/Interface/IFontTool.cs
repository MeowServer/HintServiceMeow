using HintServiceMeow.Core.Enum;

namespace HintServiceMeow.Core.Interface
{
    internal interface IFontTool
    {
        float GetCharWidth(char c, float fontSize, TextStyle style);
    }
}
