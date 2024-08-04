using System.Collections.Generic;
using System.Linq;

//Exiled
using Exiled.API.Features;
using HintServiceMeow.Core.Utilities;
using HintServiceMeow.UI.Models;

namespace HintServiceMeow.UI.Utilities
{
    public class PlayerUI
    {
        private static readonly HashSet<PlayerUI> PlayerUIList = new HashSet<PlayerUI>();

        public readonly ReferenceHub ReferenceHub;
        public readonly PlayerDisplay PlayerDisplay;

        public CommonHint CommonHint { get; private set; }

        #region Constructor and Destructors Methods

        private PlayerUI(ReferenceHub referenceHub)
        {
            //Initialize references
            this.ReferenceHub = referenceHub;
            this.PlayerDisplay = PlayerDisplay.Get(referenceHub);

            //Initialize Components
            CommonHint = new CommonHint(referenceHub);

            //Add to list
            PlayerUIList.Add(this);
        }

        internal static PlayerUI TryCreate(ReferenceHub referenceHub)
        {
            var ui = PlayerUIList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return ui ?? new PlayerUI(referenceHub);
        }

        internal static void Destruct(ReferenceHub referenceHub)
        {
            //Get player UI
            var ui = Get(referenceHub);

            //Destruct Components
            ui.CommonHint.Destruct();

            //Remove from list
            PlayerUIList.Remove(ui);
        }

        internal static void ClearInstance()
        {
            //Destruct Components
            foreach (var ui in PlayerUIList)
            {
                ui.CommonHint.Destruct();
            }

            //Clear the list
            PlayerUIList.Clear();
        }

        #endregion

        public static PlayerUI Get(ReferenceHub referenceHub)
        {
            var ui = PlayerUIList.FirstOrDefault(x => x.ReferenceHub == referenceHub);

            return ui ?? new PlayerUI(referenceHub);
        }

        public static PlayerUI Get(Player player)
        {
            return Get(player.ReferenceHub);
        }
    }
}
