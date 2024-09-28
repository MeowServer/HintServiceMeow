using HintServiceMeow.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HintServiceMeow.Core.Models.Hints;

namespace HintServiceMeow.Core.Extension
{
    public static class HintExtension
    {
        private static readonly Dictionary<Hint, TaskScheduler> HideTime = new Dictionary<Hint, TaskScheduler>();

        public static void HideAfter(this Hint hint, float delay)
        {
            if (!HideTime.TryGetValue(hint, out var scheduler))
                HideTime.Add(hint, scheduler = new TaskScheduler(TimeSpan.Zero, () => hint.Hide = true));

            scheduler.StartAction(delay, TaskScheduler.DelayType.Latest);
        }
    }
}
