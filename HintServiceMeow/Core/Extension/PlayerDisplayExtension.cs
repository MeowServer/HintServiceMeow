using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using System;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Extension
{
    public static class PlayerDisplayExtension
    {
        private static readonly Dictionary<PlayerDisplay, Dictionary<AbstractHint, TaskScheduler>> RemoveTime = new Dictionary<PlayerDisplay, Dictionary<AbstractHint, TaskScheduler>>();

        /// <summary>
        /// Remove a hint after a delay. If a removal task is in progress, it will be reset.
        /// </summary>
        public static void RemoveAfter(this PlayerDisplay playerDisplay, AbstractHint hint, float delay)
        {
            if (!RemoveTime.TryGetValue(playerDisplay, out var hintDict))
                RemoveTime.Add(playerDisplay, hintDict = new Dictionary<AbstractHint, TaskScheduler>());

            if (!hintDict.TryGetValue(hint, out var scheduler))
                hintDict.Add(hint, scheduler = new TaskScheduler(TimeSpan.Zero, () => playerDisplay.RemoveHint(hint)));

            scheduler.StartAction(delay, TaskScheduler.DelayType.Latest);
        }
    }
}