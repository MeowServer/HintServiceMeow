using HintServiceMeow.Core.Enum;
using HintServiceMeow.Core.Models.Hints;
using MEC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PluginAPI.Core;
using static HintServiceMeow.Core.Utilities.PlayerDisplay;
using HintServiceMeow.Core.Models;
using System.Collections;
using UnityEngine;
using System.Threading;

namespace HintServiceMeow.Core.Utilities
{
    internal class TaskScheduler
    {
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        private Action _action;

        public readonly TimeSpan Interval;
        public DateTime LastActionTime { get; private set; } = DateTime.MinValue;
        public DateTime NextActionTime { get; private set; } = DateTime.MaxValue;

        public TaskScheduler(TimeSpan interval)
        {
            this.Interval = interval;
            Timing.RunCoroutine(RunTasks());
        }

        public void SetNextAction(Action action)
        {
            _lock.EnterWriteLock();

            try
            {
                NextActionTime = DateTime.MinValue;
                _action = action;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void SetNextAction(Action action, float delay)
        {
            _lock.EnterWriteLock();

            try
            {
                if (NextActionTime - DateTime.Now > TimeSpan.FromSeconds(delay))
                {
                    NextActionTime = DateTime.Now.AddSeconds(delay);
                    _action = action;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
            
        }

        public void ResetCountDown()
        {
            _lock.EnterWriteLock();

            try
            {
                LastActionTime = DateTime.Now;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool IsReadyForNextAction()
        {
            _lock.EnterReadLock();

            try
            {
                return DateTime.Now > LastActionTime + Interval;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        private IEnumerator<float> RunTasks()
        {
            while (true)
            {
                yield return Timing.WaitUntilTrue(() =>
                {
                    _lock.EnterReadLock();

                    try
                    {
                        if (_action == null)
                            return false;

                        if (DateTime.Now < LastActionTime + Interval)
                            return false;

                        if (DateTime.Now < NextActionTime)
                            return false;

                        return true;
                    }
                    finally
                    {
                        _lock.ExitReadLock();
                    }
                });

                try
                {
                    _lock.EnterWriteLock();

                    try
                    {
                        LastActionTime = DateTime.Now;

                        _action?.Invoke();

                        _action = null;
                        NextActionTime = DateTime.MaxValue;
                    }
                    finally
                    {
                        _lock.ExitWriteLock();
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e.ToString());
                }
            }
        }
    }
}
