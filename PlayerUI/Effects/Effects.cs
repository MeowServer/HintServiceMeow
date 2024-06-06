using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Effect
{
    //Not implemented yet
    public abstract class UIEffectBase
    {
        Player player;
        PlayerDisplay playerDisplay;

        float intensity = 100f;

        public UIEffectBase()
        {

        }

        public UIEffectBase(Player player)
        {
            this.player = player;
            this.playerDisplay = PlayerDisplay.Get(player);
        }

        public UIEffectBase(PlayerDisplay playerDisplay)
        {
            this.player = playerDisplay.player;
            this.playerDisplay = playerDisplay;
        }

        public abstract void UpdateEffect();
        public abstract void SetEffect();
        public abstract void DestructEffect();
    }

    public class BloodLetterEffect : UIEffectBase
    {
        List<DynamicHint> dynamicHints = new List<DynamicHint>();

        public BloodLetterEffect(Player player) : base(player)
        {

        }

        public override void DestructEffect()
        {

        }

        public override void SetEffect()
        {

        }

        public override void UpdateEffect()
        {

        }
    }
}
