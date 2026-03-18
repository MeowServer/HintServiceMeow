namespace HintServiceMeow.Core.Utilities.UnityAdaptors
{
    using System;
    using Hints;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Models.Arguments;
    using HintServiceMeow.Core.Models.UniryAdaptors;
    using HintServiceMeow.Core.Utilities.Tools;
    using Mirror;

    internal class ScpslDisplayOutput(ReferenceHub referenceHub) : IDisplayOutput
    {
        private readonly NetworkConnection? connectionToPlayer = referenceHub.connectionToClient;

        public IScreenResolution ScreenResolution { get; } = new ScpslScreenResolution(referenceHub);

        public void ShowHint(DisplayOutputArg ev)
        {
            try
            {
                if (connectionToPlayer is not { isReady: true })
                    return;

                HintParameter[] hintParameters = new HintParameter[ev.Parameters.Length];
                for (int i = 0; i < ev.Parameters.Length; i++)
                {
                    hintParameters[i] = ev.Parameters[i].GetScpslHintParameter();
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
