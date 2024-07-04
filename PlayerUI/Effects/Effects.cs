using Exiled.API.Features;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HintServiceMeow.Effect
{
    /// <summary>
    /// Not implemented yet
    /// </summary>
    public interface IUIEffect
    {
        void UpdateEffect();
        void SetEffect();
        void DestructEffect();
    }

    /// <summary>
    /// Not implemented yet
    /// </summary>
    public abstract class UIEffectBase : IUIEffect
    {
        protected Player player => PlayerDisplay.player;
        protected PlayerDisplay PlayerDisplay;

        protected float Intensity = 100f;

        protected UIEffectBase()
        {
        }

        protected UIEffectBase(Player player)
        {
            this.PlayerDisplay = PlayerDisplay.Get(player);
        }

        protected UIEffectBase(PlayerDisplay playerDisplay)
        {
            this.PlayerDisplay = playerDisplay;
        }

        public abstract void UpdateEffect();
        public abstract void SetEffect();
        public abstract void DestructEffect();
    }
}
