using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;

using System;

namespace HintServiceMeow.Core.Utilities
{
    internal class DefaultDisplayOutput: IDisplayOutput
    {
        private readonly Hints.HintMessage _hintMessageTemplate = new Hints.HintMessage(new Hints.TextHint("", new Hints.HintParameter[] { new Hints.StringHintParameter("") }, new Hints.HintEffect[] { Hints.HintEffectPresets.TrailingPulseAlpha(1, 1, 1) }, float.MaxValue));

        public void ShowHint(DisplayOutputArg ev)
        {
            try
            {
                if (ev.PlayerDisplay.ConnectionToClient == null || !ev.PlayerDisplay.ConnectionToClient.isReady)
                    return;

                ((Hints.TextHint)_hintMessageTemplate.Content).Text = ev.Content;
                ev.PlayerDisplay.ConnectionToClient.Send(_hintMessageTemplate);
            }
            catch (Exception ex)
            {
                PluginAPI.Core.Log.Error(ex.ToString());
            }
        }
    }
}
