namespace HintServiceMeow.Core.Models.UniryAdaptors
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using HintServiceMeow.Core.Interface;
    using MEC;
    using UnityEngine;

    internal class ScpslScreenResolution : IScreenResolution
    {
        private static readonly object StaticStatusLock = new();
        private static readonly List<ScpslScreenResolution> Instances = new();
        private static CoroutineHandle coroutineHandle;

        private static float yScreenEdge = 0;
        private readonly ReferenceHub? referenceHub;
        private volatile float xScreenEdge;
        private volatile float xyRatio;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpslScreenResolution"/> class.
        /// Not Thread Safe.
        /// </summary>
        /// <param name="referenceHub">The ReferenceHub instance that provides aspect ratio synchronization settings for the screen resolution.</param>
        public ScpslScreenResolution(ReferenceHub referenceHub)
        {
            lock (StaticStatusLock)
            {
                if (yScreenEdge == 0)
                {
                    yScreenEdge = AspectRatioSync.YScreenEdge;
                }

                this.referenceHub = referenceHub;

                _ = TryUpdate();

                if (!coroutineHandle.IsRunning)
                {
                    coroutineHandle = Timing.RunCoroutine(CoroutineMethod());
                }

                Instances.Add(this);

                Utilities.Tools.Logger.Instance.Debug($"[ScpslScreenResolution] ScpslScreenResolution object initialized for player {referenceHub.PlayerId}. Current X/Y ratio: {XyRatio}");
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public float XyRatio => xyRatio;

        private static IEnumerator<float> CoroutineMethod()
        {
            Utilities.Tools.Logger.Instance.Debug("[ScpslScreenResolution] Aspect ratio synchronization coroutine started.");

            while (true)
            {
                List<ScpslScreenResolution> updatedInstances = new();

                lock (StaticStatusLock)
                {
                    Instances.RemoveAll(x => x.referenceHub == null);

                    foreach (ScpslScreenResolution resolution in Instances)
                    {
                        if (resolution.TryUpdate())
                            updatedInstances.Add(resolution);
                    }
                }

                foreach (ScpslScreenResolution resolution in updatedInstances)
                {
                    resolution.PropertyChanged?.Invoke(resolution, new PropertyChangedEventArgs(nameof(XyRatio)));
                    Utilities.Tools.Logger.Instance.Debug($"[ScpslScreenResolution] ScpslScreenResolution object for player {resolution.referenceHub!.PlayerId} updated. Current X/Y ratio: {resolution.XyRatio}");
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }

        private bool TryUpdate()
        {
            if (xScreenEdge != referenceHub!.aspectRatioSync.XScreenEdge)
            {
                xScreenEdge = referenceHub.aspectRatioSync.XScreenEdge;
                xyRatio = Mathf.Tan(xScreenEdge * Mathf.Deg2Rad) / Mathf.Tan(yScreenEdge * Mathf.Deg2Rad);

                return true;
            }

            return false;
        }
    }
}
