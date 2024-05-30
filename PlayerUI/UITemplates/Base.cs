using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.UITemplates
{
    public abstract class PlayerUITemplateBase
    {
        public enum PlayerUITemplateType
        {
            GeneralHuman,
            Spectator,
            SCP,
            CustomSCP,
            CustomHuman
        }
        public abstract PlayerUITemplateType type { get; }

        public Player player;
        public PlayerDisplay playerDisplay;

        public PlayerUITemplateBase(Player player)
        {
            this.player = player;
            this.playerDisplay = PlayerDisplay.Get(player);

            SetUpTemplate();
        }

        public abstract void UpdateTemplate();

        public abstract void SetUpTemplate();

        public abstract void DestructTemplate();
    }

}
