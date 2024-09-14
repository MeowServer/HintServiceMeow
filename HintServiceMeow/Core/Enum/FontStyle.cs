using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Enum
{
    [Flags]
    internal enum TextStyle
    {
        Normal = 0x0000,
        Bold = 0x0001,
        Italic = 0x0010,
    }
}
