namespace HintServiceMeow.Core.Interface
{
    using System;
    using System.Collections.Generic;
    using HintServiceMeow.Core.Models.Hints;

    /// <summary>
    /// Defines the contract for managing and displaying hints for a specific player.
    /// </summary>
    public interface IPlayerDisplay : IDisposable
    {
        /// <summary>
        /// Gets or sets the parser used to convert hint collections into display messages.
        /// </summary>
        IHintParser HintParser { get; set; }

        /// <summary>
        /// Gets or sets the compatibility adaptor used to send hints to the player.
        /// </summary>
        ICompatibilityAdaptor CompatibilityAdaptor { get; set; }

        /// <summary>
        /// Registers an additional display output target.
        /// </summary>
        /// <param name="output">The display output to add.</param>
        void AddDisplayOutput(IDisplayOutput output);

        /// <summary>
        /// Removes the specified display output target.
        /// </summary>
        /// <param name="output">The display output to remove.</param>
        void RemoveDisplayOutput(IDisplayOutput output);

        /// <summary>
        /// Removes all display output targets of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of display output to remove.</typeparam>
        void RemoveDisplayOutput<T>()
            where T : IDisplayOutput;

        /// <summary>
        /// Adds a hint to the player's display.
        /// </summary>
        /// <param name="hint">The hint to add.</param>
        void AddHint(AbstractHint hint);

        /// <summary>
        /// Removes a hint from the player's display.
        /// </summary>
        /// <param name="hint">The hint to remove.</param>
        void RemoveHint(AbstractHint hint);

        /// <summary>
        /// Removes all hints from the player's display.
        /// </summary>
        void ClearHint();

        /// <summary>
        /// Retrieves all hints that match the specified identifier.
        /// </summary>
        /// <param name="id">The identifier to filter hints by.</param>
        /// <returns>An enumerable sequence of matching hints.</returns>
        IEnumerable<AbstractHint> GetHints(string id);

        /// <summary>
        /// Retrieves all hints currently registered on the player's display.
        /// </summary>
        /// <returns>An enumerable sequence of all registered hints.</returns>
        IEnumerable<AbstractHint> GetHints();

        /// <summary>
        /// Forces an immediate display update.
        /// </summary>
        /// <param name="useFastUpdate">
        /// If <see langword="true"/>, performs a fast update; otherwise uses the normal update path.
        /// </param>
        void ForceUpdate(bool useFastUpdate = false);
    }
}