using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities;

namespace HintServiceMeow.Core.Models.Arguments
{
    public class ContentUpdateArg
    {
        public AbstractHint Hint { get; }
        public PlayerDisplay PlayerDisplay { get; }

        internal ContentUpdateArg(AbstractHint hint, PlayerDisplay playerDisplay)
        {
            this.Hint = hint;
            this.PlayerDisplay = playerDisplay;
        }
    }
}
