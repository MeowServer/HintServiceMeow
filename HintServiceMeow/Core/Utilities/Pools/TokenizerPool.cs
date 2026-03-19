using System.Collections.Concurrent;
using HintServiceMeow.Core.Interface;
using HintServiceMeow.Core.Utilities.Parser;

namespace HintServiceMeow.Core.Utilities.Pools
{
    internal class TokenizerPool : IPool<Tokenizer>
    {
        public static TokenizerPool Instance { get; } = new TokenizerPool();

        private readonly ConcurrentBag<Tokenizer> pool = new ConcurrentBag<Tokenizer>();

        public Tokenizer Rent()
        {
            if (pool.TryTake(out Tokenizer? tokenizer))
            {
                return tokenizer;
            }
            else
            {
                return new Tokenizer();
            }
        }

        public void Return(Tokenizer tokenizer)
        {
            pool.Add(tokenizer);
        }
    }
}
