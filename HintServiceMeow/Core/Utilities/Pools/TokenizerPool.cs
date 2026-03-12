using System.Collections.Concurrent;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.Parser;

namespace HintServiceMeow.Core.Utilities.Pools
{
    internal class TokenizerPool : IPool<RichTextTokenizer>
    {
        public static TokenizerPool Instance { get; } = new TokenizerPool();

        private readonly ConcurrentBag<RichTextTokenizer> pool = new ConcurrentBag<RichTextTokenizer>();

        public RichTextTokenizer Rent()
        {
            if (pool.TryTake(out RichTextTokenizer? tokenizer))
            {
                return tokenizer;
            }
            else
            {
                return new RichTextTokenizer();
            }
        }

        public void Return(RichTextTokenizer tokenizer)
        {
            pool.Add(tokenizer);
        }
    }
}
