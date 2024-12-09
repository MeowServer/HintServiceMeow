using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using System;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Extension
{
    public static class HintExtension
    {
        private static readonly Dictionary<AbstractHint, TaskScheduler> HideTime = new Dictionary<AbstractHint, TaskScheduler>();

        /// <summary>
        /// Set Hint.Hide to true after a delay. If a hiding task is in progress, it will be reset.
        /// </summary>
        public static void HideAfter(this AbstractHint hint, float delay)
        {
            if (!HideTime.TryGetValue(hint, out TaskScheduler scheduler))
                HideTime.Add(hint, scheduler = new TaskScheduler(TimeSpan.Zero, () => hint.Hide = true));

            scheduler.StartAction(delay, TaskScheduler.DelayType.Override);
        }
    }
}