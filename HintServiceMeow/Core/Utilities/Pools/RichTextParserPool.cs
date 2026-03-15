namespace HintServiceMeow.Core.Utilities.Pools
{
    using System.Collections.Concurrent;
    using HintServiceMeow.Core.Interface;
    using HintServiceMeow.Core.Utilities.Parser;

    internal class RichTextParserPool : IPool<RichTextParser>
    {
        private readonly ConcurrentQueue<RichTextParser> richTextParserQueue = new();

        public static RichTextParserPool Instance { get; } = new();

        public RichTextParser Rent()
        {
            if (richTextParserQueue.TryDequeue(out RichTextParser rtp))
                return rtp;

            return new RichTextParser();
        }

        public void Return(RichTextParser parser)
        {
            richTextParserQueue.Enqueue(parser);
        }
    }
}
