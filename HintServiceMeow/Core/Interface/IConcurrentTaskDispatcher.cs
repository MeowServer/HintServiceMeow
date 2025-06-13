using System;
using System.Threading.Tasks;

namespace HintServiceMeow.Core.Interface
{
    internal interface IConcurrentTaskDispatcher
    {
        void Enqueue(Func<Task> task);
        Task<T> Enqueue<T>(Func<Task<T>> task);
    }
}
