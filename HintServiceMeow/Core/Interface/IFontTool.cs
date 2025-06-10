using HintServiceMeow.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Interface
{
    internal interface IFontTool
    {
        float GetCharWidth(char c, float fontSize, TextStyle style);
    }
}
