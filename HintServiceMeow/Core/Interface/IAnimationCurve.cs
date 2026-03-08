namespace HintServiceMeow.Core.Interface
{
    using HintServiceMeow.Core.Models.UniryAdaptors;

    public interface IAnimationCurve
    {
        KeyFrame[] KeyFrames { get; }

        float Evaluate(float time);
    }
}
