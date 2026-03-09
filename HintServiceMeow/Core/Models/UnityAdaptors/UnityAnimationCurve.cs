namespace HintServiceMeow.Core.Models.UnityAdaptors
{
    using System;
    using HintServiceMeow.Core.Enum.UnityAdaptor;
    using HintServiceMeow.Core.Extension;
    using HintServiceMeow.Core.Interface;

    /// <summary>
    /// Represents an animation curve that encapsulates Unity's AnimationCurve, providing methods to evaluate, create,
    /// and manipulate keyframes and curve behavior.
    /// </summary>
    /// <remarks>Use this class to define and modify animation curves for interpolating values over time, such
    /// as in animation systems or procedural motion. The curve supports different wrap modes and allows for adding,
    /// moving, and removing keyframes. Changes to the curve automatically update the internal cache of keyframes. This
    /// class is intended for scenarios where Unity's AnimationCurve functionality needs to be accessed or extended in a
    /// type-safe and convenient manner.</remarks>
    public class UnityAnimationCurve : IAnimationCurve, IEquatable<UnityEngine.AnimationCurve>, IEquatable<UnityAnimationCurve>
    {
        private UnityEngine.AnimationCurve curve;
        private HsmKeyFrame[]? keyFramesCache;

        public UnityAnimationCurve(UnityEngine.AnimationCurve curve)
        {
            this.curve = curve ?? throw new ArgumentNullException(nameof(curve));
        }

        public UnityAnimationCurve(params UnityEngine.Keyframe[] keys)
        {
            this.curve = new UnityEngine.AnimationCurve(keys);
        }

        public UnityAnimationCurve()
        {
            this.curve = new UnityEngine.AnimationCurve();
        }

        HsmKeyFrame[] IAnimationCurve.Keys
        {
            get
            {
                if (keyFramesCache == null || keyFramesCache.Length != curve.length)
                {
                    keyFramesCache = new HsmKeyFrame[curve.length];
                    var unityKeys = curve.keys;
                    for (int i = 0; i < unityKeys.Length; i++)
                    {
                        UnityEngine.Keyframe keyFrame = unityKeys[i];
                        keyFramesCache[i] = new HsmKeyFrame(keyFrame.time, keyFrame.value, keyFrame.inTangent, keyFrame.outTangent);
                    }
                }

                return keyFramesCache;
            }
        }

        HsmWrapMode IAnimationCurve.PreWrapMode
        {
            get => curve.preWrapMode.ToHsmWrapMode();
            set => curve.preWrapMode = value.ToUnityWarpMode();
        }

        HsmWrapMode IAnimationCurve.PostWrapMode
        {
            get => curve.postWrapMode.ToHsmWrapMode();
            set => curve.postWrapMode = value.ToUnityWarpMode();
        }

        public UnityEngine.Keyframe[] Keys
        {
            get => curve.keys;
            set
            {
                curve.keys = value;
                keyFramesCache = null; // Clear Cache
            }
        }

        public int Length => curve.length;

        public UnityEngine.WrapMode PreWrapMode
        {
            get => curve.preWrapMode;
            set => curve.preWrapMode = value;
        }

        public UnityEngine.WrapMode PostWrapMode
        {
            get => curve.postWrapMode;
            set => curve.postWrapMode = value;
        }

        public UnityEngine.Keyframe this[int index] => curve[index];

        public static implicit operator UnityAnimationCurve(UnityEngine.AnimationCurve c)
            => new UnityAnimationCurve(c);

        public static explicit operator UnityEngine.AnimationCurve(UnityAnimationCurve c)
            => c.curve;

        public static UnityAnimationCurve Constant(float timeStart, float timeEnd, float value)
        {
            return new UnityAnimationCurve(UnityEngine.AnimationCurve.Constant(timeStart, timeEnd, value));
        }

        public static UnityAnimationCurve Linear(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            return new UnityAnimationCurve(UnityEngine.AnimationCurve.Linear(timeStart, valueStart, timeEnd, valueEnd));
        }

        public static UnityAnimationCurve EaseInOut(float timeStart, float valueStart, float timeEnd, float valueEnd)
        {
            return new UnityAnimationCurve(UnityEngine.AnimationCurve.EaseInOut(timeStart, valueStart, timeEnd, valueEnd));
        }

        float IAnimationCurve.Evaluate(float time)
        {
            return curve.Evaluate(time);
        }

        public float Evaluate(float time)
        {
            return curve.Evaluate(time);
        }

        public int AddKey(float time, float value)
        {
            keyFramesCache = null; // Clear Cache
            return curve.AddKey(time, value);
        }

        public int AddKey(UnityEngine.Keyframe key)
        {
            keyFramesCache = null; // Clear Cache
            return curve.AddKey(key);
        }

        public int MoveKey(int index, UnityEngine.Keyframe key)
        {
            keyFramesCache = null; // Clear Cache
            return curve.MoveKey(index, key);
        }

        public void RemoveKey(int index)
        {
            keyFramesCache = null; // Clear Cache
            curve.RemoveKey(index);
        }

        public void ClearKeys()
        {
            keyFramesCache = null; // Clear Cache
            curve.ClearKeys();
        }

        public void SmoothTangents(int index, float weight)
        {
            curve.SmoothTangents(index, weight);
            keyFramesCache = null; // Clear Cache
        }

        public void CopyFrom(UnityEngine.AnimationCurve other)
        {
            curve.CopyFrom(other);
            keyFramesCache = null; // Clear Cache
        }

        public bool Equals(UnityEngine.AnimationCurve other)
        {
            return curve.Equals(other);
        }

        public bool Equals(UnityAnimationCurve other)
        {
            return curve.Equals(other.curve);
        }

        public override int GetHashCode()
        {
            return curve.GetHashCode();
        }
    }
}