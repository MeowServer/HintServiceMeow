using HintServiceMeow.Core.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace HintServiceMeow.UI.Utilities
{
    public class PlayerUI
    {
        private static readonly HashSet<PlayerUI> PlayerUIList = new HashSet<PlayerUI>();

        private object _lock = new object();

        public ReferenceHub ReferenceHub { get; }
        public PlayerDisplay PlayerDisplay { get; }

        public CommonHint CommonHint { get; }

        //public Style Style { get; }

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
            PlayerUI ui = PlayerUIList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return ui ?? new PlayerUI(referenceHub);
        }

#if EXILED
        public static PlayerUI Get(Exiled.API.Features.Player player)
        {
            return Get(player.ReferenceHub);
        }
#endif
    }
}
