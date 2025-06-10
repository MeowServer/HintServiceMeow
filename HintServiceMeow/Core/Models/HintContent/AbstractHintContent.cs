using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Utilities.Tools;
using System;

namespace HintServiceMeow.Core.Models.HintContent
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
                Logger.Instance.Error(ex);
            }
        }

        public abstract void TryUpdate(ContentUpdateArg ev);

        public abstract string GetText();
    }
}
