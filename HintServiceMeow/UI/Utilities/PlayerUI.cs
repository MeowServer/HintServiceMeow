namespace HintServiceMeow.UI.Utilities
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using HintServiceMeow.Core.Utilities;

    /// <summary>
    /// Aggregates the display and UI components for a specific player, providing access to common hints and the underlying <see cref="PlayerDisplay"/>.
    /// </summary>
    public class PlayerUI : IDisposable
    {
        private static readonly HashSet<PlayerUI> PlayerUIList = [];

        #region Constructor

        private PlayerUI(ReferenceHub referenceHub)
        {
            // Initialize references
            ReferenceHub = referenceHub;
            PlayerDisplay = PlayerDisplay.Get(referenceHub);

            // Initialize Components
            CommonHint = new CommonHint(referenceHub);

            // this.Style = new Style(referenceHub);
            // Add to list
            PlayerUIList.Add(this);
        }
        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="global::ReferenceHub"/> this UI instance is bound to.
        /// </summary>
        public ReferenceHub ReferenceHub { get; }

        /// <summary>
        /// Gets the <see cref="Core.Utilities.PlayerDisplay"/> associated with this player UI.
        /// </summary>
        public PlayerDisplay PlayerDisplay { get; }

        /// <summary>
        /// Gets the <see cref="Utilities.CommonHint"/> component used to display pre-configured common hints.
        /// </summary>
        public CommonHint CommonHint { get; }
        #endregion

        #region Methods

        /// <summary>
        /// Gets or creates the <see cref="PlayerUI"/> instance for the specified reference hub.
        /// </summary>
        /// <param name="referenceHub">The reference hub whose UI is retrieved or created.</param>
        /// <returns>The <see cref="PlayerUI"/> for the given reference hub.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="referenceHub"/> is <see langword="null"/>.</exception>
        public static PlayerUI Get(ReferenceHub referenceHub)
        {
            if (referenceHub is null)
                throw new System.ArgumentNullException(nameof(referenceHub));

            PlayerUI? ui = PlayerUIList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return ui ?? new PlayerUI(referenceHub);
        }

        /// <summary>
        /// Gets or creates the <see cref="PlayerUI"/> instance for the specified LabApi player.
        /// </summary>
        /// <param name="player">The LabApi player whose UI is retrieved or created.</param>
        /// <returns>The <see cref="PlayerUI"/> for the given player.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="player"/> is <see langword="null"/>.</exception>
        public static PlayerUI Get(LabApi.Features.Wrappers.Player player)
        {
            if (player is null)
                throw new System.ArgumentNullException(nameof(player));

            return Get(player.ReferenceHub);
        }

#if EXILED
        /// <summary>
        /// Gets or creates the <see cref="PlayerUI"/> instance for the specified Exiled player.
        /// </summary>
        /// <param name="player">The Exiled player whose UI is retrieved or created.</param>
        /// <returns>The <see cref="PlayerUI"/> for the given player.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown when <paramref name="player"/> is <see langword="null"/>.</exception>
        public static PlayerUI Get(Exiled.API.Features.Player player)
        {
            if (player is null)
                throw new System.ArgumentNullException(nameof(player));

            return Get(player.ReferenceHub);
        }
#endif
        #endregion

        #region Destructor Methods
        void IDisposable.Dispose()
        {
            // Destruct Components
            ((IDisposable)CommonHint).Dispose();
        }

        internal static void Destruct(ReferenceHub referenceHub)
        {
            // Get player UI
            PlayerUI? ui = PlayerUIList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            if (ui == null)
                return;

            ((IDisposable)ui).Dispose();

            // Remove from list
            PlayerUIList.Remove(ui);
        }

        internal static void ClearInstance()
        {
            // Destruct Components
            foreach (PlayerUI ui in PlayerUIList)
            {
                ((IDisposable)ui.CommonHint).Dispose();
            }

            // Clear the list
            PlayerUIList.Clear();
        }
        #endregion
    }
}
