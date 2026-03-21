using System.Text;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Models.Hints;
using HintServiceMeow.Core.Utilities.Parser;
using HintServiceMeow.Core.Utilities.Tools;
using HintServiceMeow.Core.Utilities.UnityAdaptors;

namespace HintServiceMeow.Core.Utilities
{
    internal class ServiceContext
    {
        public static ServiceContext Default { get; set; } = new ServiceContext();

        public ITaskScheduler TaskScheduler { get; } = new TaskScheduler();

        public IMainThreadDispatcher MainThreadDispatcher { get; } = new UnityMainThreadDispatcher();

        public ICoroutineRunner CoroutineRunner { get; } = new UnityCoroutineRunner();

        public ILogger Logger { get; } = new Logger();

        public IPool<StringBuilder> StringBuilderPool { get; } = global::HintServiceMeow.Core.Utilities.Pools.StringBuilderPool.Instance;

        public IPool<RichTextParser> RichTextParserPool { get; } = global::HintServiceMeow.Core.Utilities.Pools.RichTextParserPool.Instance;

        public IPool<Hint> HintPool { get; } = global::HintServiceMeow.Core.Utilities.Pools.HintPool.Instance;

        public ICoordinateTools CoordinateTools { get; } = new CoordinateTools();
    }
}
