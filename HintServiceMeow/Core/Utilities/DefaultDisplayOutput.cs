using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Utilities.Tools;

using System;

namespace HintServiceMeow.Core.Utilities
{
    internal class DefaultDisplayOutput : IDisplayOutput
    {
        private readonly Hints.HintMessage _hintMessageTemplate = new Hints.HintMessage(new Hints.TextHint("", new Hints.HintParameter[] { new Hints.StringHintParameter("") }, new Hints.HintEffect[] { Hints.HintEffectPresets.TrailingPulseAlpha(1, 1, 1) }, 99999f));

        public void ShowHint(DisplayOutputArg ev)
        {
            try
            {
                if (ev.PlayerDisplay.ConnectionToClient is null || !ev.PlayerDisplay.ConnectionToClient.isReady)
                    return;

                ((Hints.TextHint)_hintMessageTemplate.Content).Text = ev.Content;
                ev.PlayerDisplay.ConnectionToClient.Send(_hintMessageTemplate);
            }
            catch (Exception ex)
            {
                LogTool.Error(ex);
            }
        }
    }
}
