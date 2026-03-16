using System;
using HintServiceMeow.Core.Enum.UnityAdaptor;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.UnityAdaptors;

namespace HintServiceMeow.Tests.Core.Models.Transition
{
    /// <summary>Minimal mock for <see cref="IAnimationCurve"/>.</summary>
    internal sealed class FakeCurve : IAnimationCurve
    {
        private readonly Func<float, float>? evaluator;

        /// <summary>Creates a linear 0→1 curve with two keyframes by default.</summary>
        public FakeCurve(HsmKeyFrame[]? keys = null, Func<float, float>? evaluator = null)
        {
            Keys = keys ?? new[]
            {
                new HsmKeyFrame(time: 0f, value: 0f, inTangent: 1f, outTangent: 1f),
                new HsmKeyFrame(time: 1f, value: 1f, inTangent: 1f, outTangent: 1f),
            };

            this.evaluator = evaluator;
        }

        public HsmWrapMode PreWrapMode { get; set; }

        public HsmWrapMode PostWrapMode { get; set; }

        public HsmKeyFrame[] Keys { get; }

        /// <summary>Default behaviour: linear interpolation (value = t).</summary>
        public float Evaluate(float time) => evaluator != null ? evaluator(time) : time;
    }
}
