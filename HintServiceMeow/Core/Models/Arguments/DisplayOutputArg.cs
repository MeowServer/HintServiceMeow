namespace HintServiceMeow.Core.Models.Arguments
{
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Utilities;

    /// <summary>
    /// Provides the data passed to a display output when rendering hints for a player.
    /// </summary>
    public class DisplayOutputArg
    {
        internal DisplayOutputArg(PlayerDisplay playerDisplay, string content, IParameter[] parameters, IEffect[] effects, float duration)
        {
            PlayerDisplay = playerDisplay;
            Content = content;
            Parameters = parameters;
            Effects = effects;
            Duration = duration;
        }

        /// <summary>
        /// Gets the player display that triggered the output.
        /// </summary>
        public PlayerDisplay PlayerDisplay { get; }

        /// <summary>
        /// Gets the formatted hint content string to be rendered.
        /// </summary>
        public string Content { get; }

        public IParameter[] Parameters { get; }

        public IEffect[] Effects { get; }

        public float Duration { get; }
    }
}
