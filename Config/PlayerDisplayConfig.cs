using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Config
{
    public class PlayerDisplayConfig
    {
        [Description("The minimum time between each updates for player displays. 0.5 is experimental value.")]
        public TimeSpan MinUpdateInterval = TimeSpan.FromMilliseconds(500);

        [Description("The minimum time wait before each update, you can shorten it if you're confident with your server's performance, count in miliseconds")]
        public float MinTimeDelayBeforeUpdate = 50f;

        [Description("The interval between each hint content refresh, count in second.")]
        public float HintUpdateInterval = 0.1f;

        [Description("The time between each force update.")]
        public float ForceUpdateInterval = 3f;
    }
}
