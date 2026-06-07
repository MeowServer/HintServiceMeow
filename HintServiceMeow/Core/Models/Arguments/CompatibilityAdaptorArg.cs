namespace HintServiceMeow.Core.Models.Arguments
{
    using System.Collections.Generic;
    using global::Hints;

    /// <summary>
    /// Provides data passed to an <see cref="ICompatibilityAdaptor"/> when sending a hint to a player.
    /// </summary>
    public class CompatibilityAdaptorArg
    {
        internal CompatibilityAdaptorArg(string assemblyName, string? content, float duration, IReadOnlyList<HintParameter>? parameters = null)
        {
            AssemblyName = assemblyName;
            Content = content;
            Duration = duration;
            Parameters = parameters;
        }

        /// <summary>
        /// Gets the name of the assembly that registered the hint.
        /// </summary>
        public string AssemblyName { get; }

        /// <summary>
        /// Gets the formatted hint content string to display, or <see langword="null"/> if there is no content.
        /// </summary>
        public string? Content { get; }

        /// <summary>
        /// Gets the duration in seconds for which the hint should be displayed.
        /// </summary>
        public float Duration { get; }

        /// <summary>
        /// Gets the native hint parameters (e.g. keybinds, item icons) referenced by placeholders (<c>{0}</c>, <c>{1}</c>, ...) in <see cref="Content"/>, or <see langword="null"/> if there are none.
        /// </summary>
        public IReadOnlyList<HintParameter>? Parameters { get; }
    }
}
