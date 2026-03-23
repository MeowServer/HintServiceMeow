namespace HintServiceMeow.Core.Utilities.Pools
{
    using System.Collections.Concurrent;
    using HintServiceMeow.Core.Utilities.Parser;

    internal class TokenizerPool : PoolBase<Tokenizer>
    {
        private readonly ConcurrentBag<Tokenizer> pool = new ConcurrentBag<Tokenizer>();

        public static TokenizerPool Instance { get; } = new TokenizerPool();

        protected override void Reset(Tokenizer item)
        {
        }

        protected override Tokenizer Create() => new Tokenizer();
    }
}
