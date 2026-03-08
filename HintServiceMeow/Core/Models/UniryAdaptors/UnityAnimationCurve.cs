using System;
using HintServiceMeow.Core.Interface;

namespace HintServiceMeow.Core.Models.UniryAdaptors
{
    public class UnityAnimationCurve : IAnimationCurve, IEquatable<UnityEngine.AnimationCurve>
    {
        private UnityEngine.AnimationCurve curve;
        private KeyFrame[]? keyFramesCache;

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

        public KeyFrame[] KeyFrames
        {
            get
            {
                if (keyFramesCache == null || keyFramesCache.Length != curve.length)
                {
                    keyFramesCache = new KeyFrame[curve.length];
                    var unityKeys = curve.keys;
                    for (int i = 0; i < unityKeys.Length; i++)
                    {
                        UnityEngine.Keyframe keyFrame = unityKeys[i];
                        keyFramesCache[i] = new KeyFrame(keyFrame.time, keyFrame.value, keyFrame.inTangent, keyFrame.outTangent);
                    }
                }

                return keyFramesCache;
            }
        }

        public UnityEngine.Keyframe[] keys
        {
            get => curve.keys;
            set
            {
                curve.keys = value;
                keyFramesCache = null; // Clear Cache
            }
        }

        public UnityEngine.Keyframe this[int index] => curve[index];

        public int length => curve.length;

        public UnityEngine.WrapMode preWrapMode
        {
            get => curve.preWrapMode;
            set => curve.preWrapMode = value;
        }

        public UnityEngine.WrapMode postWrapMode
        {
            get => curve.postWrapMode;
            set => curve.postWrapMode = value;
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

        public override bool Equals(object o)
        {
            return curve.Equals(o);
        }

        public bool Equals(UnityEngine.AnimationCurve other)
        {
            return curve.Equals(other);
        }

        public override int GetHashCode()
        {
            return curve.GetHashCode();
        }

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
    }
}