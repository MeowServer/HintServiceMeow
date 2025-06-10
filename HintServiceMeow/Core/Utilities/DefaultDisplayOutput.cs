using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Arguments;
using HintServiceMeow.Core.Utilities.Tools;
using Mirror;
using System;

namespace HintServiceMeow.Core.Utilities
{
    internal class DefaultDisplayOutput : IDisplayOutput
    {
        private readonly NetworkConnection _connectionToPlayer;
        private readonly Hints.HintMessage _hintMessageTemplate = new(new Hints.TextHint("", new Hints.HintParameter[] { new Hints.StringHintParameter("") }, new Hints.HintEffect[] { Hints.HintEffectPresets.TrailingPulseAlpha(1, 1, 1) }, 99999f));

        public DefaultDisplayOutput(NetworkConnection connectionToPlayer)
        {
            _connectionToPlayer = connectionToPlayer ?? throw new ArgumentNullException(nameof(connectionToPlayer), "NetworkConnection cannot be null");
        }

        public void ShowHint(DisplayOutputArg ev)
        {
            try
            {
                if (_connectionToPlayer == null || !_connectionToPlayer.isReady)
                    return;

                ((Hints.TextHint)_hintMessageTemplate.Content).Text = ev.Content;
                _connectionToPlayer.Send(_hintMessageTemplate);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }
        }
    }
}
