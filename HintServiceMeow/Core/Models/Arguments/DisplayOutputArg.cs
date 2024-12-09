using HintServiceMeow.Core.Utilities;

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
