using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using System;
using System.Runtime.CompilerServices;

namespace HintServiceMeow.Core.Extension
{
    public static class HintExtension
    {
        private static readonly ConditionalWeakTable<AbstractHint, TaskScheduler> HideTimers = new();

        /// <summary>
        /// Set Hint.Hide to true after a delay. If a hiding task is in progress, it will be reset.
        /// </summary>
        public static void HideAfter(this AbstractHint hint, float delay)
        {
            if (!HideTimers.TryGetValue(hint, out TaskScheduler scheduler))
            {
                scheduler = new TaskScheduler();
                scheduler.Start(TimeSpan.Zero, () => hint.Hide = true);

                HideTimers.Add(hint, scheduler);
            }

            scheduler.Invoke(delay, DelayType.Override);
        }
    }
}