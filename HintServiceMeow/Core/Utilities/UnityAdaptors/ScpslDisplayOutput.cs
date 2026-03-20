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

        public void ShowHint(DisplayOutputArg arg)
        {
            try
            {
                Logger.Instance.Debug($"[ScpslDisplayOutput] Trying to show hint to player {referenceHub.PlayerId} (X/Y: {ScreenResolution.XyRatio}) with content: {arg.Content}");

                if (connectionToPlayer is not { isReady: true })
                    return;

                Logger.Instance.Debug($"[ScpslDisplayOutput] Player {referenceHub.PlayerId} is ready to receive messages. Proceeding to send hint.");

                HintParameter[] hintParameters;
                if (arg.Parameters.Length > 0)
                {
                    hintParameters = new HintParameter[arg.Parameters.Length];
                    for (int i = 0; i < arg.Parameters.Length; i++)
                    {
                        hintParameters[i] = arg.Parameters[i].GetScpslHintParameter();
                    }
                }
                else
                {
                    hintParameters = [new StringHintParameter(string.Empty)];
                }

                HintEffect[] hintEffects = new HintEffect[arg.Effects.Length];
                for (int i = 0; i < arg.Effects.Length; i++)
                {
                    hintEffects[i] = arg.Effects[i].GetScpslHintEffect();
                }

                HintMessage hintMessage = new(new TextHint(arg.Content, hintParameters, hintEffects, arg.Duration));
                connectionToPlayer.Send(hintMessage);

                Logger.Instance.Debug($"[ScpslDisplayOutput] Hint sent to player {referenceHub.PlayerId} successfully.");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex);
            }
        }
    }
}
