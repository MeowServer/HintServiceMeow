using HintServiceMeow.Core.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Models.Arguments
{
    public class DisplayOutputArg
    {
        public PlayerDisplay PlayerDisplay { get; }
        public string Content { get; }

        internal DisplayOutputArg(PlayerDisplay playerDisplay, string content)
        {
            this.PlayerDisplay = playerDisplay;
            this.Content = content;
        }
    }
}
