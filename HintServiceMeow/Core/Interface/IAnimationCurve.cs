namespace HintServiceMeow.Core.Interface
{
    using HintServiceMeow.Core.Enum.UnityAdaptor;
    using HintServiceMeow.Core.Models.UniryAdaptors;

    public interface IAnimationCurve
    {
        HsmKeyFrame[] Keys { get; }

        HsmWrapMode PreWrapMode { get; set; }

        HsmWrapMode PostWrapMode { get; set; }

        float Evaluate(float time);
    }
}
