namespace HintServiceMeow.Core.Interface
{
    using HintServiceMeow.Core.Enum;
    using HintServiceMeow.Core.Models.UnityAdaptors;

    internal interface IAnimationCurveFactory
    {
        IAnimationCurve BuildNormalized(EasingType type);

        IAnimationCurve Build(HsmKeyFrame[] keyframes);
    }
}
