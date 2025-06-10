using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.Parser;
using System.Collections.Concurrent;

namespace HintServiceMeow.Core.Utilities.Pools
{
    internal class RichTextParserPool : IPool<RichTextParser>
    {
        public static RichTextParserPool Instance { get; } = new RichTextParserPool();

        private readonly ConcurrentQueue<RichTextParser> RichTextParserQueue = new();

        public RichTextParser Rent()
        {
            if (RichTextParserQueue.TryDequeue(out RichTextParser rtp))
                return rtp;

            return new RichTextParser();
        }

        public void Return(RichTextParser parser)
        {
            RichTextParserQueue.Enqueue(parser);
        }
    }
}
