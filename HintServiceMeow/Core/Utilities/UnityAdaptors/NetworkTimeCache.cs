using System.Collections.Generic;
using System.Threading;
using HintServiceMeow.Core.Interface;
using Mirror;

namespace HintServiceMeow.Core.Utilities.UnityAdaptors
{
    /// <summary>
    /// Thread-safe cache for NetworkTime.time.
    /// Must call Update() from the main thread every frame.
    /// </summary>
    internal static class NetworkTimeCache
    {
        private static double cachedTime;

        /// <summary>
        /// Gets the most recently cached NetworkTime.time value.
        /// Safe to read from any thread.
        /// </summary>
        public static double Time
        {
            get => Volatile.Read(ref cachedTime);
        }

        public static IEnumerator<float> Update()
        {
            while (true)
            {
                Volatile.Write(ref cachedTime, NetworkTime.time);
                yield return 0f;
            }
        }

        public static void Initialize(ICoroutineRunner runner)
        {
            runner.StartCoroutine(Update());
        }
    }
}
