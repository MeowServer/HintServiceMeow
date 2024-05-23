using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow
{
    public enum HintAlignment
    {
        Left,
        Right,
        Center
    }

    public class DynamicHintField
    {
        public int topYCoordinate;
        public int bottomYCoordinate;

        public DynamicHintField(int topYCoordinate, int bottomYCoordinate)
        {
            this.topYCoordinate = topYCoordinate;
            this.bottomYCoordinate = bottomYCoordinate;
        }
    }
}