namespace HintServiceMeow.Core.Models.UniryAdaptors
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using HintServiceMeow.Core.Interface;
    using MEC;
    using UnityEngine;

    internal class ScpslScreenResolution : IScreenResolution
    {
        private static float yScreenEdge = 0;
        private AspectRatioSync ratioSync;
        private float xScreenEdge;

        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScpslScreenResolution"/> class.
        /// Not Thread Safe. 
        /// </summary>
        /// <param name="referenceHub">The ReferenceHub instance that provides aspect ratio synchronization settings for the screen resolution.</param>
        public ScpslScreenResolution(ReferenceHub referenceHub)
        {
            if (yScreenEdge == 0)
            {
                yScreenEdge = AspectRatioSync.YScreenEdge;
            }

            ratioSync = referenceHub.aspectRatioSync;
            Timing.RunCoroutine(GetRatioEnumerator());
        }

        public float XyRatio => Mathf.Tan(xScreenEdge * Mathf.Deg2Rad) / Mathf.Tan(yScreenEdge * Mathf.Deg2Rad);

        private IEnumerator<float> GetRatioEnumerator()
        {
            while (true)
            {
                if (xScreenEdge != ratioSync.XScreenEdge)
                {
                    xScreenEdge = ratioSync.XScreenEdge;
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(XyRatio)));
                }

                yield return Timing.WaitForOneFrame;
            }
        }
    }
}
