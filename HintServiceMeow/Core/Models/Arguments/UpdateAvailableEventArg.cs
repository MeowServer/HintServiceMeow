using HintServiceMeow.Core.Utilities;

namespace HintServiceMeow.Core.Models.Arguments
{
    /// <summary>
    /// Argument for UpdateAvailable Event
    /// </summary>
    public class UpdateAvailableEventArg
    {
        public PlayerDisplay PlayerDisplay { get; set; }

        internal UpdateAvailableEventArg(PlayerDisplay playerDisplay)
        {
            this.PlayerDisplay = playerDisplay;
        }
    }
}
