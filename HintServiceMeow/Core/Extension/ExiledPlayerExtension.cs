namespace HintServiceMeow.Core.Extension
{
#if EXILED
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using HintServiceMeow.Core.Enum;
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
        /// Adds multiple hints to the specified Exiled player's display using the calling assembly as the owner.
        /// </summary>
        /// <param name="player">The Exiled player to show the hint on.</param>
        /// <param name="hints">The hints to add.</param>
        public static void AddHint(this Exiled.API.Features.Player player, IEnumerable<AbstractHint>? hints)
        {
            if (hints is null)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;
            var pd = PlayerDisplay.Get(player);

            foreach (var hint in hints)
                pd.InternalAddHint(groupName, hint);
        }

        /// <summary>
        /// Adds multiple hints to the specified Exiled player's display using the calling assembly as the owner.
        /// </summary>
        /// <param name="player">The Exiled player to show the hint on.</param>
        /// <param name="hints">The hints to add.</param>
        public static void AddHint(this Exiled.API.Features.Player player, params AbstractHint[]? hints)
        {
            if (hints is null || hints.Length == 0)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;
            var pd = PlayerDisplay.Get(player);

            foreach (var hint in hints)
                pd.InternalAddHint(groupName, hint);
        }

        /// <summary>
        /// Only use this if you know what you are doing. Add a hint to a specified group.
        /// </summary>
        /// <param name="player">The Exiled player to show the hint on.</param>
        /// <param name="hint">Hint added.</param>
        /// <param name="groupName">Group the hint will be assigned to.</param>
        public static void AddHint(this Exiled.API.Features.Player player, AbstractHint? hint, string groupName)
        {
            if (hint is null)
                return;

            PlayerDisplay.Get(player).InternalAddHint(groupName, hint);
        }

        /// <summary>
        /// Adds a hint to the player's display and schedules its removal or hiding after the specified duration.
        /// </summary>
        /// <param name="player">The Exiled player to show the hint on.</param>
        /// <param name="hint">The hint to show.</param>
        /// <param name="duration">The duration in seconds before the after-show action is applied.</param>
        /// <param name="afterShow">The action to take after the duration elapses.</param>
        public static void ShowHint(this Exiled.API.Features.Player player, AbstractHint hint, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove)
        {
            if (hint is null)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;
            var pd = PlayerDisplay.Get(player);

            pd.InternalAddHint(groupName, hint);

            switch (afterShow)
            {
                case AfterShowAction.Remove:
                    pd.RemoveAfter(hint, duration);
                    break;
                case AfterShowAction.Hide:
                    hint.HideAfter(duration);
                    break;
            }
        }

        /// <summary>
        /// Adds a collection of hints to the player's display and schedules their removal or hiding after the specified duration.
        /// </summary>
        /// <param name="player">The Exiled player to show the hint on.</param>
        /// <param name="hints">The hints to show.</param>
        /// <param name="duration">The duration in seconds before the after-show action is applied.</param>
        /// <param name="afterShow">The action to take after the duration elapses.</param>
        public static void ShowHint(this Exiled.API.Features.Player player, IEnumerable<AbstractHint> hints, float duration = 7f, AfterShowAction afterShow = AfterShowAction.Remove)
        {
            if (hints is null)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;
            var pd = PlayerDisplay.Get(player);

            foreach (var hint in hints)
            {
                pd.InternalAddHint(groupName, hint);
                switch (afterShow)
                {
                    case AfterShowAction.Remove:
                        pd.RemoveAfter(hint, duration);
                        break;
                    case AfterShowAction.Hide:
                        hint.HideAfter(duration);
                        break;
                }
            }
        }

        /// <summary>
        /// Removes a hint from the specified Exiled player's display.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="hint">The hint to remove.</param>
        public static void RemoveHint(this Exiled.API.Features.Player player, AbstractHint hint) => PlayerDisplay.Get(player).InternalRemoveHint(Assembly.GetCallingAssembly().FullName, hint);

        /// <summary>
        /// Removes a list of hints from the specified Exiled player's display.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="hints">The hints to remove.</param>
        public static void RemoveHint(this Exiled.API.Features.Player player, IEnumerable<AbstractHint>? hints)
        {
            if (hints is null)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;
            var pd = PlayerDisplay.Get(player);
            foreach (var hint in hints)
                pd.InternalRemoveHint(groupName, hint);
        }

        /// <summary>
        /// Removes a list of hints from the specified Exiled player's display.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="hints">The hints to remove.</param>
        public static void RemoveHint(this Exiled.API.Features.Player player, params AbstractHint[]? hints)
        {
            if (hints is null || hints.Length == 0)
                return;

            string groupName = Assembly.GetCallingAssembly().FullName;
            var pd = PlayerDisplay.Get(player);
            foreach (var hint in hints)
                pd.InternalRemoveHint(groupName, hint);
        }

        /// <summary>
        /// Only use this if you know what you are doing. Removes the specified hint from the given group.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="hint">The hint to remove. Ignored if <see langword="null"/>.</param>
        /// <param name="groupName">The group name to remove the hint from.</param>
        public static void RemoveHint(this Exiled.API.Features.Player player, AbstractHint? hint, string groupName)
        {
            if (hint is null)
                return;

            PlayerDisplay.Get(player).InternalRemoveHint(groupName, hint);
        }

        /// <summary>
        /// Removes all hints with the specified identifier from the calling assembly's group.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="id">The identifier of the hints to remove.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public static void RemoveHint(this Exiled.API.Features.Player player, string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("A null or empty string had been passed to RemoveHint");

            PlayerDisplay.Get(player).InternalRemoveHint(Assembly.GetCallingAssembly().FullName, id);
        }

        /// <summary>
        /// Removes the hint with the specified <see cref="Guid"/> from the calling assembly's group.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="id">The unique identifier of the hint to remove.</param>
        public static void RemoveHint(this Exiled.API.Features.Player player, Guid id) =>
            PlayerDisplay.Get(player).InternalRemoveHint(Assembly.GetCallingAssembly().FullName, id);

        /// <summary>
        /// Removes all hints registered by the calling assembly.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        public static void ClearHint(this Exiled.API.Features.Player player) =>
            PlayerDisplay.Get(player).InternalClearHint(Assembly.GetCallingAssembly().FullName);

        /// <summary>
        /// Returns the first hint registered by the calling assembly that matches the specified identifier.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="id">The identifier to search for.</param>
        /// <returns>The matching <see cref="AbstractHint"/>, or <see langword="null"/> if none is found.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public static AbstractHint? GetHint(this Exiled.API.Features.Player player, string? id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("A null or empty string had been passed to GetHint");

            return PlayerDisplay.Get(player).InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id).FirstOrDefault();
        }

        /// <summary>
        /// Returns the first hint registered by the calling assembly that matches the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="guid">The unique identifier to search for.</param>
        /// <returns>The matching <see cref="AbstractHint"/>, or <see langword="null"/> if none is found.</returns>
        public static AbstractHint? GetHint(this Exiled.API.Features.Player player, Guid guid) =>
            PlayerDisplay.Get(player).InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Guid == guid).FirstOrDefault();

        /// <summary>
        /// Returns all hints registered by the calling assembly that match the specified identifier.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="id">The identifier to filter hints by.</param>
        /// <returns>An enumerable sequence of matching hints.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public static IEnumerable<AbstractHint> GetHints(this Exiled.API.Features.Player player, string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("A null or empty string had been passed to GetHints");

            return PlayerDisplay.Get(player).InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id);
        }

        /// <summary>
        /// Returns all hints registered by the calling assembly.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <returns>An enumerable sequence of all hints belonging to the calling assembly.</returns>
        public static IEnumerable<AbstractHint> GetHints(this Exiled.API.Features.Player player) =>
            PlayerDisplay.Get(player).InternalGetHints(Assembly.GetCallingAssembly().FullName);

        /// <summary>
        /// Determines whether any hint registered by the calling assembly has the specified identifier.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="id">The identifier to search for.</param>
        /// <returns><see langword="true"/> if a matching hint exists; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public static bool HasHint(this Exiled.API.Features.Player player, string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("A null or empty string had been passed to HasHint");

            return PlayerDisplay.Get(player).InternalGetHints(Assembly.GetCallingAssembly().FullName, hint => hint.Id == id).Any();
        }

        /// <summary>
        /// Determines whether any hint registered by the calling assembly matches the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="guid">The unique identifier to search for.</param>
        /// <returns><see langword="true"/> if a matching hint exists; otherwise <see langword="false"/>.</returns>
        public static bool HasHint(this Exiled.API.Features.Player player, Guid guid) =>
            PlayerDisplay.Get(player).InternalGetHints(Assembly.GetCallingAssembly().FullName, hint => hint.Guid == guid).Any();

        /// <summary>
        /// Attempts to retrieve the first hint registered by the calling assembly that matches the specified identifier.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="id">The identifier to search for.</param>
        /// <param name="hint">When this method returns, contains the matching hint, or <see langword="null"/> if none was found.</param>
        /// <returns><see langword="true"/> if a matching hint was found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
#nullable disable
        public static bool TryGetHint(this Exiled.API.Features.Player player, string id, out AbstractHint hint)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("A null or empty string had been passed to TryGetHint");

            hint = PlayerDisplay.Get(player).InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id).FirstOrDefault();
            return hint != null;
        }

        /// <summary>
        /// Attempts to retrieve the first hint registered by the calling assembly that matches the specified <see cref="Guid"/>.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="guid">The unique identifier to search for.</param>
        /// <param name="hint">When this method returns, contains the matching hint, or <see langword="null"/> if none was found.</param>
        /// <returns><see langword="true"/> if a matching hint was found; otherwise <see langword="false"/>.</returns>
        public static bool TryGetHint(this Exiled.API.Features.Player player, Guid guid, out AbstractHint hint)
        {
            hint = PlayerDisplay.Get(player).InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Guid == guid).FirstOrDefault();
            return hint != null;
        }
#nullable restore

        /// <summary>
        /// Attempts to retrieve all hints registered by the calling assembly that match the specified identifier.
        /// </summary>
        /// <param name="player">The Exiled player whose hint is removed.</param>
        /// <param name="id">The identifier to search for.</param>
        /// <param name="hints">When this method returns, contains the matching hints.</param>
        /// <returns><see langword="true"/> if at least one matching hint was found; otherwise <see langword="false"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="id"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="id"/> is an empty string.</exception>
        public static bool TryGetHints(this Exiled.API.Features.Player player, string? id, out IEnumerable<AbstractHint> hints)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("A null or empty string had been passed to TryGetHints");

            hints = PlayerDisplay.Get(player).InternalGetHints(Assembly.GetCallingAssembly().FullName, x => x.Id == id);
            return hints.Any();
        }
    }
#endif
}