using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.AI;

namespace HintServiceMeow.Core.Enum
{
    public enum HintPriority
    {
        Highest = 192,
        High = 160,
        Medium = 128,
        Low = 96,
        Lowest = 64
    }

    public enum DynamicHintStrategy
    {
        /// <summary>
        /// Let dynamic hint hide itself when no position is available
        /// </summary>
        Hide,
        /// <summary>
        /// Let dynamic hint stay on target position when no position is available
        /// </summary>
        StayInPosition
    }

    public enum HintAlignment
    {
        Left,
        Right,
        Center
    }

    public enum HintVerticalAlign
    {
        Top,
        Middle,
        Bottom
    }

    public enum HintSyncSpeed
    {
        /// <summary>
        /// Fastest sync, the hint will be updated as soon as it can
        /// Using the fastest sync speed might cause delay in update of other hints
        /// </summary>
        Fastest = 192,

        /// <summary>
        /// Plan an update immediately when the hint is updated
        /// </summary>
        Fast = 160,

        /// <summary>
        /// Normal update speed
        /// </summary>
        Normal = 128,

        /// <summary>
        /// Will wait for other hints to update before update.
        /// </summary>
        Slow = 96,

        /// <summary>
        /// This hint will not automatically sync when updated
        /// It will still be sync when other hints are updated
        /// </summary>
        UnSync = 64,
    }
}