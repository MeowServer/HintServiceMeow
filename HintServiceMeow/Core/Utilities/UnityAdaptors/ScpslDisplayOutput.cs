namespace HintServiceMeow.Core.Utilities.UnityAdaptors
{
    using System;
    using Hints;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Arguments;
    using HintServiceMeow.Core.Utilities.Tools;
    using Mirror;

    internal class ScpslDisplayOutput(NetworkConnection connectionToPlayer) : IDisplayOutput
    {
        private readonly NetworkConnection? connectionToPlayer = connectionToPlayer;

        public void ShowHint(DisplayOutputArg ev)
        {
            try
            {
                if (connectionToPlayer is not { isReady: true })
                    return;

                HintParameter[] hintParameters;
                if (ev.Parameters.Length > 0)
                {
                    hintParameters = new HintParameter[ev.Parameters.Length];
                    for (int i = 0; i < ev.Parameters.Length; i++)
                    {
                        hintParameters[i] = ev.Parameters[i].GetScpslHintParameter();
                    }
                }
                else
                {
                    hintParameters = [new StringHintParameter(string.Empty)];
                }

                HintEffect[] hintEffects = new HintEffect[ev.Effects.Length];
                for (int i = 0; i < ev.Effects.Length; i++)
                {
                    hintEffects[i] = ev.Effects[i].GetScpslHintEffect();
                }

                HintMessage hintMessageTemplate = new(new TextHint(ev.Content, hintParameters, hintEffects, ev.Duration));

                connectionToPlayer.Send(hintMessageTemplate);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }
        }
    }
}
