using System;
using HintServiceMeow.Core.Models.Hints;
using PluginAPI.Core;

namespace HintServiceMeow.Core.Models.HintContent.HintContent
{
    public abstract class AbstractHintContent
    {
        public delegate void UpdateHandler();

        public event UpdateHandler ContentUpdated;

        public void OnUpdated()
        {
            try
            {
                ContentUpdated?.Invoke();
            }
            catch (Exception ex)
            {
                Log.Info(ex.ToString());
            }
        }

        public abstract void TryUpdate(AbstractHint.TextUpdateArg ev);

        public abstract string GetText(AbstractHint.TextUpdateArg ev);
    }
}
