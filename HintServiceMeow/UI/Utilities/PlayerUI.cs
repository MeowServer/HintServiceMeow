using HintServiceMeow.Core.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace HintServiceMeow.UI.Utilities
{
    public class PlayerUI
    {
        private static readonly HashSet<PlayerUI> PlayerUIList = new();

        public ReferenceHub ReferenceHub { get; }
        public PlayerDisplay PlayerDisplay { get; }

        public CommonHint CommonHint { get; }

        #region Constructor and Destructors Methods

        private PlayerUI(ReferenceHub referenceHub)
        {
            //Initialize references
            this.ReferenceHub = referenceHub;
            this.PlayerDisplay = PlayerDisplay.Get(referenceHub);

            //Initialize Components
            CommonHint = new CommonHint(referenceHub);
            //this.Style = new Style(referenceHub);

            //Add to list
            PlayerUIList.Add(this);
        }

        internal static void Destruct(ReferenceHub referenceHub)
        {
            //Get player UI
            PlayerUI ui = PlayerUIList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            if (ui == null)
                return;

            //Destruct Components
            ui.CommonHint.Destruct();

            //Remove from list
            PlayerUIList.Remove(ui);
        }

        internal static void ClearInstance()
        {
            //Destruct Components
            foreach (PlayerUI ui in PlayerUIList)
            {
                ui.CommonHint.Destruct();
            }

            //Clear the list
            PlayerUIList.Clear();
        }

        #endregion

        public static PlayerUI Get(ReferenceHub referenceHub)
        {
            if (referenceHub is null)
                throw new System.ArgumentNullException(nameof(referenceHub));

            PlayerUI ui = PlayerUIList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return ui ?? new PlayerUI(referenceHub);
        }

        public static PlayerUI Get(LabApi.Features.Wrappers.Player player)
        {
            if (player is null)
                throw new System.ArgumentNullException(nameof(player));

            return Get(player.ReferenceHub);
        }

#if EXILED
        public static PlayerUI Get(Exiled.API.Features.Player player)
        {
            if(player is null)
                throw new System.ArgumentNullException(nameof(player));

            return Get(player.ReferenceHub);
        }
#endif
    }
}
