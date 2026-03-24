namespace HintServiceMeow.Core.Utilities.Pools
{
    using HintServiceMeow.Core.Utilities.Parser;

    internal class TokenizerPool : PoolBase<Tokenizer>
    {
        public static TokenizerPool Instance { get; } = new TokenizerPool();

        protected override void Reset(Tokenizer item)
        {
        }

        protected override Tokenizer Create() => new Tokenizer();
    }
}
