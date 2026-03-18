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

        public event PropertyChangedEventHandler PropertyChanged;

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
                if (!coroutineHandle.IsRunning)
                {
                    coroutineHandle = Timing.RunCoroutine(CoroutineMethod());
                }

                Instances.Add(this);
            }
        }

        public float XyRatio => xyRatio;

        private static IEnumerator<float> CoroutineMethod()
        {
            while (true)
            {
                List<ScpslScreenResolution> updatedInstances = new();

                lock (StaticStatusLock)
                {
                    if (Instances.Count == 0)
                        yield break;

                    Instances.RemoveAll(x => x.referenceHub == null);

                    foreach (ScpslScreenResolution resolution in Instances)
                    {
                        if (resolution.xScreenEdge != resolution.referenceHub!.aspectRatioSync.XScreenEdge)
                        {
                            resolution.xScreenEdge = resolution.referenceHub.aspectRatioSync.XScreenEdge;
                            resolution.xyRatio = Mathf.Tan(resolution.xScreenEdge * Mathf.Deg2Rad) / Mathf.Tan(yScreenEdge * Mathf.Deg2Rad);
                            updatedInstances.Add(resolution);
                        }
                    }
                }

                foreach (ScpslScreenResolution resolution in updatedInstances)
                {
                    resolution.PropertyChanged?.Invoke(resolution, new PropertyChangedEventArgs(nameof(XyRatio)));
                }

                yield return Timing.WaitForSeconds(1f);
            }
        }
    }
}
