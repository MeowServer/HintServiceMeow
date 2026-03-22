using System.Collections.Concurrent;
using HintServiceMeow.Core.Utilities.Parser;

namespace HintServiceMeow.Core.Utilities.Pools
{
    internal class TokenizerPool : PoolBase<Tokenizer>
    {
        public static TokenizerPool Instance { get; } = new TokenizerPool();

        private readonly ConcurrentBag<Tokenizer> pool = new ConcurrentBag<Tokenizer>();

        protected override void Reset(Tokenizer item) { }

        protected override Tokenizer Create() => new Tokenizer();
    }
}