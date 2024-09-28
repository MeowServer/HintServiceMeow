using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;
using System;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Extension
{
    public static class PlayerDisplayExtension
    {
        private static readonly Dictionary<PlayerDisplay, Dictionary<Hint, TaskScheduler>> RemoveTime = new Dictionary<PlayerDisplay, Dictionary<Hint, TaskScheduler>>();

        public static void RemoveAfter(this PlayerDisplay playerDisplay, Hint hint, float delay)
        {
            if (!RemoveTime.TryGetValue(playerDisplay, out var hintDict))
                RemoveTime.Add(playerDisplay, hintDict = new Dictionary<Hint, TaskScheduler>());

            if (!hintDict.TryGetValue(hint, out var scheduler))
                hintDict.Add(hint, scheduler = new TaskScheduler(TimeSpan.Zero, () => playerDisplay.RemoveHint(hint)));

            scheduler.StartAction(delay, TaskScheduler.DelayType.Latest);
        }
    }
}
