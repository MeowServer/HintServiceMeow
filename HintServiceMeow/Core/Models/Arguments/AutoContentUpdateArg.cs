using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using System;

namespace HintServiceMeow.Core.Models.Arguments
{
    public class AutoContentUpdateArg
    {
        public AbstractHint Hint { get; }
        public PlayerDisplay PlayerDisplay { get; }

        /// <summary>
        /// The delay before the next update. Count in seconds.
        /// </summary>
        public TimeSpan NextUpdateDelay { get; set; }

        public TimeSpan DefaultUpdateDelay { get; set; }

        public AutoContentUpdateArg(AbstractHint hint, PlayerDisplay playerDisplay, TimeSpan defaultUpdateDelay)
        {
            Hint = hint;
            PlayerDisplay = playerDisplay;
            NextUpdateDelay = defaultUpdateDelay;
            DefaultUpdateDelay = defaultUpdateDelay;
        }
    }
}
