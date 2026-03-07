namespace HintServiceMeow.Core.Extension
{
#if EXILED
    using System.Reflection;
    using HintServiceMeow.Core.Models.Hints;
    using HintServiceMeow.Core.Utilities;

    /// <summary>
    /// Provides extension methods for Exiled <c>Player</c> to manage hint display via <see cref="PlayerDisplay"/>.
    /// </summary>
    public static class ExiledPlayerExtension
    {
        /// <summary>
        /// Gets the <see cref="PlayerDisplay"/> associated with the specified Exiled player.
        /// </summary>
        /// <param name="player">The Exiled player whose display is retrieved.</param>
        /// <returns>The <see cref="PlayerDisplay"/> for the given player.</returns>
        public static PlayerDisplay GetPlayerDisplay(this Exiled.API.Features.Player player) => PlayerDisplay.Get(player);

        /// <summary>
        /// Adds a hint to the specified Exiled player's display using the calling assembly as the owner.
        /// </summary>
        /// <param name="player">The Exiled player to show the hint on.</param>
        /// <param name="hint">The hint to add.</param>
        public static void AddHint(this Exiled.API.Features.Player player, AbstractHint hint) => PlayerDisplay.Get(player).InternalAddHint(Assembly.GetCallingAssembly().FullName, hint);

        /// <summary>
        /// Removes a hint from the specified Exiled player's display.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="hint">The hint to remove.</param>
        public static void RemoveHint(this Exiled.API.Features.Player player, AbstractHint hint) => PlayerDisplay.Get(player).InternalRemoveHint(Assembly.GetCallingAssembly().FullName, hint);
    }
#endif
}