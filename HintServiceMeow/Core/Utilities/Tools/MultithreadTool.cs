using Exiled.API.Features;
using MEC;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace HintServiceMeow.Core.Utilities.Tools
{
    /// <summary>
    /// Used to synchronize the action into the main thread
    /// </summary>
    internal class MultithreadTool
    {
        private static ConcurrentQueue<Action> _actionQueue = new ConcurrentQueue<Action>();

        static MultithreadTool()
        {
            Timing.RunCoroutine(CoroutineMethod());
        }

        /// <summary>
        /// Enqueue the action and invoke it in next tick of the main thread
        /// </summary>
        public static void EnqueueAction(Action action)
        {
            _actionQueue.Enqueue(action);
        }

        private static IEnumerator<float> CoroutineMethod()
        {
            while (true)
            {
                yield return Timing.WaitForOneFrame;

                while (_actionQueue.TryDequeue(out var action))
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch(Exception e)
                    {
                        Log.Error(e);
                    }
                }
            }
        }
    }
}
