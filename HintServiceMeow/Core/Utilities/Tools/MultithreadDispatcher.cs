using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Utilities.Tools
{
    internal class MultithreadDispatcher
    {
        public static MultithreadDispatcher Instance { get; private set; } = new MultithreadDispatcher(Environment.ProcessorCount);

        private readonly BlockingCollection<ITaskPatch> _taskQueue = new();
        private readonly List<Task> _workers = new();

        public MultithreadDispatcher(int workerCount)
        {
            for (; workerCount > 0; workerCount--)
            {
                _workers.Add(Task.Run(WorkerMethod));
            }
        }

        private async Task WorkerMethod()
        {
            foreach (var task in _taskQueue.GetConsumingEnumerable())
            {
                try
                {
                    await task.ExecuteAsync();
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
            }
        }

        public void Enqueue(Func<Task> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var wrapper = new TaskPatch(task);
            _taskQueue.Add(wrapper);
        }

        public Task<T> Enqueue<T>(Func<Task<T>> task)
        {
            if (task == null)
                throw new ArgumentNullException(nameof(task));

            var wrapper = new TaskPatch<T>(task);
            _taskQueue.Add(wrapper);
            return wrapper.Completion.Task;
        }

        private interface ITaskPatch
        {
            Task ExecuteAsync();
        }

        private class TaskPatch<T> : ITaskPatch
        {
            public Func<Task<T>> Task { get; }
            public TaskCompletionSource<T> Completion { get; }

            public TaskPatch(Func<Task<T>> task)
            {
                Task = task ?? throw new ArgumentNullException(nameof(task));
                Completion = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
            }

            public async Task ExecuteAsync()
            {
                try
                {
                    var result = await Task();
                    Completion.SetResult(result);
                }
                catch (Exception ex)
                {
                    Completion.SetException(ex);
                }
            }
        }

        private class TaskPatch : ITaskPatch
        {
            public Func<Task> Task { get; }

            public TaskPatch(Func<Task> task)
            {
                Task = task ?? throw new ArgumentNullException(nameof(task));
            }

            public async Task ExecuteAsync()
            {
                try
                {
                    await Task();
                }
                catch (Exception ex)
                {
                    LogTool.Error(ex);
                }
            }
        }
    }
}
