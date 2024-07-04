using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;

namespace HintServiceMeow
{
    public enum HintPriority
    {
        Highest = 192,
        High = 160,
        Medium = 128,
        Low = 96,
        Lowest = 64
    }

    public enum HintAlignment
    {
        Left,
        Right,
        Center
    }

    public class DynamicHintField
    {
        internal delegate void UpdateHandler();
        internal event UpdateHandler OnUpdate;

        public int topYCoordinate;
        public int bottomYCoordinate;

        public DynamicHintField(int topYCoordinate, int bottomYCoordinate)
        {
            this.topYCoordinate = topYCoordinate;
            this.bottomYCoordinate = bottomYCoordinate;
        }
    }
}